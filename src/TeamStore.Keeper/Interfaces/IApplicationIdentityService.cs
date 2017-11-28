namespace TeamStore.Keeper.Interfaces
{
    using System;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using TeamStore.Keeper.Models;

    /// <summary>
    /// An interface for the <see cref="ApplicatiionIdentityService"/>
    /// </summary>
    public interface IApplicationIdentityService
    {
        /// Retrieves the current logged in user Application Identity
        /// </summary>
        /// <returns>A Task with the <see cref="ApplicationUser"/> as a result</returns>
        Task<ApplicationUser> GetCurrentUser();

        /// <summary>
        /// Creates or returns the <see cref="ApplicationUser"/> representing a passed IIdentity.
        /// This method checks the database first.
        /// </summary>
        /// <param name="identity">The IIdentity to resolve and map to an ApplicationUser</param>
        /// <returns>A Task with the <see cref="ApplicationUser"/> as a result</returns>
        Task<ApplicationUser> GetCurrentUser(IIdentity identity);

        /// <summary>
        /// Retrieves an ApplicationUser from the database by looking up the passed condition.
        /// </summary>
        /// <param name="lookupCondition">A predicate of the condition to lookup</param>
        /// <returns>A Task with the <see cref="ApplicationUser "/> as a result</returns>
        Task<ApplicationUser> FindUserAsync(Expression<Func<ApplicationIdentity, bool>> lookupCondition);

        /// <summary>
        /// Retrieves an ApplicationUser from the database by looking up the 
        /// passed ClaimsIdentity object. Matches a user by the object identifier claim within the ClaimsIdentity 
        /// claims collection.
        /// </summary>
        /// <param name="identity">The ClaimsIdentity holding the object identifier claim to lookup.</param>
        /// <returns>A Task with the <see cref="ApplicationUser "/> as a result</returns>
        Task<ApplicationUser> FindUserAsync(ClaimsIdentity identity);

        /// <summary>
        /// Retrieves an <see cref="ApplicationUser "/> from the database by looking up the 
        /// AzureAdObjectIdentifier. Matches a user by the object identifier claim.
        /// </summary>
        /// <param name="azureAdObjectIdentifier">The value of the object identifier claim to lookup.</param>
        /// <returns>A Task with the <see cref="ApplicationUser "/> as a result</returns>
        Task<ApplicationUser> FindUserByObjectIdAsync(string azureAdObjectIdentifier);

        /// <summary>
        /// Retrieves an ApplicationUser from the database by looking up the 
        /// UPN. Matches a user by the UPN claim.
        /// </summary>
        /// <param name="upn">The value of the UPN claim to lookup.</param>
        /// <returns>A Task with the ApplicationUser as a result</returns>
        Task<ApplicationUser> FindUserByUpnAsync(string upn);

        /// <summary>
        /// Attempts to Find a user by the object identifier claim.
        /// </summary>
        /// <param name="azureAdObjectIdentifier">The value of the object identifier claim to lookup.</param>
        /// <returns>A Task with the <see cref="ApplicationUser "/> as a result</returns>
        Task<ApplicationUser> EnsureUserByObjectIdAsync(string azureAdObjectIdentifier);

        /// <summary>
        /// Attempts to Find a user by the UPN.
        /// </summary>
        /// <param name="upn">The value of the object identifier claim to lookup.</param>
        /// <returns>A Task with the <see cref="ApplicationUser "/> as a result</returns>
        Task<ApplicationUser> EnsureUserByUpnAsync(string upn);

        // TODO: move to IPermissionService
        /// <summary>
        /// Checks if the current logged in user is a ystem administrator
        /// </summary>
        /// <returns>True if the current iser is a system administrator, False of not.</returns>
        Task<bool> IsCurrentUserAdmin();

        /// <summary>
        /// Sets an ApplicationUser as a System Addministrator
        /// </summary>
        /// <param name="applicationUser">The ApplicationUser to set as a System Administrator</param>
        /// <returns>True if the operation succeeded, otherwise False.</returns>
        Task<bool> SetSystemAdministrator(ApplicationUser applicationUser);

        /// <summary>
        /// Removes an ApplicationUser as a System Addministrator
        /// </summary>
        /// <param name="applicationUser">The ApplicationUser to remove as a System Administrator</param>
        /// <returns>True if the operation succeeded, otherwise False.</returns>
        Task<bool> RemoveSystemAdministrator(ApplicationUser applicationUser);
    }
}
