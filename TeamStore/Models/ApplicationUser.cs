namespace TeamStore.Models
{
    using System;

    public class ApplicationUser : ApplicationIdentity
    {
        public string Upn { get; set; }
        public string AzureAdNameIdentifier { get; set; }
    }
}
