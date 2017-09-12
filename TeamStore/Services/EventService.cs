using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TeamStore.DataAccess;
using TeamStore.Factories;
using TeamStore.Interfaces;
using TeamStore.Models;

namespace TeamStore.Services
{
    public class EventService : IEventService
    {
        public ApplicationDbContext DbContext { get; set; }

        public EventService(ApplicationDbContext context)
        {
            DbContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task StoreLoginEventAsync(ClaimsIdentity identity)
        {
            var loginEvent = new Event();
            loginEvent.DateTime = DateTime.UtcNow;
            loginEvent.Type = Enums.EventType.Signin;
            //loginEvent.User = UserIdentityFactory.CreateApplicationUser(identity);

            await DbContext.Events.AddAsync(loginEvent);
            await DbContext.SaveChangesAsync();
        }
    }
}
