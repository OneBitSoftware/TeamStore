namespace TeamStore.Factories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using TeamStore.Models;

    public static class UserIdentityFactory
    {
        public static ApplicationUser CreateApplicationUserFromAzureIdentity(ClaimsIdentity identity)
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
    }
}
