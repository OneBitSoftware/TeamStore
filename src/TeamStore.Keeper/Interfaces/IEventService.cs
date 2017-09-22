namespace TeamStore.Keeper.Interfaces
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using TeamStore.Keeper.Models;

    public interface IEventService
    {
        /// <summary>
        /// Logs a Sign in event.
        /// </summary>
        /// <param name="identity">The created Claims Identity during sign-ing.</param>
        /// <param name="accessIpAddress">The IP address of the originating request.</param>
        /// <returns>A void Task object</returns>
        Task StoreLoginEventAsync(ClaimsIdentity identity, string remoteIpAddress);

        /// <summary>
        /// Logs a Grant Access event
        /// </summary>
        /// <param name="projectId">The Id of the project for which to revoke access.</param>
        /// <param name="remoteIpAddress">The IP address of the originating request.</param>
        /// <param name="newRole">The Role, level of access the identity must have against the project.</param>
        /// <param name="azureAdObjectIdentifier">The Azure AD Object Identifier.</param>
        /// <param name="revokingUser">The ApplicationUser performing the event.</param>
        /// <returns>A Task object</returns>
        Task StoreGrantAccessEventAsync(
            int projectId,
            string remoteIpAddress,
            string newRole,
            string azureAdObjectIdentifier,
            ApplicationUser grantingUser);

        /// <summary>
        /// Logs a Revoke Access event
        /// </summary>
        /// <param name="projectId">The Id of the project for which to revoke access.</param>
        /// <param name="remoteIpAddress">The IP address of the originating request</param>
        /// <param name="azureAdObjectIdentifier">The Azure AD Object Identifier</param>
        /// <param name="revokingUser">The ApplicationUser performing the event</param>
        /// <returns>A Task object</returns>
        Task LogRevokeAccessEventAsync(
            int projectId,
            string remoteIpAddress,
            string role,
            string azureAdObjectIdentifier,
            ApplicationUser revokingUser);
    }
}
