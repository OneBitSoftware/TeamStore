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
        public Task GrantAccess(Project project, string principals)
        {
            throw new NotImplementedException();
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
