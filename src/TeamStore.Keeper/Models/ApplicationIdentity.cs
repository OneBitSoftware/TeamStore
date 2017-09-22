namespace TeamStore.Keeper.Models
{
    public abstract class ApplicationIdentity
    {
        /// <summary>
        /// The internal id for the database
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The claim value given by the "name" claim
        /// NOTE: this is not http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The claim value given by the "http://schemas.microsoft.com/identity/claims/objectidentifier" claim
        /// </summary>
        public string AzureAdObjectIdentifier { get; set; }

        /// <summary>
        /// The claim value given by the "http://schemas.microsoft.com/identity/claims/tenantid" claim
        /// </summary>
        public string TenantId { get; set; }
    }
}
