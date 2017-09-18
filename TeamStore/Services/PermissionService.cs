namespace TeamStore.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using TeamStore.DataAccess;
    using TeamStore.Interfaces;
    using TeamStore.Models;

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
        /// <param name="azureAdObjectIdentifier">The identifier of the identity for which access will be granted to</param>
        /// <param name="role">The role/level of access that will be granted</param>
        /// <param name="grantingUser">The ApplicationUser granting the access</param>
        /// <param name="remoteIpAddress">The IP address of the incoming request</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>A Task object</returns>
        public async Task GrantAccessAsync(
            int projectId,
            string azureAdObjectIdentifier,
            string role,
            ApplicationUser grantingUser,
            string remoteIpAddress,
            IProjectsService projectsService)
        {
            if (string.IsNullOrWhiteSpace(azureAdObjectIdentifier)) throw new ArgumentNullException(nameof(azureAdObjectIdentifier));
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentNullException(nameof(remoteIpAddress));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));
            if (projectId == 0) throw new ArgumentException("You must provide a valid project id.");
            if (grantingUser == null) throw new ArgumentNullException(nameof(grantingUser));

            // Get/find the project
            var project = await projectsService.GetProject(projectId, true);
            if (project == null) throw new ArgumentNullException(nameof(project));

            // Refresh the entity to discard changes and avoid saving a decrypted project
            _dbContext.Entry(project).State = EntityState.Unchanged; // project will be encrypted here

            // Verify current user has permissions to grant access, aka Owner
            if (await CurrentUserHasAccessAsync(projectId, projectsService, "Owner") == false)
            {
                throw new Exception("The current user does not have permissions to grant access.");
            }

            var newAccessIdentifier = new AccessIdentifier();
            newAccessIdentifier.Project = project ?? throw new ArgumentNullException(nameof(project));
            newAccessIdentifier.Role = role;
            newAccessIdentifier.Created = DateTime.UtcNow;
            newAccessIdentifier.CreatedBy = grantingUser ?? throw new ArgumentNullException(nameof(grantingUser));

            // Save Grant event
            await _eventService.StoreGrantAccessEventAsync(projectId, remoteIpAddress, role, azureAdObjectIdentifier, grantingUser);

            // TODO: grant access to AD group
            newAccessIdentifier.Identity = await _applicationIdentityService.EnsureUserAsync(azureAdObjectIdentifier);

            project.AccessIdentifiers.Add(newAccessIdentifier);

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Revokes access to a project
        /// </summary>
        /// <param name="projectId">The Id of the project</param>
        /// <param name="azureAdObjectIdentifier">The identifier of the identity for which access will be granted to</param>
        /// <param name="role">The role/level of access that will be granted</param>
        /// <param name="revokingUser">The ApplicationUser revoking the access</param>
        /// <param name="remoteIpAddress">The IP address of the incoming request</param>
        /// <param name="projectsService">An instance of IProjectService to assist with resolving of the project</param>
        /// <returns>A Task object</returns>
        public async Task RevokeAccessAsync(
            int projectId,
            string azureAdObjectIdentifier,
            string role,
            ApplicationUser revokingUser,
            string remoteIpAddress,
            IProjectsService projectsService)
        {
            if (string.IsNullOrWhiteSpace(azureAdObjectIdentifier)) throw new ArgumentNullException(nameof(azureAdObjectIdentifier));
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentNullException(nameof(remoteIpAddress));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));
            if (projectId == 0) throw new ArgumentException("You must provide a valid project id.");
            if (revokingUser == null) throw new ArgumentNullException(nameof(revokingUser));
           
            // Get/find the project
            var project = await projectsService.GetProject(projectId, true);
            if (project == null) throw new ArgumentNullException(nameof(project));

            // Save Revoke Access event
            await _eventService.LogRevokeAccessEventAsync(projectId, remoteIpAddress, azureAdObjectIdentifier, role, revokingUser);

            // Verify that the current user has permissions to grant access, aka Owner
            if (await CurrentUserHasAccessAsync(projectId, projectsService, "Owner") == false)
            {
                throw new Exception("The current user does not have permissions to revoke access.");
            }

            // Validate that the AzureAD Object Identifier has access
            var existingAccess = _dbContext.AccessIdentifiers.Where(a => 
                a.Project.Id == projectId && 
                a.Identity.AzureAdObjectIdentifier == azureAdObjectIdentifier &&
                a.Role == role);

            // we remove all in case there are two grants with the same role
            foreach (var item in existingAccess)
            {
                project.AccessIdentifiers.Remove(item);
            }

            await _dbContext.SaveChangesAsync();
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
            if (projectsService == null) throw new ArgumentNullException(nameof(projectsService));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));
            var project = await projectsService.GetProject(projectId, true);

            return CurrentUserHasAccess(project, projectsService, role);
        }

        /// <summary>
        /// Checks if the current ApplicationUser has the passed role access to the specified project 
        /// </summary>
        /// <param name="project">The instance of the project to check for access</param>
        /// <param name="projectsService">An instance of IProjectService to resolve projects</param>
        /// <param name="role">The role/level of access to check</param>
        /// <returns>True if the current user has the role access to the specified project</returns>
        public bool CurrentUserHasAccess(Project project, IProjectsService projectsService, string role)
        {
            // Get/find the project
            if (projectsService == null) throw new ArgumentNullException(nameof(projectsService));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));
            var currentUser = _applicationIdentityService.GetCurrentUser();
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
    }
}
