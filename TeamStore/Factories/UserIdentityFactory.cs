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
            newUser.AzureAdNameIdentifier = identity.Name;
            newUser.AzureAdObjectIdentifier = identity.Claims.First(
                item => item.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")
                .Value;
            newUser.TenantId = "";
            // TODO: resolve all claims

            return newUser;
        }
    }
}
