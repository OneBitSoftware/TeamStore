namespace TeamStore.Models
{
    public abstract class ApplicationIdentity
    {
        public int Id { get; set; }
        public string AzureAdObjectIdentifier { get; set; }
        public string TenantId { get; set; }
    }
}
