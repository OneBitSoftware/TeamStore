namespace TeamStore.Keeper.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using TeamStore.Keeper.DataAccess;
    using TeamStore.Keeper.Enums;
    using TeamStore.Keeper.Factories;
    using TeamStore.Keeper.Interfaces;
    using TeamStore.Keeper.Models;

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
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

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
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

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
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

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
            if (string.IsNullOrWhiteSpace(accessIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

            // Get/Create user
            var claim = identity.Claims.FirstOrDefault(c => c.Type == Constants.CLAIMS_OBJECTIDENTIFIER);

            // Set IP as extra data
            var signInIpAddress = UserIdentityFactory.GetClaimValue("ipaddr", identity.Claims);

            var loginEvent = new Models.Event();
            loginEvent.DateTime = DateTime.UtcNow;
            loginEvent.ActedByUser = claim.Value;
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

        /// <summary>
        /// Retrieves all sign-in events for the specified period.
        /// </summary>
        /// <param name="startDateTime">The start time of the login events</param>
        /// <param name="endDateTime">The end time of the login events</param>
        /// <returns>A Task of IEnumerable<Event> objects.</returns>
        public async Task<IEnumerable<Models.Event>> GetLoginEventsAsync(DateTime startDateTime, DateTime endDateTime)
        {
            var results = await _dbContext.Events
                .Where(e =>
                    e.DateTime >= startDateTime &&
                    e.DateTime <= endDateTime &&
                    e.Type == EventType.Signin.ToString())
                .ToListAsync();

            return results;
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
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

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

        public async Task LogAssetAccessEventAsync(
            int projectId,
            string projectTitle,
            int actingUserId,
            string actingUserUpn,
            string remoteIpAddress,
            int assetId,
            string assetTitle,
            string assetLogin
            )
        {
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

            var retrieveAssetEvent = new Models.Event();
            retrieveAssetEvent.DateTime = DateTime.UtcNow;
            retrieveAssetEvent.Type = Enums.EventType.RetrieveAsset.ToString();
            retrieveAssetEvent.RemoteIpAddress = remoteIpAddress;
            retrieveAssetEvent.ActedByUserId = actingUserId.ToString();
            retrieveAssetEvent.ActedByUser = actingUserUpn;
            retrieveAssetEvent.ProjectId = projectId;
            retrieveAssetEvent.ProjectTitle = projectTitle;
            retrieveAssetEvent.AssetId = assetId;
            retrieveAssetEvent.AssetTitle = assetTitle;
            retrieveAssetEvent.AssetLogin = assetLogin;

            await _dbContext.Events.AddAsync(retrieveAssetEvent);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }
        }

        /// <summary>
        /// Gets all Asset access logs for the specified period.
        /// </summary>
        /// <param name="startDateTime">The start time of the asset retrievals.</param>
        /// <param name="endDateTime">The end time of the asset retrievals.</param>
        /// <returns>A Task of IEnumerable<Event> objects.</returns>
        public async Task<IEnumerable<Event>> GetAssetAccessEventsAsync(DateTime startDateTime, DateTime endDateTime, string login = "")
        {
            var results = await _dbContext.Events
                .Where(e =>
                    (login != "" ? e.ActedByUser == login : true) &&
                    e.DateTime >= startDateTime &&
                    e.DateTime <= endDateTime &&                   
                    e.Type == EventType.RetrieveAsset.ToString())
            .ToListAsync();

            return results;
        }

        public async Task<Event> GetAssetLastAccessEventAsync(int assetId)
        {
            // gets the last access event for a given asset id
            var result = await _dbContext.Events
                    .Where(e => e.AssetId == assetId &&
                                e.Type == EventType.RetrieveAsset.ToString())
                    .OrderByDescending(e => e.DateTime)
                    .FirstOrDefaultAsync();

            return result;
        }

        public async Task LogUpdatePasswordEventAsync(int projectId, string remoteIpAddress, int actingUserId, int assetId)
        {
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

            var retrieveAssetEvent = new Models.Event();
            retrieveAssetEvent.DateTime = DateTime.UtcNow;
            retrieveAssetEvent.Type = EventType.UpdatePassword.ToString();
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
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

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
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

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
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

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

        public async Task LogArchiveAssetEventAsync(int projectId, string remoteIpAddress, int actingUserId, int assetId)
        {
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

            var retrieveAssetEvent = new Models.Event();
            retrieveAssetEvent.DateTime = DateTime.UtcNow;
            retrieveAssetEvent.Type = Enums.EventType.ArchiveAsset.ToString();
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
    }
}
