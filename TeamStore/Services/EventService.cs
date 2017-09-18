using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using TeamStore.DataAccess;
using TeamStore.Factories;
using TeamStore.Interfaces;
using TeamStore.Models;

namespace TeamStore.Services
{
    public class EventService : IEventService
    {
        private readonly IApplicationIdentityService _applicationIdentityService;

        private ApplicationDbContext _dbContext;

        public EventService(
            ApplicationDbContext context,
            IApplicationIdentityService applicationIdentityService)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
            _applicationIdentityService = applicationIdentityService ?? throw new ArgumentNullException(nameof(applicationIdentityService));

        }

        public async Task StoreLoginEventAsync(ClaimsIdentity identity, string accessIpAddress)
        {
            var loginEvent = new Event();
            loginEvent.DateTime = DateTime.UtcNow;
            loginEvent.Type = Enums.EventType.Signin;

            // Get/Create user
            ApplicationUser existingUser = _applicationIdentityService.GetUser(identity);
            if (existingUser == null)
            {
                loginEvent.User = UserIdentityFactory.CreateApplicationUserFromAzureIdentity(identity);
            }
            else
            {
                loginEvent.User = existingUser;
            }

            // Set IP as extra data
            var signInIpAddress = UserIdentityFactory.GetClaimValue("ipaddr", identity.Claims);
            loginEvent.Data = "SignInIpAddress: " + signInIpAddress;
            loginEvent.RemoteIpAddress = accessIpAddress;

            await _dbContext.Events.AddAsync(loginEvent);
            await _dbContext.SaveChangesAsync();
        }
    }
}
