using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamStore.Interfaces;
using TeamStore.Models;

namespace TeamStore.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IGraphService _graphService;

        public PermissionService(IGraphService graphService)
        {
            _graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
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
    }
}
