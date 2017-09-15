using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using TeamStore.Factories;
using TeamStore.Interfaces;
using TeamStore.Models;

namespace TeamStore.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IGraphService _graphService;
        private HttpContext _context;

        public PermissionService(IGraphService graphService, IHttpContextAccessor httpContextAccessor)
        {
            _graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
            _context = httpContextAccessor.HttpContext;
        }

        public async Task GrantAccess(Project project, string principals, ApplicationIdentity contextUser)
        {
            var newAccessIdentifier = new AccessIdentifier();
            newAccessIdentifier.Project = project;
            newAccessIdentifier.Role = "Edit"; // TODO
            newAccessIdentifier.Created = DateTime.UtcNow;
            //newAccessIdentifier.CreatedBy = (ApplicationUser)contextUser;

            var newGroup = new ApplicationGroup();

            // Get group object
            await _graphService.GetGroup(principals, contextUser.AzureAdObjectIdentifier);


           // project.AccessIdentifiers.Add

        }

        public Task<bool> UserHasAccess(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UserHasAccess(Project project)
        {
            throw new NotImplementedException();
        }

        public ApplicationUser GetCurrentUser()
        {
            if (_context == null) throw new ArgumentNullException(nameof(_context));
            return GetCurrentUser(_context.User?.Identity);
        }

        public ApplicationUser GetCurrentUser(IIdentity identity)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (identity.IsAuthenticated == false) // TODO needs a better Exception type
                throw new Exception("The current request is not authenticated.");

            return UserIdentityFactory.CreateApplicationUserFromAzureIdentity(identity as ClaimsIdentity);
        }
    }
}
