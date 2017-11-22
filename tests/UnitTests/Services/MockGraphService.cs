using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using TeamStore.Keeper.Interfaces;
using TeamStore.Keeper.Models;
using System.Linq;

namespace UnitTests.Services
{
    public class MockGraphService : IGraphService
    {
        List<ApplicationUser> _usersList;

        public MockGraphService()
        {
            _usersList = new List<ApplicationUser>();

            PopulateUsers();
        }

        public void AddUserToInternalList(ApplicationUser user)
        {
            _usersList.Add(user);
        }

        private void PopulateUsers()
        {
            _usersList.Add(new ApplicationUser()
            {
                Id = 1,
                AzureAdName = "Mock User 1",
                AzureAdNameIdentifier = "AzureAdNameId-1",
                AzureAdObjectIdentifier = "AzureAdObjectId-1",
                DisplayName = "Mock User Display Name 1",
                TenantId = "Mock Tenant Id 1",
                Upn = "mock@upn.com-1"
            });

            _usersList.Add(new ApplicationUser()
            {
                Id = 2,
                AzureAdName = "Mock User 2",
                AzureAdNameIdentifier = "AzureAdNameId-2",
                AzureAdObjectIdentifier = "AzureAdObjectId-2",
                DisplayName = "Mock User Display Name 2",
                TenantId = "Mock Tenant Id 2",
                Upn = "mock@upn.com-2"
            });
        }

        public GraphServiceClient GetAuthenticatedClient(string userId)
        {
            throw new NotImplementedException("Should not be called in integration tests.");
        }

        public Task<ApplicationGroup> GetGroup(string groupObjectIdentifier, string userObjectId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ApplicationGroup>> GetGroupMembershipForUser(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ApplicationGroup>> GetGroups(string prefix, string userObjectId)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticationResult> GetTokenByAuthorizationCodeAsync(string userId, string code, string redirectHost)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser> ResolveUserByObjectIdAsync(string azureAdObjectIdentifier, string currentUserId)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser> ResolveUserByUpnAsync(string upn, string currentUserId)
        {
            var user = _usersList.Where(u => u.Upn == upn).FirstOrDefault();

            return Task.FromResult(user);
        }
    }
}
