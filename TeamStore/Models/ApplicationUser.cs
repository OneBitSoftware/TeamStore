namespace TeamStore.Models
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

        /// <summary>
        /// The IP address returned from Azure AD during sign in. Stored for extra security.
        /// </summary>
        public string SignInIpAddress { get; set; }

        /// <summary>
        /// The IP address captured from the ASP.NET Core runtime.
        /// </summary>
        public string AccessIpAddress { get; set; }
    }
}
