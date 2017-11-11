namespace TeamStore.Keeper.Models
{
    public class Credential : Asset
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
    }
}
