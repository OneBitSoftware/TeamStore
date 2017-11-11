namespace TeamStore.Keeper.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    // TODO: validate/test created/createdby/modified/modifiedby/Role not null
    public class AccessIdentifier
    {
        public int Id { get; set; }

        public ApplicationIdentity Identity { get; set; }

        public string Role { get; set; }

        public int ProjectForeignKey { get; set; }

        [ForeignKey("ProjectForeignKey")]
        public Project Project { get; set; }

        public ApplicationUser CreatedBy { get; set; }

        public DateTime Created{ get; set; }

        public DateTime Modified { get; set; }

        public ApplicationUser ModifiedBy { get; set; }
    }
}
