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

    /// <summary>
    /// Responsible for CRUD operations of Project.
    /// </summary>
    public class ProjectsService : IProjectsService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEncryptionService _encryptionService;
        private readonly IPermissionService _permissionService;
        private readonly IApplicationIdentityService _applicationIdentityService;

        /// <summary>
        /// Constructor for the Project Service
        /// </summary>
        /// <param name="context"></param>
        /// <param name="encryptionService"></param>
        public ProjectsService(
            ApplicationDbContext context,
            IEncryptionService encryptionService,
            IApplicationIdentityService applicationIdentityService,
            IPermissionService permissionService)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _applicationIdentityService = applicationIdentityService ?? throw new ArgumentNullException(nameof(applicationIdentityService));
        }

        /// <summary>
        /// Gets all projects for which the current user has access to. Excludes archived projects.
        /// </summary>
        /// <returns>A list of Project objects</returns>
        public async Task<List<Project>> GetProjects()
        {
            // Get user to validate access
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) return null;

            // Get projects with access
            var projects = await _dbContext.Projects.Where(p =>
                p.IsArchived == false)
                .Include(p => p.AccessIdentifiers)
                .ToListAsync();

            var projectsWiithAccess = projects.Where(p =>
                p.AccessIdentifiers.Any(ai => ai.Identity.Id == currentUser.Id));

            foreach (var project in projects)
            {
                DecryptProject(project);
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
            return await GetProject(projectId, false);
        }

        /// <summary>
        /// Retrieves a Project by Project Id, if the user has access to it.
        /// </summary>
        /// <param name="projectId">The Project Id to lookup.</param>
        /// <param name="skipDecryption">Set to True if the project should not be decrypted.</param>
        /// <returns>A Project object, null if none are found or the current user does not have access to it.</returns>
        public async Task<Project> GetProject(int projectId, bool skipDecryption = false)
        {
            // Validate request
            if (projectId < 0) throw new ArgumentException("You must pass a valid project id.");

            // Validate access
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) return null;

            // NOTE: we can't use the permission service, the below results in an infinite loop
            //if(await _permissionService.CurrentUserHasAccessAsync(projectId, this) == false) return null;

            // Get project
            var result = await _dbContext.Projects.Where(p => 
                p.Id == projectId && 
                p.IsArchived == false &&
                p.AccessIdentifiers.Any(ai=>ai.Identity.Id == currentUser.Id)).FirstOrDefaultAsync();

            if (result == null) return null;

            if (skipDecryption == false) // decrypt project
            {
                DecryptProject(result);
            }

            return result;
        }

        /// <summary>
        /// Decrypts all project properties.
        /// </summary>
        /// <param name="result">The Project to decrypt</param>
        private void DecryptProject(Project result)
        {
            result.Title = _encryptionService.DecryptStringAsync(result.Title);
            result.Description = _encryptionService.DecryptStringAsync(result.Description);
            result.Category = _encryptionService.DecryptStringAsync(result.Category);
        }

        /// <summary>
        /// Encrypts and persists a Project in the database
        /// </summary>
        /// <param name="decryptedProject">The Project object to encrypt and persist</param>
        /// <returns>A Task of int with the Project Id.</returns>
        public async Task<int> CreateProject(Project project)
        {
            // Validate title
            if (string.IsNullOrWhiteSpace(project.Title)) throw new ArgumentException("A project must have a title.");

            // Encrypt
            project.Title = _encryptionService.EncryptStringAsync(project.Title);
            project.Description = _encryptionService.EncryptStringAsync(project.Description);
            project.Category = _encryptionService.EncryptStringAsync(project.Category);

            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new ArgumentNullException(nameof(currentUser)); // we fail on no current user

            // Ensure the creating user has Owner permissions to be able to grant access to other users
            project.AccessIdentifiers.Add(new AccessIdentifier() {
                Identity = currentUser,
                Role = "Owner",
                Project = project
            });

            // Set any AccessIdentifier statuses
            foreach (var accessItem in project.AccessIdentifiers)
            {
                accessItem.Created = DateTime.UtcNow;
                accessItem.Modified = DateTime.UtcNow;
                accessItem.CreatedBy = currentUser;
                accessItem.ModifiedBy = currentUser;

                // Access Item Validation
                if (accessItem.CreatedBy == null) throw new ArgumentException("The current user could not be resolved during project createion.");
            }

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


        /// <summary>
        /// Sets a Project's Created/CreatedBy and Modified/ModifiedBy values
        /// </summary>
        /// <param name="project">The Project to set</param>
        //private void SetModifiedAccessControl(Project project)
        //{
        //    foreach (var item in project.AccessIdentifiers)
        //    {
        //        item.Modified = DateTime.UtcNow;
        //        item.ModifiedBy = _permissionService.GetCurrentUser();
        //    }
        //}
    }
}
