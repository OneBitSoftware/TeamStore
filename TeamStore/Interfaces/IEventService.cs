namespace TeamStore.Interfaces
{
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using TeamStore.Models;

    public interface IEventService
    {
        Task StoreLoginEventAsync(ClaimsIdentity identity, string accessIpAddress);

        Task StoreGrantAccessEventAsync(
            int projectId,
            string remoteIpAddress,
            string newRole,
            string azureAdObjectIdentifier,
            ApplicationUser grantingUser);
    }
}
