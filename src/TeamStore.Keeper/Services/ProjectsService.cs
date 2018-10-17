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
        /// Gets all projects for which the current user has access to. Can include archived projects. Can skip decryption.
        /// </summary>
        /// <returns>A list of Project objects</returns>
        public async Task<List<Project>> GetProjects(bool skipDecryption = false, bool includeArchived = false)
        {
            // Get user to validate access
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed.");

            if (await _applicationIdentityService.IsCurrentUserAdmin())
            {
                return await GetProjectsForAdmin(skipDecryption);
            }

            // Get projects with access
            // TODO: attempt to make this in 1 query
            var projects = new List<Project>();

            if (includeArchived) // filter out archived
            {
                projects = await _dbContext.Projects
                    .Include(p => p.AccessIdentifiers)
                    .ToListAsync();
            }
            else
            {
                projects = await _dbContext.Projects
                    .Where(p => p.IsArchived == false)
                    .Include(p => p.AccessIdentifiers)
                    .ToListAsync();
            }

            // filter our those without access for the current user
            var projectsWithAccess = projects.Where(p =>
                p.AccessIdentifiers.Any(ai => ai.Identity != null && ai.Identity.Id == currentUser.Id))
                .ToList();

            if (projectsWithAccess == null) return null;

            if (skipDecryption == false) // double false...
            {
                foreach (var project in projectsWithAccess)
                {
                    DecryptProject(project);
                }
            }

            return projectsWithAccess.ToList();
        }

        /// <summary>
        /// Gets all archived projects. Requires system administrator access. Can skip decryption.
        /// </summary>
        /// <returns>A list of Project objects</returns>
        public async Task<List<Project>> GetArchivedProjectsAsync(bool skipDecryption = false)
        {
            // Get user to validate access
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed.");

            // check system administrator permissions
            if (await _applicationIdentityService.IsCurrentUserAdmin() == false)
            {
                throw new Exception("This operation is only allowed for system administrators");
            }

            var archivedProjects = await _dbContext.Projects
                .Where(p => p.IsArchived == true)
                .Include(p => p.AccessIdentifiers)
                .Include(p => p.Assets)
                .ToListAsync();

            if (skipDecryption == false) // double false...
            {
                foreach (var project in archivedProjects)
                {
                    DecryptProject(project);
                }
            }

            return archivedProjects;
        }

        public async Task<List<Project>> GetArchivedProjectsAsync(DateTime startDateTime, DateTime endDateTime, string projectTitle = "", bool skipDecryption = false)
        {
            List<Project> archivedProjects =  await this.GetArchivedProjectsAsync(skipDecryption);
            List<Project> filteredProjects = archivedProjects
                .Where(p => (projectTitle != "" ? p.Title == projectTitle : true))
                .ToList();

            for (int i = 0; i < filteredProjects.Count(); i++)
            {
                filteredProjects[i].Assets = filteredProjects[i].Assets
                    .Where(a => (a.Modified >= startDateTime && a.Modified <= endDateTime))
                    .ToList();
            }

            return filteredProjects;
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
                p.AccessIdentifiers.Any(ai => ai.Identity.Id == currentUser.Id))
                .Include(p => p.AccessIdentifiers)
                .ThenInclude(p => p.Identity) // NOTE: intellisense doesn't work here (23.09.2017) https://github.com/dotnet/roslyn/issues/8237
                .FirstOrDefaultAsync();

            if (result == null) return null;

            result.IsDecrypted = false;

            // this line makes sure that the retrieved project is not retrieved
            // from EF's cache, as it will go through decryption and fail
            // It might be better to have a decrypted status rather than tinker with EF's state
            //_dbContext.Entry(result).State = EntityState.Unchanged;

            if (skipDecryption == false) // decrypt project
            {
                DecryptProject(result);
            }

            if (skipDecryption == true && result.IsDecrypted == true)
            {
                EncryptProject(result);
            }

            return result;
        }

        /// <summary>
        /// Retrieves all projects, ignoring any access identifier restrictions.
        /// Used for a database export only.
        /// </summary>
        /// <param name="skipDecryption">Wether to return encrypted projects</param>
        /// <returns>A Task result of a List of Projects.</returns>
        private async Task<List<Project>> GetProjectsForAdmin(bool skipDecryption = false, bool includeArchived = false)
        {
            var projects = new List<Project>();

            if (includeArchived)
            {
                projects = await _dbContext.Projects
                    .Include(p => p.AccessIdentifiers)
                    .ToListAsync();
            }
            else
            {
                projects = await _dbContext.Projects.Where(p => p
                    .IsArchived == includeArchived)
                    .Include(p => p.AccessIdentifiers)
                    .ToListAsync();
            }

            foreach (var project in projects) //ensure status is decrypted after DB retrieval
            {
                project.IsDecrypted = false;
            }

            if (skipDecryption == false) // double false...
            {
                foreach (var project in projects)
                {
                    DecryptProject(project);
                }
            }

            return projects.ToList();
        }

        /// <summary>
        /// Decrypts all project properties.
        /// </summary>
        /// <param name="result">The Project to decrypt</param>
        private void DecryptProject(Project result)
        {
            if (result.IsDecrypted == false)
            {
                try
                {
                    // in case the title is persisted with a non-decrypted string, this helps recover the application.
                    result.Title = _encryptionService.DecryptString(result.Title);
                }
                catch
                {
                    result.Title = "Decryption error";
                }

                result.Description = _encryptionService.DecryptString(result.Description);
                result.Category = _encryptionService.DecryptString(result.Category);
                result.IsDecrypted = true; 
                result.IsProjectTitleDecrypted = true;
            }
        }

        /// <summary>
        /// Encrypts and persists a Project in the database.
        /// </summary>
        /// <param name="project">The Project object to encrypt and persist</param>
        /// <returns>A Task of int with the Project Id.</returns>
        public async Task<int> CreateProject(Project project, string remoteIpAddress)
        {
            // Validate title
            if (string.IsNullOrWhiteSpace(project.Title)) throw new ArgumentException("A project must have a title.");

            // Encrypt
            EncryptProject(project);

            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed."); // we fail on no current user

            // Ensure the creating user has Owner permissions to be able to grant access to other users
            // It is important to distinguish between creating through a UI call vs importing projects
            // This method is used in both cases
            if (project.AccessIdentifiers.Any(ai => ai.Identity?.AzureAdObjectIdentifier == currentUser.AzureAdObjectIdentifier) == false)
            {
                project.AccessIdentifiers.Add(new AccessIdentifier()
                {
                    Identity = currentUser,
                    Role = Enums.Role.Owner,
                    Project = project
                });
            }

            // Set any AccessIdentifier statuses
            foreach (var accessItem in project.AccessIdentifiers)
            {
                accessItem.Created = DateTime.UtcNow;
                accessItem.CreatedBy = currentUser;
                // Modified is not set in the create routine

                // Access Identifier Validation
                if (accessItem.CreatedBy == null) throw new ArgumentException("The current user could not be resolved during project creation.");
            }

            // Save
            await _dbContext.Projects.AddAsync(project);
            var updatedRowCount = await _dbContext.SaveChangesAsync(); // returns 2 or 3 (currentUser)

            if (project.IsProjectTitleDecrypted == true)
            {
                throw new Exception("Saving a decrypted project is not allowed");
            }

            // LOG event
            await _eventService.LogUpdateProjectEventAsync(project.Id, currentUser.Id, remoteIpAddress);

            return project.Id;
        }

        private void EncryptProject(Project project)
        {
            if (project.IsDecrypted == true)
            {
                project.Title = _encryptionService.EncryptString(project.Title);
                project.Description = _encryptionService.EncryptString(project.Description);
                project.Category = _encryptionService.EncryptString(project.Category);
                project.IsDecrypted = false;
                project.IsProjectTitleDecrypted = false;
            }
        }

        /// <summary>
        /// Imports and persists a Project into the database.
        /// This is designed to be used by a database import.
        /// </summary>
        /// <param name="decryptedProject">The Project object to encrypt and persist</param>
        /// <returns>A Task of int with the Project Id.</returns>
        public async Task<int> ImportProject(Project project, string remoteIpAddress)
        {
            // reset all Id's of the entity hierarchy to avoid primary key conflicts
            project.Id = 0;
            project.AccessIdentifiers.All(ai => { ai.Id = 0; return true; });
            project.Assets.All(a => { a.Id = 0; return true; });

            // this logic will fail if we persist a project with decrypted assets
            // thus we just run through the decryptor to check and allow it to throw on import
            // if the assets are decrypted, rather than persist decrypted
            foreach (var asset in project.Assets)
            {
                _encryptionService.DecryptString(asset.Title); // will throw if decrypted
            }

            // if this is a fresh database the current user might not exists yet
            // causing double tracking to exist in EF, which throws.
            // the workaround is to save the user, so it has a valid Id
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser.Id == 0)
            {
                _dbContext.ApplicationIdentities.Add(currentUser);
                await _dbContext.SaveChangesAsync(); // save the current user in the database
            }

            return await CreateProject(project, remoteIpAddress);
        }

        /// <summary>
        /// Discards all tracked changes to the entity and marks it as archived in the database
        /// </summary>
        /// <param name="decryptedProject">The Project entity to archive.</param>
        /// <param name="remoteIpAddress">The IP address of the request causing the event</param>
        /// <returns>A Task result</returns>
        public async Task ArchiveProject(Project decryptedProject, string remoteIpAddress)
        {
            // Validation
            if (decryptedProject == null) throw new ArgumentException("You must pass a valid project.");

            // TODO: ensure the current user has access to archive this project
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            var currentRetrievedUser = _applicationIdentityService.FindUserAsync(u => u.AzureAdObjectIdentifier == currentUser.AzureAdObjectIdentifier).Result;

            if (currentRetrievedUser == null) throw new Exception("Unauthorised requests are not allowed."); // we fail on no current user

            if (await _permissionService.CheckAccessAsync(decryptedProject.Id, currentRetrievedUser, Enums.Role.Owner, this) == false)
            {
                // CR: this should rather return a failed result
                throw new Exception("The current user does not have enough permissions to archive this project.");
            }

            _dbContext.Entry(decryptedProject).Collection(x => x.Assets).Load();
            _dbContext.Entry(currentUser).State = EntityState.Detached;
            _dbContext.Entry(currentRetrievedUser).State = EntityState.Detached;

            // Refresh the entity to discard changes and avoid saving a decrypted project
            //_dbContext.Entry(decryptedProject).State = EntityState.Unchanged;

            // Refresh assets and set to archived
            foreach (var asset in decryptedProject.Assets)
            {
                //_dbContext.Entry(asset.CreatedBy).State = EntityState.Unchanged;
                //_dbContext.Entry(asset).State = EntityState.Unchanged;
                asset.IsArchived = true;
                asset.Modified = DateTime.UtcNow;
                asset.ModifiedBy = currentRetrievedUser;

                // LOG asset archive event
                await _eventService.LogArchiveAssetEventAsync(decryptedProject.Id, remoteIpAddress, currentRetrievedUser.Id, asset.Id);
            }

            decryptedProject.IsArchived = true; // set archive status

            await _eventService.LogArchiveProjectEventAsync(decryptedProject.Id, currentRetrievedUser.Id, remoteIpAddress);

            var entries = _dbContext.ChangeTracker.Entries()
                .Where(e=>
                    e.Entity.GetType() == typeof(ApplicationUser) 
                    && e.State == EntityState.Added);

            foreach (var item in entries)
            {
                _dbContext.Entry(item).State = EntityState.Detached;

            }

            var modifiedRows = await _dbContext.SaveChangesAsync(); // save to db
        }

        public async Task ArchiveProject(Project decryptedProject, string remoteIpAddress, ApplicationUser actingUser)
        {
            // Validation
            if (decryptedProject == null) throw new ArgumentException("You must pass a valid project.");

            // TODO: ensure the current user has access to archive this project

            if (await _permissionService.CheckAccessAsync(decryptedProject.Id, actingUser, Enums.Role.Owner, this) == false)
            {
                // CR: this should rather return a failed result
                throw new Exception("The current user does not have enough permissions to archive this project.");
            }

            _dbContext.Entry(decryptedProject).Collection(x => x.Assets).Load();

            // Refresh the entity to discard changes and avoid saving a decrypted project
            //_dbContext.Entry(decryptedProject).State = EntityState.Unchanged;

            // Refresh assets and set to archived
            foreach (var asset in decryptedProject.Assets)
            {
                //_dbContext.Entry(asset.CreatedBy).State = EntityState.Unchanged;
                //_dbContext.Entry(asset).State = EntityState.Unchanged;
                asset.IsArchived = true;
                asset.Modified = DateTime.UtcNow;
                asset.ModifiedBy = actingUser;

                // LOG asset archive event
                await _eventService.LogArchiveAssetEventAsync(decryptedProject.Id, remoteIpAddress, actingUser.Id, asset.Id);
            }

            decryptedProject.IsArchived = true; // set archive status

            await _eventService.LogArchiveProjectEventAsync(decryptedProject.Id, actingUser.Id, remoteIpAddress);

            var entries = _dbContext.ChangeTracker.Entries()
                .Where(e =>
                    e.Entity.GetType() == typeof(ApplicationUser)
                    && e.State == EntityState.Added);

            foreach (var item in entries)
            {
                _dbContext.Entry(item).State = EntityState.Detached;
            }

            var modifiedRows = await _dbContext.SaveChangesAsync(); // save to db
        }

        public async Task UpdateProject(Project updatedProject, string remoteIpAddress)
        {
            // Validate
            if (updatedProject.Id < 1) throw new ArgumentException("You must pass a valid project id.");

            // Validate current user
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed."); ;

            // TODO: this code reloads the project entity from the DB, causing it to persist encrypted strings. 
            //if (await _permissionService.CheckAccessAsync(updatedProject.Id, currentUser, Enums.Role.Owner, this) == false)
            //{
            //    // CR: this should rather return a failed result
            //    throw new Exception("Only owners can update projects.");
            //}

            // validate that access hasn't changed by getting a fresh, NoTracking copy of the project.
            var retrievedProject = await _dbContext.Projects.Where(p =>
                p.Id == updatedProject.Id &&
                p.IsArchived == false &&
                p.AccessIdentifiers.Any(ai => ai.Identity.Id == currentUser.Id))
                .Include(p => p.AccessIdentifiers)
                .ThenInclude(p => p.Identity)
                .AsNoTracking()  // <--
                .FirstOrDefaultAsync();

            // iterate through all untouched Access Identifiers
            foreach (var accessIdentifier in updatedProject.AccessIdentifiers)
            {
                // check if the project-to-be-saved has an identity that doesn't already have access
                if (retrievedProject.AccessIdentifiers.Any(ai =>
                    ai.Identity.AzureAdObjectIdentifier == accessIdentifier.Identity.AzureAdObjectIdentifier) == false)
                {
                    // throw if such an item exists.
                    throw new Exception("You cannot update a project's access list unless you are sharing access.");
                }
            }

            // this is an alternative check with .Intersect
            var matchingItems = updatedProject.AccessIdentifiers.Select(ai => ai.Identity.AzureAdObjectIdentifier)
                .Intersect(retrievedProject.AccessIdentifiers.Select(ai => ai.Identity.AzureAdObjectIdentifier));
            if (matchingItems.Count() != updatedProject.AccessIdentifiers.Count())
            {
                throw new Exception("You cannot update a project's access list unless you are sharing access.");
            }

            // Encrypt
            EncryptProject(updatedProject);

            // Persist in DB
            var updatedRowCount = await _dbContext.SaveChangesAsync(); // save to db

            // LOG Event
            await _eventService.LogUpdateProjectEventAsync(updatedProject.Id, currentUser.Id, remoteIpAddress);
        }

        public async Task LoadAssetsForProjectAsync(Project project)
        {
            await _dbContext.Entry(project).Collection(a => a.Assets).LoadAsync();
        }
    }
}
