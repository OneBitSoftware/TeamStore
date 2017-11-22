namespace TeamStore.Keeper.Models
{
    /// <summary>
    /// Represents a Credential entity in the system, inheriting from Asset
    /// </summary>
    public class Credential : Asset
    {
        /// <summary>
        /// Designed to store the full login, including the domain
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// The password/secret of the credential
        /// </summary>
        public string Password { get; set; }
    }
}
