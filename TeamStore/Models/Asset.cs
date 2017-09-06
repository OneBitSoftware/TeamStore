namespace TeamStore.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using TeamStore.Interfaces;

    public abstract class Asset : IAsset
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsEnabled { get; set; }
        public int ProjectForeignKey { get; set; }

        [ForeignKey("ProjectForeignKey")]
        public Project Project { get; set; }
    }
}
