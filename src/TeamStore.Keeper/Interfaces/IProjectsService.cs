namespace TeamStore.Keeper.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TeamStore.Keeper.Models;

    /// <summary>
    /// An interface for a Project Service. Defines Project CRUD operations.
    /// </summary>
    public interface IProjectsService
    {
        /// <summary>
        /// Retrieves a Project by Project Id, if the user has access to it. Decrypts any encrypted data.
        /// </summary>
        /// <param name="projectId">The Project Id to lookup.</param>
        /// <returns>A Project object, null if none are found or the current user does not have access to it.</returns>
        Task<Project> GetProject(int projectId);

        /// <summary>
        /// Retrieves a Project by Project Id, if the user has access to it, and can skip decryption if specified.
        /// </summary>
        /// <param name="projectId">The Project Id to lookup.</param>
        /// <param name="skipDecryption">Set to True if the project should not be decrypted.</param>
        /// <returns>A Project object, null if none are found or the current user does not have access to it.</returns>
        Task<Project> GetProject(int projectId, bool skipDecryption);

        /// <summary>
        /// Gets all projects for which the current user has access to. Excludes archived projects. All data is decrypted.
        /// </summary>
        /// <returns>A list of Project objects</returns>
        Task<List<Project>> GetProjects();

        /// <summary>
        /// Encrypts and persists a Project in the database
        /// </summary>
        /// <param name="decryptedProject">The Project object to encrypt and persist</param>
        /// <returns>A Task of int with the Project Id.</returns>
        Task<int> CreateProject(Project decryptedProject);

        /// <summary>
        /// Discards all tracked changes to the entity and marks it as archived in the database
        /// </summary>
        /// <param name="decryptedProject">The Project entity to archive.</param>
        /// <returns>A Task result</returns>
        Task ArchiveProject(Project decryptedProject);
    }
}
