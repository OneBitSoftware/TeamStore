using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamStore.Keeper.Models;

namespace TeamStore.Keeper.Interfaces
{
    /// <summary>
    /// Defines the Graph Service contract
    /// </summary>
    public interface IGraphService
    {
        GraphServiceClient GetAuthenticatedClient(string userId);

        Task<AuthenticationResult> GetTokenByAuthorizationCodeAsync(string userId, string code, string redirectHost);

        Task<List<ApplicationGroup>> GetGroups(string prefix, string userObjectId);

        Task<ApplicationGroup> GetGroup(string groupObjectIdentifier, string userObjectId);

        Task<List<ApplicationGroup>> GetGroupMembershipForUser(string userId);

        Task<ApplicationUser> ResolveUserAsync(string azureAdObjectIdentifier, string currentUserId);
    }
}
