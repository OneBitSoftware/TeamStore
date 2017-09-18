namespace TeamStore.Interfaces
{
    using System.Security.Claims;
    using System.Security.Principal;
    using TeamStore.Models;

    public interface IApplicationIdentityService
    {
        ApplicationUser GetCurrentUser();
        ApplicationUser GetCurrentUser(IIdentity identity);

        ApplicationUser GetUser(ClaimsIdentity identity);
    }
}
