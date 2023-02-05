using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeamStore.Keeper.Models;

namespace TeamStore.Keeper.Interfaces
{
    /// <summary>
    /// Defines the Graph Service contract
    /// </summary>
    public interface IGraphService
    {
        Task<List<ApplicationGroup>> GetGroups(string prefix, string userObjectId);

        Task<ApplicationGroup> GetGroup(string groupObjectIdentifier, string userObjectId);

        Task<List<ApplicationGroup>> GetGroupMembershipForUser(string userId);

        Task<ApplicationUser> ResolveUserByObjectIdAsync(string azureAdObjectIdentifier, string currentUserId);

        Task<ApplicationUser> ResolveUserByUpnAsync(string upn, string currentUserId, CancellationToken cancellationToken);
    }
}
