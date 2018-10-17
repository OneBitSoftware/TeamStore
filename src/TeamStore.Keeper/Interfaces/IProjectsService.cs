namespace TeamStore.Keeper.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TeamStore.Keeper.Models;

    /// <summary>
    /// An interface for a Project Service. Defines project CRUD operations.
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
        /// Gets all projects for which the current user has access to. Can include archived projects. Can skip decryption.
        /// </summary>
        /// <returns>A list of Project objects</returns>
        Task<List<Project>> GetProjects(bool skipDecryption = false, bool includeArchived = false);

        /// <summary>
        /// Gets all archived projects. Can skip decryption.
        /// </summary>
        /// <returns>A list of Project objects</returns>
        Task<List<Project>> GetArchivedProjectsAsync(bool skipDecryption = false);

        /// <summary>

        /// Loads the related Assets for a project from the database
        /// </summary>
        /// <param name="project">The Project object to load assets for</param>
        /// <returns>A Task object</returns>
        Task LoadAssetsForProjectAsync(Project project);

        /// Gets all archived projects items for a period. Can skip decryption and project title.
        /// </summary>
        /// <returns>A list of Project objects</returns>
        Task<List<Project>> GetArchivedProjectsAsync(DateTime startDateTime, DateTime endDateTime, string projectTitle = "", bool skipDecryption = false);


        /// <summary>
        /// Encrypts and persists a Project in the database
        /// </summary>
        /// <param name="decryptedProject">The Project object to encrypt and persist</param>
        /// <param name="remoteIpAddress">The IP address of the request causing the event</param>
        /// <returns>A Task of int with the Project Id.</returns>
        Task<int> CreateProject(Project decryptedProject, string remoteIpAddress);

        /// <summary>
        /// Imports and persists a Project into the database.
        /// This is designed to be used by a database import.
        /// </summary>
        /// <param name="decryptedProject">The Project object to encrypt and persist</param>
        /// <param name="remoteIpAddress">The IP address of the request causing the event</param>
        /// <returns>A Task of int with the Project Id.</returns>
        Task<int> ImportProject(Project decryptedProject, string remoteIpAddress);

        /// <summary>
        /// Discards all tracked changes to the entity and marks it as archived in the database
        /// </summary>
        /// <param name="decryptedProject">The Project entity to archive.</param>
        /// <param name="remoteIpAddress">The IP address of the request causing the event</param>
        /// <returns>A Task result</returns>
        Task ArchiveProject(Project decryptedProject, string remoteIpAddress);
        Task ArchiveProject(Project decryptedProject, string remoteIpAddress, ApplicationUser actingUser);

        /// <summary>
        /// Persists a passed <see cref="Project"/> in the database, setting modified dates and users.
        /// </summary>
        /// <param name="project">The Project to update</param>
        /// <param name="remoteIpAddress">The IP address of the request causing the event</param>
        /// <returns>A Task result of void</returns>
        Task UpdateProject(Project project, string remoteIpAddress);
    }
}
