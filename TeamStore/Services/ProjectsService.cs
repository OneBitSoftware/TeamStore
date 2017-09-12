namespace TeamStore.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TeamStore.DataAccess;
    using TeamStore.Interfaces;
    using TeamStore.Models;

    public class ProjectsService : IProjectsService
    {
        private ApplicationDbContext _dbContext { get; set; }
        private readonly IEncryptionService _encryptionService;

        /// <summary>
        /// Constructor for the Project Service
        /// </summary>
        /// <param name="context"></param>
        /// <param name="encryptionService"></param>
        public ProjectsService(ApplicationDbContext context, IEncryptionService encryptionService)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        }

        /// <summary>
        /// Gets all projects for which the current user has access to. Excludes archived projects.
        /// </summary>
        /// <returns>A list of Project objects</returns>
        public async Task<List<Project>> GetProjects()
        {
            // Validate access

            // Get projects with access TODO
            var projects = await _dbContext.Projects.Where(p=>p.IsArchived == false).ToListAsync();
            foreach (var project in projects)
            {
                project.Title = _encryptionService.DecryptStringAsync(project.Title);
                project.Description = _encryptionService.DecryptStringAsync(project.Description);
            }

            return projects;
        }

        /// <summary>
        /// Retrieves a Project by Project Id, if the user has access to it.
        /// </summary>
        /// <param name="projectId">The Project Id to lookup.</param>
        /// <returns>A Project object, null if none are found or the current user does not have access to it.</returns>
        public async Task<Project> GetProject(int projectId)
        {
            // Validate request
            if (projectId < 0) throw new ArgumentException("You must pass a valid project id.");

            // Validate access TODO


            // Get project
            var result =  await _dbContext.Projects.Where(p => p.Id == projectId && p.IsArchived == false).FirstOrDefaultAsync();

            if (result == null) return null;

            result.Title = _encryptionService.DecryptStringAsync(result.Title);
            result.Description = _encryptionService.DecryptStringAsync(result.Description);

            return result;
        }

        /// <summary>
        /// Encrypts and persists a Project in the database
        /// </summary>
        /// <param name="decryptedProject">The Project object to encrypt and persist</param>
        /// <returns>A Task of int with the Project Id.</returns>
        public async Task<int> CreateProject(Project project)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(project.Title)) throw new ArgumentException("A project must have a title.");

            // Encrypt
            project.Title = _encryptionService.EncryptStringAsync(project.Title);
            project.Description = _encryptionService.EncryptStringAsync(project.Description);

            // Save
            await _dbContext.Projects.AddAsync(project);
            await _dbContext.SaveChangesAsync();

            return project.Id;
        }

        /// <summary>
        /// Discards all tracked changes to the entity and marks it as archived in the database
        /// </summary>
        /// <param name="decryptedProject">The Project entity to archive.</param>
        /// <returns>A Task result</returns>
        public async Task ArchiveProject(Project decryptedProject)
        {
            // Validation
            if (decryptedProject == null) throw new ArgumentException("You must pass a valid project.");

            // Refresh the entity to discard changes and avoid saving a decrypted project
            _dbContext.Entry(decryptedProject).State = EntityState.Unchanged;

            decryptedProject.IsArchived = true; // set archive status

            await _dbContext.SaveChangesAsync(); // save db
        }
    }
}
