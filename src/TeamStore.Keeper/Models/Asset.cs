namespace TeamStore.Keeper.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using TeamStore.Keeper.Interfaces;

    public abstract class Asset : IAsset
    {
        public int Id { get; set; }

        /// <summary>
        /// The display name of the Asset
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Notes/body for the Credential entity
        /// </summary>
        public string Body { get; set; }

        public DateTime Created { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public DateTime Modified { get; set; }
        public ApplicationUser ModifiedBy { get; set; }

        /// <summary>
        /// Determines if an asset is archived. Archived assets don't show in the UI.
        /// </summary>
        public bool IsArchived { get; set; }
        public int ProjectForeignKey { get; set; }

        [ForeignKey("ProjectForeignKey")]
        public Project Project { get; set; }
    }
}
