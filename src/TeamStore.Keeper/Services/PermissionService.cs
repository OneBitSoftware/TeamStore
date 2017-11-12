namespace TeamStore.Keeper.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using TeamStore.Keeper.DataAccess;
    using TeamStore.Keeper.Interfaces;
    using TeamStore.Keeper.Models;

    /// <summary>
    /// The Permission Service deals with granting, revoking and checking access
    /// </summary>
    public class PermissionService : IPermissionService
    {
        // NOTE: we don't inject a IProjectService to avoid circular dependencies, which
        // are a problem for testing. The Integration tests should really use dependency injection rather than instantiation, then
        // this would not be a problem.

        private readonly IEventService _eventService;
        private readonly IGraphService _graphService;
        private readonly IApplicationIdentityService _applicationIdentityService;
        private ApplicationDbContext _dbContext;

        /// <summary>
        /// Constructor for the PermissionService
        /// </summary>
        /// <param name="context">An ApplicationDbContext for data access</param>
        /// <param name="graphService">A GraphService instance for Microsoft Graph communication</param>
        /// <param name="eventService">An instance of the EventService for logging purposes</param>
        /// <param name="applicationIdentityService">An instance of the ApplicationIdentityService to resolve identities</param>
        public PermissionService(
            ApplicationDbContext context,
            IGraphService graphService,
            IEventService eventService,
            IApplicationIdentityService applicationIdentityService)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
            _applicationIdentityService = applicationIdentityService ?? throw new ArgumentNullException(nameof(applicationIdentityService));
            _graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        /// <summary>
        /// Grants access to a project
        /// </summary>
        /// <param name="projectId">The Id of the project</param>
        /// <param name="upn">The UPN identifier of the identity for which access will be granted to</param>
        /// <param name="role">The role/level of access that will be granted</param>
        /// <param name="remoteIpAddress">The IP address of the incoming request</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>A Task object with an AccessChangeResult representing the result</returns>
        public async Task<AccessChangeResult> GrantAccessAsync(
            int projectId,
            string upn,
            string role,
            string remoteIpAddress,
            IProjectsService projectsService)
        {
            if (string.IsNullOrWhiteSpace(upn)) throw new ArgumentNullException(nameof(upn));
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentNullException(nameof(remoteIpAddress));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));
            if (projectId == 0) throw new ArgumentException("You must provide a valid project id.");
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new ArgumentException("The current user could not be retrieved.");

            // Get/find the project
            var project = await projectsService.GetProject(projectId, true);
            if (project == null) throw new ArgumentNullException(nameof(project));

            // Refresh the entity to discard changes and avoid saving a decrypted project
            _dbContext.Entry(project).State = EntityState.Unchanged; // project will be encrypted here

            // Verify current user has permissions to grant access, aka Owner
            if (await CurrentUserHasAccess(project, projectsService, "Owner") == false)
            {
                await _eventService.LogCustomEvent(currentUser.Id.ToString(), $"User {currentUser.Upn} attepted to give access to {upn} without having access to project with ID: {projectId}.");
                throw new Exception("The current user does not have permissions to grant access.");
            }

            // Check if the target user already has access
            if (CheckAccess(project, upn, "Owner", projectsService))
            {
                await _eventService.LogCustomEvent(currentUser.Id.ToString(), $"The user {currentUser.Upn} attepted to give access to {upn} who already has access to project with ID: {projectId}.");
                return new AccessChangeResult() { Success = false, Message = $"User {upn} already has access." }; // no need to grant
            }

            // TODO: grant access to AD group
            var newAccessIdentifier = new AccessIdentifier();
            newAccessIdentifier.Project = project ?? throw new ArgumentNullException(nameof(project));
            newAccessIdentifier.Role = role;
            newAccessIdentifier.Created = DateTime.UtcNow;
            newAccessIdentifier.CreatedBy = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            newAccessIdentifier.Identity = await _applicationIdentityService.EnsureUserByUpnAsync(upn);

            project.AccessIdentifiers.Add(newAccessIdentifier);

            // Validation of the access identifiers before save
            foreach (var item in project.AccessIdentifiers)
            {
                if ((item.Identity == null) || string.IsNullOrWhiteSpace(item.Role))
                {
                    await _eventService.LogCustomEvent(currentUser.Upn, $"Ensure did not return a user for {upn}");
                    return new AccessChangeResult() { Success = false, Message = $"The user or group '{upn}' was not found." };
                }
            }

            // Save Grant event
            await _eventService.LogGrantAccessEventAsync(projectId, remoteIpAddress, role, newAccessIdentifier.Identity.Id, currentUser.Id, "UPN: " + upn);

            await _dbContext.SaveChangesAsync();

            return new AccessChangeResult() { Success = true }; 
        }

        /// <summary>
        /// Revokes access to a project
        /// </summary>
        /// <param name="projectId">The Id of the project</param>
        /// <param name="upn">The UPN of the identity for which access will be revoked</param>
        /// <param name="role">The role/level of access that will be granted</param>
        /// <param name="remoteIpAddress">The IP address of the incoming request</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>A Task object with an AccessChangeResult representing the result</returns>
        public async Task<AccessChangeResult> RevokeAccessAsync(
            int projectId,
            string upn,
            string role,
            string remoteIpAddress,
            IProjectsService projectsService)
        {
            if (string.IsNullOrWhiteSpace(upn)) throw new ArgumentNullException(nameof(upn));
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentNullException(nameof(remoteIpAddress));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));
            if (projectId < 1) throw new ArgumentException("You must provide a valid project id.");
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new ArgumentException("The current user could not be retrieved.");

            // Get/find the project
            var project = await projectsService.GetProject(projectId, true);
            if (project == null) throw new ArgumentNullException(nameof(project));

            // Verify that the current user has permissions to grant access, aka Owner
            if (await CurrentUserHasAccessAsync(projectId, projectsService, "Owner") == false)
            {
                await _eventService.LogCustomEvent(currentUser.Id.ToString(), $"User {currentUser.Upn} attepted to revoke access to {upn} without having access to project with ID: {projectId}.");
                throw new Exception("The current user does not have permissions to revoke access.");
            }

            // Validate that the AzureAD Object Identifier has access
            var existingAccess = _dbContext.AccessIdentifiers.Where(a => 
                a.Project.Id == projectId && 
                ((ApplicationUser)a.Identity).Upn == upn &&
                a.Role == role);

            // we remove all in case there are two grants with the same role
            // NOTE: ToListAsync is required, so a lock on the DB is released for the 
            // Log Event call
            foreach (var item in await existingAccess.ToListAsync())
            {
                project.AccessIdentifiers.Remove(item);

                // Save Revoke Access event
                await _eventService.LogRevokeAccessEventAsync(projectId, remoteIpAddress, item.Id, role, currentUser.Id, "AADObjectId:" + item.Identity.AzureAdObjectIdentifier );

            } // this will not error out when there are no results, which should be OK

            await _dbContext.SaveChangesAsync();

            return new AccessChangeResult() { Success = true };
        }

        /// <summary>
        /// Checks if the current ApplicationUser has the passed role access to the specified project 
        /// </summary>
        /// <param name="projectId">The Id of the project to check for access</param>
        /// <param name="projectsService">An instance of IProjectService to resolve projects</param>
        /// <param name="role">The role/level of access to check</param>
        /// <returns>A Task of bool, True if the current user has the role access to the specified project</returns>
        public async Task<bool> CurrentUserHasAccessAsync(int projectId, IProjectsService projectsService, string role)
        {
            // Get/find the project
            if (projectId < 1) throw new ArgumentException("You must provide a valid project id.");
            if (projectsService == null) throw new ArgumentNullException(nameof(projectsService));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));
            var project = await projectsService.GetProject(projectId, true);

            return await CurrentUserHasAccess(project, projectsService, role);
        }

        /// <summary>
        /// Checks if the current ApplicationUser has the passed role access to the specified project 
        /// </summary>
        /// <param name="projectId">The Id of the project to check for access</param>
        /// <param name="projectsService">An instance of IProjectService to resolve projects</param>
        /// <returns>A Task of bool, True if the current user has the role access to the specified project</returns>
        public async Task<bool> CurrentUserHasAccessAsync(int projectId, IProjectsService projectsService)
        {
            // Get/find the project
            if (projectId < 1) throw new ArgumentException("You must provide a valid project id.");
            if (projectsService == null) throw new ArgumentNullException(nameof(projectsService));
            var project = await projectsService.GetProject(projectId, true);

            return await CurrentUserHasAccess(project, projectsService);
        }

        /// <summary>
        /// Checks if the current ApplicationUser has the passed role access to the specified project 
        /// </summary>
        /// <param name="project">The instance of the project to check for access</param>
        /// <param name="projectsService">An instance of IProjectService to resolve projects</param>
        /// <param name="role">The role/level of access to check</param>
        /// <returns>True if the current user has the role access to the specified project</returns>
        public async Task<bool> CurrentUserHasAccess(Project project, IProjectsService projectsService, string role)
        {
            // Get/find the project
            if (projectsService == null) throw new ArgumentNullException(nameof(projectsService));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new ArgumentNullException(nameof(currentUser));
            if (project == null) throw new ArgumentNullException(nameof(project));

            var accessList = project.AccessIdentifiers.Where(ai =>
                ai.Project == project &&
                ai.Role == role &&
                ai.Identity == currentUser);

            if (accessList.Count() > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Checks if the current ApplicationUser has any access to the specified project 
        /// </summary>
        /// <param name="project">The instance of the project to check for access</param>
        /// <param name="projectsService">An instance of IProjectService to resolve projects</param>
        /// <returns>True if the current user has the role access to the specified project</returns>
        public async Task<bool> CurrentUserHasAccess(Project project, IProjectsService projectsService)
        {
            // Get/find the project
            if (projectsService == null) throw new ArgumentNullException(nameof(projectsService));
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new ArgumentNullException(nameof(currentUser));
            if (project == null) throw new ArgumentNullException(nameof(project));

            var accessList = project.AccessIdentifiers.Where(ai =>
                ai.Project == project &&
                ai.Identity == currentUser);

            if (accessList.Count() > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Checks if an ApplicationUser has the requested role against a project
        /// </summary>
        /// <param name="project">The Project to check</param>
        /// <param name="targetUser">The ApplicationUser to check</param>
        /// <param name="role">The role of interest</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>True if the user has the specified role, false if not.</returns>
        public bool CheckAccess(Project project, ApplicationUser targetUser, string role, IProjectsService projectsService)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (targetUser == null) throw new ArgumentNullException(nameof(targetUser));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));

            var result = project.AccessIdentifiers.Where(ai => 
                ai.Project == project && 
                ai.Identity == targetUser && 
                ai.Role == role).FirstOrDefault();

            if (result != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if an ApplicationUser has the requested role against a project
        /// </summary>
        /// <param name="project">The Project to check</param>
        /// <param name="targetUserUpn">The UPN of the ApplicationUser to check</param>
        /// <param name="role">The role of interest</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>True if the user has the specified role, false if not.</returns>
        public bool CheckAccess(Project project, string targetUserUpn, string role, IProjectsService projectsService)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));
            if (string.IsNullOrWhiteSpace(targetUserUpn)) throw new ArgumentNullException(nameof(targetUserUpn));

            // We probably need to improve this query so it has less casts
            var result = project.AccessIdentifiers.Where(ai =>
                ai.Project == project &&
                string.IsNullOrWhiteSpace(((ApplicationUser)ai.Identity).Upn) == false && // avoid nulls
                ((ApplicationUser)ai.Identity).Upn == targetUserUpn &&
                ai.Role == role).FirstOrDefault();

            if (result != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if an ApplicationUser has the requested role against a project
        /// </summary>
        /// <param name="projectId">The ID of the Project to check</param>
        /// <param name="targetUser">The ApplicationUser to check</param>
        /// <param name="role">The role of interest</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>A task of True if the user has the specified role, false if not.</returns>
        public async Task<bool> CheckAccessAsync(int projectId, ApplicationUser targetUser, string role, IProjectsService projectsService)
        {
            if (targetUser == null) throw new ArgumentNullException(nameof(targetUser));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));
            if (projectId < 1) throw new ArgumentException("You must provide a valid project id.");
            var project = await projectsService.GetProject(projectId, true);

            return CheckAccess(project, targetUser, role, projectsService);
        }
    }
}
