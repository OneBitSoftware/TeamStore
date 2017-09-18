namespace TeamStore.Interfaces
{
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using TeamStore.Models;

    public interface IApplicationIdentityService
    {
        ApplicationUser GetCurrentUser();
        ApplicationUser GetCurrentUser(IIdentity identity);

        Task<ApplicationUser> GetUserAsync(ClaimsIdentity identity);
        Task<ApplicationUser> GetUserAsync(string azureAdObjectIdentifier);
        Task<ApplicationUser> EnsureUserAsync(string azureAdObjectIdentifier);
    }
}
