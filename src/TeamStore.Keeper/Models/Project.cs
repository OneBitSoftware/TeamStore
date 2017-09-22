namespace TeamStore.Keeper.Models
{
    using System.Collections.Generic;

    public class Project
    {
        public Project()
        {
            Assets = new List<Asset>();
            AccessIdentifiers = new List<AccessIdentifier>();

            Description = string.Empty;
            Category = string.Empty;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public List<Asset> Assets { get; set; }
        public List<AccessIdentifier> AccessIdentifiers { get; set; }
        public bool IsArchived { get; set; }
    }
}
