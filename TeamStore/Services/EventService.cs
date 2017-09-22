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

        /// <summary>
        /// Constructor for the EventService.
        /// </summary>
        /// <param name="context">A database ApplicationDbContext</param>
        /// <param name="applicationIdentityService">An instance of ApplicationIdentityService to retrieve users.</param>
        public EventService(
            ApplicationDbContext context,
            IApplicationIdentityService applicationIdentityService)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
            _applicationIdentityService = applicationIdentityService ?? throw new ArgumentNullException(nameof(applicationIdentityService));
        }

        /// <summary>
        /// Logs a Revoke Access event
        /// </summary>
        /// <param name="projectId">The Id of the project for which to revoke access.</param>
        /// <param name="remoteIpAddress">The IP address of the originating request</param>
        /// <param name="azureAdObjectIdentifier">The Azure AD Object Identifier</param>
        /// <param name="revokingUser">The ApplicationUser performing the event</param>
        /// <returns>A Task object</returns>
        public async Task LogRevokeAccessEventAsync(
            int projectId,
            string remoteIpAddress, 
            string azureAdObjectIdentifier,
            string role,
            ApplicationUser revokingUser)
        {
            var revokeAccess = new Event();
            revokeAccess.DateTime = DateTime.UtcNow;
            revokeAccess.Type = Enums.EventType.RevokeAccess;
            revokeAccess.OldValue = role;
            revokeAccess.RemoteIpAddress = remoteIpAddress;
            revokeAccess.ActedByUser = revokingUser;
            revokeAccess.Data = "ProjectId: " + projectId + " RemovingAccessFor: " + azureAdObjectIdentifier;

            await _dbContext.Events.AddAsync(revokeAccess);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Logs a Grant Access event
        /// </summary>
        /// <param name="projectId">The Id of the project for which to revoke access.</param>
        /// <param name="remoteIpAddress">The IP address of the originating request.</param>
        /// <param name="newRole">The Role, level of access the identity must have against the project.</param>
        /// <param name="azureAdObjectIdentifier">The Azure AD Object Identifier.</param>
        /// <param name="revokingUser">The ApplicationUser performing the event.</param>
        /// <returns>A Task object</returns>
        public async Task StoreGrantAccessEventAsync(int projectId, string remoteIpAddress, string newRole, string azureAdObjectIdentifier, ApplicationUser grantingUser)
        {
            var grantAccess = new Event();
            grantAccess.DateTime = DateTime.UtcNow;
            grantAccess.Type = Enums.EventType.GrantAccess;
            grantAccess.NewValue = newRole;
            grantAccess.RemoteIpAddress = remoteIpAddress;
            grantAccess.ActedByUser = grantingUser;
            grantAccess.Data = "ProjectId: " + projectId + " GrantedTo: " + azureAdObjectIdentifier;

            await _dbContext.Events.AddAsync(grantAccess);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Logs a Sign in event.
        /// </summary>
        /// <param name="identity">The created Claims Identity during sign-ing.</param>
        /// <param name="accessIpAddress">The IP address of the originating request.</param>
        /// <returns>A void Task object</returns>
        public async Task StoreLoginEventAsync(ClaimsIdentity identity, string accessIpAddress)
        {
            var loginEvent = new Event();
            loginEvent.DateTime = DateTime.UtcNow;
            loginEvent.Type = Enums.EventType.Signin;

            // Get/Create user
            ApplicationUser existingUser = await _applicationIdentityService.FindUserAsync(identity);
            if (existingUser == null)
            {
                loginEvent.ActedByUser = UserIdentityFactory.CreateNewApplicationUserFromAzureIdentity(identity);
            }
            else
            {
                loginEvent.ActedByUser = existingUser;
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
