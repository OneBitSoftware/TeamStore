namespace TeamStore.Keeper.Services
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TeamStore.Keeper.DataAccess;
    using TeamStore.Keeper.Interfaces;
    using TeamStore.Keeper.Models;

    /// <summary>
    /// Responsible for CRUD operations of Project.
    /// </summary>
    public class ProjectsService : IProjectsService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEncryptionService _encryptionService;
        private readonly IEventService _eventService;
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
            IEventService eventService,
            IApplicationIdentityService applicationIdentityService,
            IPermissionService permissionService)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _applicationIdentityService = applicationIdentityService ?? throw new ArgumentNullException(nameof(applicationIdentityService));
        }

        /// <summary>
        /// Gets all projects for which the current user has access to. Excludes archived projects.
        /// </summary>
        /// <returns>A list of Project objects</returns>
        public async Task<List<Project>> GetProjects(bool skipDecryption = false)
        {
            // Get user to validate access
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed.");

            // Get projects with access
            var projects = await _dbContext.Projects.Where(p =>
                p.IsArchived == false)
                .Include(p => p.AccessIdentifiers)
                .ToListAsync();

            var projectsWiihAccess = projects.Where(p =>
                p.AccessIdentifiers.Any(ai => ai.Identity.Id == currentUser.Id));

            if (skipDecryption == false)
            {
                foreach (var project in projectsWiihAccess)
                {
                    DecryptProject(project);
                } 
            }

            return projectsWiihAccess.ToList();
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
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed."); ;

            // Get project - ensures user has access to it
            var result = await _dbContext.Projects.Where(p =>
                p.Id == projectId &&
                p.IsArchived == false &&
                p.AccessIdentifiers.Any(ai=>ai.Identity.Id == currentUser.Id))
                .Include(p => p.AccessIdentifiers)
                .ThenInclude(p=>p.Identity) // NOTE: intellisense doesn't work here (23.09.2017) https://github.com/dotnet/roslyn/issues/8237
                .FirstOrDefaultAsync();

            if (result == null) return null;

            // this line makes sure that the retrieved project is not retrieved
            // from EF's cache, as it will go through decryption and fail
            // It might be better to have a decrypted status rather than tinker with EF's state
            _dbContext.Entry(result).State = EntityState.Unchanged;

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
            result.Title = _encryptionService.DecryptString(result.Title);
            result.Description = _encryptionService.DecryptString(result.Description);
            result.Category = _encryptionService.DecryptString(result.Category);
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
            project.Title = _encryptionService.EncryptString(project.Title);
            project.Description = _encryptionService.EncryptString(project.Description);
            project.Category = _encryptionService.EncryptString(project.Category);

            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed."); // we fail on no current user

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
                //accessItem.Modified = DateTime.UtcNow; // We should only set Modified/By when an update occurs
                accessItem.CreatedBy = currentUser;
                //accessItem.ModifiedBy = currentUser;

                // Access Item Validation
                if (accessItem.CreatedBy == null) throw new ArgumentException("The current user could not be resolved during project createion.");
            }

            // Save
            await _dbContext.Projects.AddAsync(project);
            await _dbContext.SaveChangesAsync();

            // LOG event

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

            // TODO: ensure the current user has access to archive this project

            // Refresh the entity to discard changes and avoid saving a decrypted project
            _dbContext.Entry(decryptedProject).State = EntityState.Unchanged;

            // Refresh assets and set to archived
            foreach (var asset in decryptedProject.Assets)
            {
                _dbContext.Entry(asset).State = EntityState.Unchanged;
                asset.IsArchived = true;
            }

            decryptedProject.IsArchived = true; // set archive status

            // LOG EVENT

            await _dbContext.SaveChangesAsync(); // save db
        }
    }
}
