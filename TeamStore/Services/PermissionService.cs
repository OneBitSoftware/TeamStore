namespace TeamStore.Services
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using TeamStore.DataAccess;
    using TeamStore.Factories;
    using TeamStore.Interfaces;
    using TeamStore.Models;

    public class PermissionService : IPermissionService
    {
        private readonly IEventService _eventService;
        private readonly IGraphService _graphService;
        private readonly IApplicationIdentityService _applicationIdentityService;
        private ApplicationDbContext _dbContext;

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
            if (await UserHasAccessAsync(projectId, projectsService, "Owner") == false)
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

        public async Task<bool> UserHasAccessAsync(int projectId, IProjectsService projectsService, string role)
        {
            // Get/find the project
            if (projectsService == null) throw new ArgumentNullException(nameof(projectsService));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));
            var project = await projectsService.GetProject(projectId, true);
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

        public Task<bool> UserHasAccessAsync(Project project, string role)
        {
            throw new NotImplementedException();
        }
    }
}
