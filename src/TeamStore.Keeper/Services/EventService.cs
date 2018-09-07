namespace TeamStore.Keeper.Services
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using TeamStore.Keeper.DataAccess;
    using TeamStore.Keeper.Enums;
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
            Role role,
            int revokingUserId,
            string customData)
        {
            var revokeAccess = new Models.Event();
            revokeAccess.DateTime = DateTime.UtcNow;
            revokeAccess.Type = Enums.EventType.RevokeAccess.ToString();
            revokeAccess.OldValue = role.ToString();
            revokeAccess.RemoteIpAddress = remoteIpAddress;
            revokeAccess.ActedByUser = revokingUserId.ToString();
            revokeAccess.TargetUserId = targetUserId;
            revokeAccess.ProjectId = projectId;
            revokeAccess.Data = "CustomData: " + customData;

            await _dbContext.Events.AddAsync(revokeAccess);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }
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
            Role newRole,
            int targetUserId,
            int grantingUserId,
            string customData)
        {
            var grantAccess = new Models.Event();
            grantAccess.DateTime = DateTime.UtcNow;
            grantAccess.Type = Enums.EventType.GrantAccess.ToString();
            grantAccess.NewValue = newRole.ToString();
            grantAccess.TargetUserId = targetUserId;
            grantAccess.RemoteIpAddress = remoteIpAddress;
            grantAccess.ActedByUser = grantingUserId.ToString();
            grantAccess.ProjectId = projectId;
            grantAccess.Data = customData;

            await _dbContext.Events.AddAsync(grantAccess);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }
        }

        /// <summary>
        /// Logs a project archive event
        /// </summary>
        /// <param name="projectId">The ID of the project to archive</param>
        /// <param name="actingUserId">The id of the ApplicationUser performing the action</param>
        /// <param name="remoteIpAddress">The IP address of the user calling the action</param>
        /// <returns>A Task result</returns>
        public async Task LogArchiveProjectEventAsync(int projectId, int actingUserId, string remoteIpAddress)
        {
            var archiveEvent = new Models.Event();
            archiveEvent.DateTime = DateTime.UtcNow;
            archiveEvent.Type = Enums.EventType.ArchiveProject.ToString();
            archiveEvent.RemoteIpAddress = remoteIpAddress;
            archiveEvent.ActedByUser = actingUserId.ToString();
            archiveEvent.ProjectId = projectId;

            await _dbContext.Events.AddAsync(archiveEvent);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }
        }

        /// <summary>
        /// Logs a Sign in event.
        /// </summary>
        /// <param name="identity">The created Claims Identity during sign-ing.</param>
        /// <param name="accessIpAddress">The IP address of the originating request.</param>
        /// <returns>A void Task object</returns>
        public async Task LogLoginEventAsync(ClaimsIdentity identity, string accessIpAddress)
        {
            // Get/Create user
            var claim = identity.Claims.FirstOrDefault(c => c.Type == Constants.CLAIMS_OBJECTIDENTIFIER);

            // Set IP as extra data
            var signInIpAddress = UserIdentityFactory.GetClaimValue("ipaddr", identity.Claims);

            var loginEvent = new Models.Event();
            loginEvent.DateTime = DateTime.UtcNow;
            loginEvent.ActedByUser = claim.Value;
            loginEvent.Type = Enums.EventType.Signin.ToString();
            loginEvent.Data = "AADSignInIpAddress: " + signInIpAddress;
            loginEvent.RemoteIpAddress = accessIpAddress;

            await _dbContext.Events.AddAsync(loginEvent);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }
        }

        public async Task LogCustomEventAsync(string actingUserId, string customData)
        {
            var loginEvent = new Models.Event();
            loginEvent.DateTime = DateTime.UtcNow;
            loginEvent.Type = Enums.EventType.CustomEvent.ToString();
            loginEvent.ActedByUser = actingUserId;
            loginEvent.Data = customData;

            await _dbContext.Events.AddAsync(loginEvent);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }
        }

        public async Task LogCreateAssetEventAsync(
            int projectId,
            int actingUserId,
            string remoteIpAddress,
            int assetId,
            string assetDescription)
        {
            var createAssetEvent = new Models.Event();
            createAssetEvent.DateTime = DateTime.UtcNow;
            createAssetEvent.Type = Enums.EventType.CreateAsset.ToString();
            createAssetEvent.RemoteIpAddress = remoteIpAddress;
            createAssetEvent.ActedByUser = actingUserId.ToString();
            createAssetEvent.ProjectId = projectId;
            createAssetEvent.AssetId = assetId;
            createAssetEvent.NewValue = assetDescription;

            await _dbContext.Events.AddAsync(createAssetEvent);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }
        }

        public async Task LogAssetAccessEventAsync(int projectId, int actingUserId, string remoteIpAddress, int assetId)
        {
            var retrieveAssetEvent = new Models.Event();
            retrieveAssetEvent.DateTime = DateTime.UtcNow;
            retrieveAssetEvent.Type = Enums.EventType.RetrieveAsset.ToString();
            retrieveAssetEvent.RemoteIpAddress = remoteIpAddress;
            retrieveAssetEvent.ActedByUser = actingUserId.ToString();
            retrieveAssetEvent.ProjectId = projectId;
            retrieveAssetEvent.AssetId = assetId;

            await _dbContext.Events.AddAsync(retrieveAssetEvent);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }
        }

        public async Task LogUpdatePasswordEventAsync(int projectId, string remoteIpAddress, int actingUserId, int assetId)
        {
            var retrieveAssetEvent = new Models.Event();
            retrieveAssetEvent.DateTime = DateTime.UtcNow;
            retrieveAssetEvent.Type = Enums.EventType.UpdatePassword.ToString();
            retrieveAssetEvent.RemoteIpAddress = remoteIpAddress;
            retrieveAssetEvent.ActedByUser = actingUserId.ToString();
            retrieveAssetEvent.ProjectId = projectId;
            retrieveAssetEvent.AssetId = assetId;

            await _dbContext.Events.AddAsync(retrieveAssetEvent);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }
        }

        public async Task LogUpdateAssetEventAsync(int projectId, string remoteIpAddress, int actingUserId, int assetId)
        {
            var retrieveAssetEvent = new Models.Event();
            retrieveAssetEvent.DateTime = DateTime.UtcNow;
            retrieveAssetEvent.Type = Enums.EventType.UpdateAsset.ToString();
            retrieveAssetEvent.RemoteIpAddress = remoteIpAddress;
            retrieveAssetEvent.ActedByUser = actingUserId.ToString();
            retrieveAssetEvent.ProjectId = projectId;
            retrieveAssetEvent.AssetId = assetId;

            await _dbContext.Events.AddAsync(retrieveAssetEvent);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }
        }
        /// <summary>
        /// Logs a project create event
        /// </summary>
        /// <param name="projectId">The ID of the project to archive</param>
        /// <param name="actingUserId">The id of the ApplicationUser performing the action</param>
        /// <param name="remoteIpAddress">The IP address of the user calling the action</param>
        /// <returns>A Task result</returns>
        public async Task LogCreateProjectEventAsync(int projectId, int actingUserId, string remoteIpAddress)
        {
            var createProjectEvent = new Models.Event();
            createProjectEvent.DateTime = DateTime.UtcNow;
            createProjectEvent.Type = Enums.EventType.CreateProject.ToString();
            createProjectEvent.RemoteIpAddress = remoteIpAddress;
            createProjectEvent.ActedByUser = actingUserId.ToString();
            createProjectEvent.ProjectId = projectId;

            await _dbContext.Events.AddAsync(createProjectEvent);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }
        }

        /// <summary>
        /// Logs a project update event
        /// </summary>
        /// <param name="projectId">The ID of the project to archive</param>
        /// <param name="actingUserId">The id of the ApplicationUser performing the action</param>
        /// <param name="remoteIpAddress">The IP address of the user calling the action</param>
        /// <returns>A Task result</returns>
        public async Task LogUpdateProjectEventAsync(int projectId, int actingUserId, string remoteIpAddress)
        {
            var updateProjectEvent = new Models.Event();
            updateProjectEvent.DateTime = DateTime.UtcNow;
            updateProjectEvent.Type = Enums.EventType.UpdateProject.ToString();
            updateProjectEvent.RemoteIpAddress = remoteIpAddress;
            updateProjectEvent.ActedByUser = actingUserId.ToString();
            updateProjectEvent.ProjectId = projectId;

            await _dbContext.Events.AddAsync(updateProjectEvent);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }
        }
    }
}
