namespace TeamStore.Keeper.Models
{
    public class ApplicationUser : ApplicationIdentity
    {
        /// <summary>
        /// The claim value given by the "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn" claim
        /// </summary>
        public string Upn { get; set; }

        /// <summary>
        /// The claim value given by the "http://schemas.microsoft.com/identity/claims/nameidentifier" claim
        /// </summary>
        public string AzureAdNameIdentifier { get; set; }

        /// <summary>
        /// The claim value given by the "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" claim
        /// </summary>
        public string AzureAdName { get; set; }

        public override string ToString()
        {
            return $"{DisplayName} ({Upn})";
        }
    }
}
