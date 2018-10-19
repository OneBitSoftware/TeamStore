namespace TeamStore.Keeper.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Threading.Tasks;
    using TeamStore.Keeper.DataAccess;
    using TeamStore.Keeper.Interfaces;
    using TeamStore.Keeper.Models;
    using Microsoft.EntityFrameworkCore.Extensions;
    using Microsoft.EntityFrameworkCore;

    public class AssetService : IAssetService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IProjectsService _projectService;
        private readonly IEncryptionService _encryptionService;
        private readonly IEventService _eventService;
        private readonly IApplicationIdentityService _applicationIdentityService;

        /// <summary>
        /// Constructor for the AssetService.
        /// </summary>
        /// <param name="context">A database <c>ApplicationDbContext</c></param>
        /// <param name="projectsService">An <c>IProjectsService</c> instance</param>
        public AssetService(ApplicationDbContext context,
            IProjectsService projectsService,
            IEncryptionService encryptionService,
            IEventService eventService,
            IApplicationIdentityService applicationIdentityService
            )
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
            _projectService = projectsService ?? throw new ArgumentNullException(nameof(projectsService));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _applicationIdentityService = applicationIdentityService ?? throw new ArgumentNullException(nameof(applicationIdentityService));
        }

        /// <summary>
        /// Sets an Assets archive status to true in the DB.
        /// </summary>
        /// <param name="projectId">The project Id of the project owning the Asset</param>
        /// <param name="assetId">The Asset Id of the asset to archive</param>
        /// <returns>A Task result</returns>
        public async Task ArchiveAssetAsync(int projectId, int assetId, string remoteIpAddress)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");
            if (projectId < 1) throw new ArgumentException("You must pass a valid project id.");
            if (assetId < 1) throw new ArgumentException("You must pass a valid asset id.");

            // Validate current user
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed.");

            Asset retrievedAsset;

            if (await _applicationIdentityService.IsCurrentUserAdmin())
            {
                // Get Asset from DB with a permission and project check
                retrievedAsset = await _dbContext.Assets.Where(a =>
                    a.Id == assetId &&
                    a.IsArchived == false &&
                    a.Project.Id == projectId)
                    .Include(p => p.Project.AccessIdentifiers)
                    .ThenInclude(p => p.Identity) // NOTE: intellisense doesn't work here (23.09.2017) https://github.com/dotnet/roslyn/issues/8237
                    .FirstOrDefaultAsync();
            }
            else
            {
                // Get Asset from DB with a permission and project check
                retrievedAsset = await _dbContext.Assets.Where(a =>
                    a.Id == assetId &&
                    a.IsArchived == false &&
                    a.Project.Id == projectId &&
                    a.Project.AccessIdentifiers.Any(ai => ai.Identity.Id == currentUser.Id))
                    .Include(p => p.Project.AccessIdentifiers)
                    .ThenInclude(p => p.Identity) // NOTE: intellisense doesn't work here (23.09.2017) https://github.com/dotnet/roslyn/issues/8237
                    .FirstOrDefaultAsync();
            }



            retrievedAsset.IsDecrypted = false;

            if (retrievedAsset == null) throw new Exception("The asset was not found or the current user does not have access to it.");

            // Refresh the entity to discard changes and avoid saving a decrypted project
            //_dbContext.Entry(retrievedAsset).State = EntityState.Unchanged;
            _dbContext.Entry(retrievedAsset.Project).State = EntityState.Unchanged;
            retrievedAsset.Project.IsProjectTitleDecrypted = false;
            retrievedAsset.IsArchived = true;

            // Set modified time/user
            retrievedAsset.Modified = DateTime.UtcNow;
            retrievedAsset.ModifiedBy = currentUser;

            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }

            // decrypt before logging
            // TODO: try-catch
            DecryptAsset(retrievedAsset);
            DecryptProjectTitle(retrievedAsset);

            // LOG event
            await _eventService.LogArchiveAssetEventAsync(
                projectId,
                retrievedAsset.Project.Title,
                remoteIpAddress,
                currentUser.Id,
                currentUser.Upn,
                assetId,
                retrievedAsset.Title,
                retrievedAsset.GetType() == typeof(Credential) ? (retrievedAsset as Credential).Login : string.Empty
                );
        }

        /// <summary>
        /// Adds an instantiated asset to the database.
        /// Internally, this will encyrpt the asset.
        /// </summary>
        /// <param name="projectId">The project Id of the project owning the Asset</param>
        /// <param name="asset">The Asset to add</param>
        /// <returns></returns>
        public async Task<Asset> AddAssetToProjectAsync(int projectId, Asset asset, string remoteIpAddress)
        {
            // Validate
            if (projectId < 1) throw new ArgumentException("You must pass a valid project id.");
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            if (asset.Id != 0) throw new Exception("Updates are not allowed, the asset must be a new asset.");
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

            // the project must be kept encrypted
            var retrievedProjectEncrypted = await _projectService.GetProject(projectId, true);

            var currentUser = await _applicationIdentityService.GetCurrentUser();
            asset.Project = retrievedProjectEncrypted;
            asset.Created = DateTime.UtcNow;
            asset.CreatedBy = currentUser ?? throw new Exception("Unauthorised requests are not allowed."); // cannot be null here

            // encrypt asset
            EncryptAsset(asset);

            if (asset.Project == null) throw new ArgumentNullException(nameof(asset.Project));
            if (string.IsNullOrWhiteSpace(asset.Title)) throw new ArgumentNullException(nameof(asset.Title));

            // persist through context

            // Ensure project is encrypted
            if (asset.Project.IsProjectTitleDecrypted == true || asset.Project.IsDecrypted == true)
            {
                throw new Exception("Saving a decrypted project is not allowed");
            }

            await _dbContext.Assets.AddAsync(asset);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }

            // LOG event TODO: added login or title
            await _eventService.LogCreateAssetEventAsync(projectId, currentUser.Id, remoteIpAddress, asset.Id, string.Empty);
            return asset;
        }

        /// <summary>
        /// Retrieves an Asset for a given Project Id. 
        /// Excludes archived assets and takes permissions into consideration.
        /// </summary>
        /// <param name="projectId">The project Id of the project owning the Asset</param>
        /// <param name="assetId">The Asset Id of the asset to archive</param>
        /// <param name="remoteIpAddress">The IP address of the originating request</param>
        /// <returns>A decrypted Asset object</returns>
        public async Task<Asset> GetAssetAsync(int projectId, int assetId, string remoteIpAddress)
        {
            if (projectId < 1) throw new ArgumentException("You must pass a valid project id.");
            if (assetId < 1) throw new ArgumentException("You must pass a valid asset id.");
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

            // Validate current user
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed.");

            Asset retrievedAsset;

            // Get assets with appropriate access checks
            if (await _applicationIdentityService.IsCurrentUserAdmin())
            {
                retrievedAsset = await _dbContext.Assets.Where(a =>
                a.Id == assetId &&
                a.IsArchived == false &&
                a.Project.Id == projectId)
                .Include(p => p.Project.AccessIdentifiers)
                .ThenInclude(p => p.Identity) // NOTE: intellisense doesn't work here (23.09.2017) https://github.com/dotnet/roslyn/issues/8237
                .FirstOrDefaultAsync();
            }
            else
            {
                retrievedAsset = await _dbContext.Assets.Where(a =>
                a.Id == assetId &&
                a.IsArchived == false &&
                a.Project.Id == projectId &&
                a.Project.AccessIdentifiers.Any(ai => ai.Identity.Id == currentUser.Id))
                .Include(p => p.Project.AccessIdentifiers)
                .ThenInclude(p => p.Identity) // NOTE: intellisense doesn't work here (23.09.2017) https://github.com/dotnet/roslyn/issues/8237
                .FirstOrDefaultAsync();
            }

            string assetLogin = string.Empty;

            // decrypt
            if (retrievedAsset == null) return null;
            retrievedAsset.IsDecrypted = false;

            DecryptAsset(retrievedAsset);
            DecryptProjectTitle(retrievedAsset);

            // extract asset login
            if (retrievedAsset.GetType() == typeof(Credential))
            {
                assetLogin = (retrievedAsset as Credential).Login;
            }

            // log event
            await _eventService.LogAssetAccessEventAsync(
                projectId,
                retrievedAsset.Project.Title,
                currentUser.Id,
                currentUser.Upn,
                remoteIpAddress,
                assetId,
                retrievedAsset.Title,
                assetLogin);

            return retrievedAsset;
        }

        public async Task<List<Asset>> GetAssetsAsync(int projectId)
        {
            // Validate
            if (projectId < 1) throw new ArgumentException("You must pass a valid project id.");

            // Validate current user
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed.");

            List<Asset> retrievedAssets;

            // Get assets with appropriate access checks
            if (await _applicationIdentityService.IsCurrentUserAdmin())
            {
                retrievedAssets = await _dbContext.Assets.Where(a =>
                   a.IsArchived == false &&
                   a.Project.Id == projectId)
                   .Include(p => p.Project.AccessIdentifiers)
                   .ThenInclude(p => p.Identity) // NOTE: intellisense doesn't work here (23.09.2017) https://github.com/dotnet/roslyn/issues/8237
                   .ToListAsync();
            }
            else
            {
                retrievedAssets = await _dbContext.Assets.Where(a =>
                    a.IsArchived == false &&
                    a.Project.Id == projectId &&
                    a.Project.AccessIdentifiers.Any(ai => ai.Identity.Id == currentUser.Id))
                    .Include(p => p.Project.AccessIdentifiers)
                    .ThenInclude(p => p.Identity) // NOTE: intellisense doesn't work here (23.09.2017) https://github.com/dotnet/roslyn/issues/8237
                    .ToListAsync();
            }


            // LOG access asset - open project? TODO

            foreach (var asset in retrievedAssets)
            {
                asset.IsDecrypted = false;
                DecryptAsset(asset);
            }

            return retrievedAssets;
        }

        // searching assets should get all matching assets
        public async Task<List<Asset>> GetAssetResultsAsync(string searchPrefix, int maxResults)
        {
            // Validate current user
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed.");

            List<Asset> retrievedAssets;

            // Get assets with appropriate access checks
            if (await _applicationIdentityService.IsCurrentUserAdmin())
            {
                retrievedAssets = await _dbContext.Assets.Where(a =>
                a.IsArchived == false)
                .ToListAsync();
            }
            else
            {
                retrievedAssets = await _dbContext.Assets.Where(a =>
                a.IsArchived == false &&
                a.Project.AccessIdentifiers.Any(ai => ai.Identity.Id == currentUser.Id))
                .ToListAsync();
            }

            // LOG access asset - open project? TODO
            var assets = new List<Asset>();
            foreach (var asset in retrievedAssets)
            {
                try
                {
                    var title = _encryptionService.DecryptString(asset.Title);
                    if (title.ToLowerInvariant().Contains(searchPrefix.ToLowerInvariant()))
                    {
                        asset.IsDecrypted = false;
                        DecryptAsset(asset);
                        assets.Add(asset);
                    }

                    if (assets.Count >= maxResults)
                    {
                        break;
                    }
                }
                catch // swallow any decryption exceptions here
                {
                    // TODO: LOG
                    // TODO: set "error" in title?
                    continue;
                }
            }

            return assets;
        }

        // Question: Why are we returning on update??

        public async Task<Asset> UpdateAssetAsync(int projectId, Asset asset, string remoteIpAddress)
        {
            // Validate
            if (projectId < 1) throw new ArgumentException("You must pass a valid project id.");
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

            // Validate current user
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed.");

            // CR: check access restrictions

            // Encrypt
            EncryptAsset(asset);

            // Set modified times
            asset.Modified = DateTime.UtcNow;
            asset.ModifiedBy = currentUser;

            // Persist in DB
            _dbContext.Assets.Update(asset);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }

            // LOG Event
            await _eventService.LogUpdateAssetEventAsync(projectId, remoteIpAddress, currentUser.Id, asset.Id);

            return asset;
        }

        // TODO tests: modified date, password as expected, current user changed, all fails
        public async Task<Asset> UpdateAssetPasswordAsync(int projectId, int assetId, string password, string remoteIpAddress)
        {
            // Validate
            if (projectId < 1) throw new ArgumentException("You must pass a valid project id.");
            if (assetId < 1) throw new ArgumentException("You must pass a valid asset id.");
            if (string.IsNullOrWhiteSpace(remoteIpAddress)) throw new ArgumentException("You must provide a valid IP address.");

            // Validate current user
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed.");

            //retrieve asset (performs current user check and permission check)
            var asset = await this.GetAssetAsync(projectId, assetId, remoteIpAddress);
            var credential = asset as Credential;

            // set new password and encrypt
            credential.Password = password;
            EncryptAsset(credential);

            // Set modified times
            credential.Modified = DateTime.UtcNow;
            credential.ModifiedBy = currentUser;

            _dbContext.Entry(asset.Project).State = EntityState.Unchanged;

            // Persist in DB
            _dbContext.Assets.Update(credential);
            var updatedRowCount = await _dbContext.SaveChangesAsync();
            if (updatedRowCount > 1)
            {
                // we have a problem
            }

            // LOG Event
            await _eventService.LogUpdatePasswordEventAsync(projectId, remoteIpAddress, currentUser.Id, asset.Id);

            return asset;
        }

        /// <summary>
        /// Loads the Assets for a given Project explicitly, then decrypts them. 
        /// Used when the initial Projects query does not explicitly include them. 
        /// This should not be called if the Project assets are already loaded.
        /// </summary>
        /// <param name="project">The Project for which to populate the assets</param>
        /// <returns>The populated Project</returns>
        public async Task LoadAssetsAsync(Project project)
        {
            if (project == null) return;

            // NOTE: this will ignore assets already loaded, so the IsArchived status might not be truthful.
            // Avoid using this method if the project assets are already populated
            await _dbContext.Entry(project)
                .Collection(p => p.Assets)
                .Query()
                .Where(asset => asset.IsArchived == false)
                .LoadAsync();

            foreach (var asset in project.Assets)
            {
                asset.IsDecrypted = false;
                DecryptAsset(asset);
            }
        }

        /// <summary>
        /// Performs in-memory encryption of the sensitive properties of an Asset object
        /// </summary>
        /// <param name="asset">The asset to encrypt</param>
        public void EncryptAsset(Asset asset)
        {
            if (asset.IsDecrypted == false)
            {
                return;
            }

            // TODO: create a test to validate the argumentnullexception
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            if (string.IsNullOrWhiteSpace(asset.Title)) throw new Exception("An asset title cannot be an empty string. Cannot encrypt and decrypt.");

            if (asset.GetType() == typeof(Credential))
            {
                var credential = asset as Credential;

                if (string.IsNullOrWhiteSpace(credential.Password)) throw new Exception("A password cannot be an empty string. Cannot encrypt and decrypt.");
                if (string.IsNullOrWhiteSpace(credential.Login)) throw new Exception("A login cannot be an empty string. Cannot encrypt and decrypt.");

                credential.Login = _encryptionService.EncryptString(credential.Login);
                credential.Password = _encryptionService.EncryptString(credential.Password);
            }
            else if (asset.GetType() == typeof(Note))
            {
                //var note = asset as Note; // currently no need to encrypt anything
            }

            asset.Title = _encryptionService.EncryptString(asset.Title);
            if (string.IsNullOrWhiteSpace(asset.Notes) == false)
            {
                asset.Notes = _encryptionService.EncryptString(asset.Notes);
            }

            asset.IsDecrypted = false;
        }

        /// <summary>
        /// Decrypts all properties of given asset, excluding the Password.
        /// </summary>
        /// <param name="asset">The Asset to decrypt</param>
        public void DecryptAsset(Asset asset)
        {
            if (asset.IsDecrypted == true)
            {
                return;
            }

            if (asset.GetType() == typeof(Credential))
            {
                var credential = asset as Credential;
                credential.Login = _encryptionService.DecryptString(credential.Login);
            }
            else if (asset.GetType() == typeof(Note))
            {
                //var note = asset as Note; // currently no need to encrypt anything
            }

            asset.Title = _encryptionService.DecryptString(asset.Title);

            if (string.IsNullOrWhiteSpace(asset.Notes) == false)
            {
                asset.Notes = _encryptionService.DecryptString(asset.Notes);
            }

            asset.IsDecrypted = true;
        }

        /// <summary>
        /// Decrypts a given password.
        /// </summary>
        /// <param name="password">The password to decrypt</param>
        public string DecryptPassword(string encryptedPassword)
        {
            return _encryptionService.DecryptString(encryptedPassword);
        }

        private void DecryptProjectTitle(Asset asset)
        {
            if (asset?.Project?.IsProjectTitleDecrypted == false)
            {
                var projectTitle = asset?.Project?.Title;

                if (string.IsNullOrWhiteSpace(projectTitle) == false)
                {
                    asset.Project.Title = _encryptionService.DecryptString(projectTitle);
                    asset.Project.IsProjectTitleDecrypted = true;
                }
            }
        }

        private void EncryptProjectTitle(Asset asset)
        {
            if (asset?.Project?.IsProjectTitleDecrypted == true)
            {
                var projectTitle = asset?.Project?.Title;

                if (string.IsNullOrWhiteSpace(projectTitle) == false)
                {
                    asset.Project.Title = _encryptionService.EncryptString(projectTitle);
                    asset.Project.IsProjectTitleDecrypted = false;
                }
            }
        }
    }
}
