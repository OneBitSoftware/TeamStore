using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using TeamStore.Models;

namespace TeamStore.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> UserHasAccessAsync(int projectId, IProjectsService projectsService, string role);

        Task<bool> UserHasAccessAsync(Project project, string role);

        Task GrantAccessAsync(
            int projectId,
            string azureAdObjectIdentifier,
            string role,
            ApplicationUser grantingUser,
            string remoteIpAddress,
            IProjectsService projectsService
            );

        Task RevokeAccessAsync(
            int projectId,
            string azureAdObjectIdentifier,
            string role,
            ApplicationUser revokingUser,
            string remoteIpAddress,
            IProjectsService projectsService
            );
    }
}
