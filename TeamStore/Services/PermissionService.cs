using Microsoft.AspNetCore.Http;
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

namespace TeamStore.Services
{
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

        public async Task GrantAccess(Project project, string azureAdObjectIdentifier, ApplicationUser grantingUser, string remoteIpAddress)
        {
            if (string.IsNullOrWhiteSpace(azureAdObjectIdentifier)) throw new ArgumentNullException(nameof(azureAdObjectIdentifier));

            var newAccessIdentifier = new AccessIdentifier();
            newAccessIdentifier.Project = project ?? throw new ArgumentNullException(nameof(project));
            newAccessIdentifier.Role = "Edit"; // TODO
            newAccessIdentifier.Created = DateTime.UtcNow;
            newAccessIdentifier.CreatedBy = grantingUser ?? throw new ArgumentNullException(nameof(grantingUser));

            // Save Grant event
            await _eventService.StoreGrantAccessEventAsync(project.Id, remoteIpAddress, "Edit", azureAdObjectIdentifier, grantingUser);

            // TODO: grant access to AD group
            newAccessIdentifier.Identity = _applicationIdentityService.GetUser(azureAdObjectIdentifier);

            project.AccessIdentifiers.Add(newAccessIdentifier);

            await _dbContext.SaveChangesAsync();
        }

        public Task<bool> UserHasAccess(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UserHasAccess(Project project)
        {
            throw new NotImplementedException();
        }
    }
}
