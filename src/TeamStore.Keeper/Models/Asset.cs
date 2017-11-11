namespace TeamStore.Keeper.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using TeamStore.Keeper.Interfaces;

    public abstract class Asset : IAsset
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public DateTime Modified { get; set; }
        public ApplicationUser ModifiedBy { get; set; }

        public bool IsArchived { get; set; }
        public int ProjectForeignKey { get; set; }

        [ForeignKey("ProjectForeignKey")]
        public Project Project { get; set; }
    }
}
