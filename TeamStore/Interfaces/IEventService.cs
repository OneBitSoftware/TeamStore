namespace TeamStore.Interfaces
{
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;

    public interface IEventService
    {
        Task StoreLoginEventAsync(ClaimsIdentity identity, string accessIpAddress);
    }
}
