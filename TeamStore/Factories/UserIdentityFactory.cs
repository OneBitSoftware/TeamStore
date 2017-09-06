using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TeamStore.Models;

namespace TeamStore.Factories
{
    public static class UserIdentityFactory
    {
        public static ApplicationUser CreateApplicationUser(ClaimsIdentity identity)
        {
            var newUser = new ApplicationUser();
            newUser.AzureAdNameIdentifier = identity.Name;

            // TODO: resolve all claims

            return newUser;
        }
    }
}
