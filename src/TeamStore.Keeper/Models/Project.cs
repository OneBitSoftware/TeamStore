namespace TeamStore.Keeper.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a Project entity in the application
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Constructor for the Project entity
        /// </summary>
        public Project()
        {
            Assets = new List<Asset>();
            AccessIdentifiers = new List<AccessIdentifier>();

            Description = string.Empty;
            Category = string.Empty;
        }

        /// <summary>
        /// The database primary key for Project
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The project title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A short description of the project
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// An end-user defined category to allow grouping of related projects
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// A list of Assets that belong to the project
        /// </summary>
        public List<Asset> Assets { get; set; }

        /// <summary>
        /// A list of <c>AccessIdentifier</c> objects, storing permissions against the project
        /// </summary>
        public List<AccessIdentifier> AccessIdentifiers { get; set; }

        /// <summary>
        /// The archive status of a project. True if archived.
        /// </summary>
        public bool IsArchived { get; set; }
    }
}
