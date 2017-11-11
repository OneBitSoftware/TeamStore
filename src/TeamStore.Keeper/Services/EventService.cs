namespace TeamStore.Keeper.Services
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using TeamStore.Keeper.DataAccess;
    using TeamStore.Keeper.Factories;
    using TeamStore.Keeper.Interfaces;

    public class EventService : IEventService
    {
        private readonly IApplicationIdentityService _applicationIdentityService;

        private EventDbContext _dbContext;

        /// <summary>
        /// Constructor for the EventService.
        /// </summary>
        /// <param name="context">A database ApplicationDbContext</param>
        /// <param name="applicationIdentityService">An instance of ApplicationIdentityService to retrieve users.</param>
        public EventService(
            EventDbContext context,
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
            int targetUserId,
            string role,
            int revokingUserId,
            string customData)
        {
            var revokeAccess = new Models.Event();
            revokeAccess.DateTime = DateTime.UtcNow;
            revokeAccess.Type = Enums.EventType.RevokeAccess;
            revokeAccess.OldValue = role;
            revokeAccess.RemoteIpAddress = remoteIpAddress;
            revokeAccess.ActedByUser = revokingUserId.ToString();
            revokeAccess.TargetUserId = targetUserId;

            revokeAccess.Data = "ProjectId: " + projectId + " CustomData: " + customData;

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
        public async Task LogGrantAccessEventAsync(
            int projectId,
            string remoteIpAddress,
            string newRole,
            int targetUserId,
            int grantingUserId,
            string customData)
        {
            var grantAccess = new Models.Event();
            grantAccess.DateTime = DateTime.UtcNow;
            grantAccess.Type = Enums.EventType.GrantAccess;
            grantAccess.NewValue = newRole;
            grantAccess.TargetUserId = targetUserId;
            grantAccess.RemoteIpAddress = remoteIpAddress;
            grantAccess.ActedByUser = grantingUserId.ToString();
            grantAccess.Data = customData + " ProjectId: " + projectId;

            await _dbContext.Events.AddAsync(grantAccess);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Logs a Sign in event.
        /// </summary>
        /// <param name="identity">The created Claims Identity during sign-ing.</param>
        /// <param name="accessIpAddress">The IP address of the originating request.</param>
        /// <returns>A void Task object</returns>
        public async Task LogLoginEventAsync(ClaimsIdentity identity, string accessIpAddress)
        {
            var loginEvent = new Models.Event();
            loginEvent.DateTime = DateTime.UtcNow;
            loginEvent.Type = Enums.EventType.Signin;

            // Get/Create user
            var claim = identity.Claims.FirstOrDefault(c => c.Type == Constants.CLAIMS_OBJECTIDENTIFIER);

            loginEvent.ActedByUser = claim.Value;

            // Set IP as extra data
            var signInIpAddress = UserIdentityFactory.GetClaimValue("ipaddr", identity.Claims);
            loginEvent.Data = "AADSignInIpAddress: " + signInIpAddress;
            loginEvent.RemoteIpAddress = accessIpAddress;

            await _dbContext.Events.AddAsync(loginEvent);
            await _dbContext.SaveChangesAsync();
        }

        public async Task LogCustomEvent(string actingUserId, string customData)
        {
            var loginEvent = new Models.Event();
            loginEvent.DateTime = DateTime.UtcNow;
            loginEvent.Type = Enums.EventType.CustomEvent;
            loginEvent.ActedByUser = actingUserId;
            loginEvent.Data = customData;

            await _dbContext.Events.AddAsync(loginEvent);
            await _dbContext.SaveChangesAsync();
        }
    }
}
