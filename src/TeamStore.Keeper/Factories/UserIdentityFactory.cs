namespace TeamStore.Keeper.Factories
{
    using Microsoft.Graph;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using TeamStore.Keeper.Models;

    public static class UserIdentityFactory
    {
        public static ApplicationUser CreateNewApplicationUserFromAzureIdentity(ClaimsIdentity identity)
        {
            var newUser = new ApplicationUser();
            newUser.DisplayName = GetClaimValue("name", identity.Claims);
            newUser.AzureAdNameIdentifier = GetClaimValue(ClaimTypes.NameIdentifier, identity.Claims);
            newUser.AzureAdName = GetClaimValue(ClaimTypes.Name, identity.Claims);
            newUser.Upn = GetClaimValue(ClaimTypes.Upn, identity.Claims);
            newUser.AzureAdObjectIdentifier = GetClaimValue("http://schemas.microsoft.com/identity/claims/objectidentifier", identity.Claims);
            newUser.TenantId = GetClaimValue("http://schemas.microsoft.com/identity/claims/tenantid", identity.Claims);

            return newUser;
        }

        public static string GetClaimValue(string claimType, IEnumerable<Claim> claimsCollection)
        {
            var claimValue = claimsCollection.FirstOrDefault(item => item.Type == claimType);
            if (claimValue == null)
            {
                return null;
            }
            else
            {
                return claimValue.Value;
            }
        }

        public static ApplicationUser MapApplicationUser(User user)
        {
            var newUser = new ApplicationUser();

            newUser.AzureAdObjectIdentifier = user.Id;
            newUser.Upn = user.UserPrincipalName;
            newUser.DisplayName = user.DisplayName;
            // TODO: figure out TenantId and AzureName and Azure Name ID

            return newUser;
        }
    }
}
