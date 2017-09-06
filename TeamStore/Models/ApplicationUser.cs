namespace TeamStore.Models
{
    using System;

    public class ApplicationUser
    {
        public int Id { get; set; }
        public string Upn { get; set; }
        public Guid AzureAdObjectIdentifier { get; set; }
        public string AzureAdNameIdentifier { get; set; }
        public Guid TenantId { get; set; }
    }
}
