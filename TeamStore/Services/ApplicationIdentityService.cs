namespace TeamStore.Services
{
    using System.Security.Principal;
    using Microsoft.AspNetCore.Http;
    using TeamStore.Interfaces;
    using TeamStore.Models;
    using System.Collections.Generic;
    using System;
    using System.Linq;
    using TeamStore.Factories;
    using System.Security.Claims;
    using TeamStore.DataAccess;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;

    public class ApplicationIdentityService : IApplicationIdentityService
    {
        public const string CURRENTUSERKEY = "Auth_CurrentUser";
        private ApplicationDbContext _dbContext;

        private HttpContext _httpContext;
        IDictionary<object, object> _itemsCollection;
        IPrincipal _claimsPrincipal;

        public ApplicationIdentityService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IDictionary<object, object> itemsCollection = null,
            IPrincipal claimsPrincipal = null
            )
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));

            _httpContext = httpContextAccessor?.HttpContext;
            if (_httpContext == null)
            {
                throw new ArgumentNullException(nameof(_httpContext));
            }
            else
            {
                // set the claims principal with priority from the context
                _claimsPrincipal = _httpContext.User;
            }

            if (_claimsPrincipal == null) // if the context principal is null, set from the passed object
            {
                _claimsPrincipal = claimsPrincipal;
            }

            // Set the items colletion from the context
            if (_httpContext.Items.Count > 0)
            {
                _itemsCollection = _httpContext.Items;
            }
            else // if no items in context, set from passed values
            {
                _itemsCollection = itemsCollection;
            }
        }

        public ApplicationUser GetCurrentUser()
        {
            if (_httpContext == null) return null;
            if (_itemsCollection == null) return null;
            if (_itemsCollection.ContainsKey(CURRENTUSERKEY) == false) return null;

            // 1. Check context Item for Application User
            var applicationUser = _itemsCollection[CURRENTUSERKEY] as ApplicationUser;
            if (applicationUser != null) return applicationUser;

            // 2. return from HttpContext.User if the context item collection does not have it
            return GetCurrentUser(_httpContext.User?.Identity);
        }

        public ApplicationUser GetCurrentUser(IIdentity identity)
        {
            if (identity == null) return null;
            if (identity.IsAuthenticated == false) return null; // not authenticated, so we shouldn't build an object

            var currentApplicationUser = UserIdentityFactory.CreateApplicationUserFromAzureIdentity(identity as ClaimsIdentity);

            // Update the HttpContext requet object if it is not set. On the next Get it will get it from the context.
            if (_itemsCollection[CURRENTUSERKEY] != null && _itemsCollection[CURRENTUSERKEY] as ApplicationUser != currentApplicationUser)
            {
                _itemsCollection[CURRENTUSERKEY] = currentApplicationUser;
            }

            // Set the context if it is empty
            if (_itemsCollection[CURRENTUSERKEY] == null)
            {
                _itemsCollection[CURRENTUSERKEY] = currentApplicationUser;
            }

            return currentApplicationUser;
        }

        public async Task<ApplicationUser> GetUserAsync(ClaimsIdentity identity)
        {
            var claim = identity.Claims.FirstOrDefault(c => c.Type == Constants.CLAIMS_OBJECTIDENTIFIER);
            if (claim == null) return null;
            if (string.IsNullOrWhiteSpace(claim.Value)) return null;

            return await GetUserAsync(claim.Value);
        }

        public async Task<ApplicationUser> GetUserAsync(string azureAdObjectIdentifier)
        {
            if (string.IsNullOrWhiteSpace(azureAdObjectIdentifier)) return null;

            var returnedObject = await _dbContext.ApplicationIdentities.Where
                (u => u.AzureAdObjectIdentifier == azureAdObjectIdentifier).FirstOrDefaultAsync();

            var returnUser = returnedObject as ApplicationUser;
            return returnUser;
        }


        public async Task<ApplicationUser> EnsureUserAsync(string azureAdObjectIdentifier)
        {
            var existingUser = await GetUserAsync(azureAdObjectIdentifier);
            if (existingUser != null)
            {
                return existingUser;
            }
            else
            {
                // TODO: RESOLVE USER from Azure AD
                existingUser = new ApplicationUser()
                {
                    AzureAdObjectIdentifier = azureAdObjectIdentifier
                };
            }

            return existingUser;
        }
    }
}
