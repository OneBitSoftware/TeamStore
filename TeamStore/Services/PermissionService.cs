using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using TeamStore.Factories;
using TeamStore.Interfaces;
using TeamStore.Models;

namespace TeamStore.Services
{
    public class PermissionService : IPermissionService
    {

        public const string CURRENTUSERKEY = "Auth_CurrentUser";
        private readonly IGraphService _graphService;
        private HttpContext _context;

        IDictionary<object, object> _itemsCollection;
        IPrincipal _claimsPrincipal;

        public PermissionService(
            IGraphService graphService,
            IHttpContextAccessor httpContextAccessor,
            IDictionary<object, object> itemsCollection = null,
            IPrincipal claimsPrincipal = null
            )
        {
            _graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
            _context = httpContextAccessor?.HttpContext;
            if (_context == null)
            {
                throw new ArgumentNullException(nameof(_context));
            }
            else
            {
                // set the claims principal with priority from the context
                _claimsPrincipal = _context.User;
            }

            if (_claimsPrincipal == null) // if the context principal is null, set from the passed object
            {
                _claimsPrincipal = claimsPrincipal;
            }

            // Set the items colletion from the context
            if (_context.Items.Count > 0)
            {
                _itemsCollection = _context.Items;
            }
            else // if no items in context, set from passed values
            {
                _itemsCollection = itemsCollection;
            }
        }

        public async Task GrantAccess(Project project, string principals, ApplicationIdentity contextUser)
        {
            var newAccessIdentifier = new AccessIdentifier();
            newAccessIdentifier.Project = project;
            newAccessIdentifier.Role = "Edit"; // TODO
            newAccessIdentifier.Created = DateTime.UtcNow;
            //newAccessIdentifier.CreatedBy = (ApplicationUser)contextUser;

            var newGroup = new ApplicationGroup();

            // Get group object
            await _graphService.GetGroup(principals, contextUser.AzureAdObjectIdentifier);


            // project.AccessIdentifiers.Add

        }

        public Task<bool> UserHasAccess(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UserHasAccess(Project project)
        {
            throw new NotImplementedException();
        }

        public ApplicationUser GetCurrentUser()
        {
            if (_context == null) return null;

            // 1. Check context Item for Application User
            var applicationUserObject = _itemsCollection[CURRENTUSERKEY];
            if (applicationUserObject != null)
            {
                var applicationUser = applicationUserObject as ApplicationUser;
                if (applicationUser != null) return applicationUser;
            }

            // 2. return from HttpContext.User if the context item collection does not have it
            return GetCurrentUser(_context.User?.Identity);
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
    }
}
