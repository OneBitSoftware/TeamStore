namespace TeamStore.Keeper.Models
{
    /// <summary>
    /// Represents a simple note asset as an entity, inherits from Asset
    /// </summary>
    public class Note : Asset
    {
        /// <summary>
        /// A simple message body string to store useful information
        /// </summary>
        public string Body { get; set; }
    }
}
