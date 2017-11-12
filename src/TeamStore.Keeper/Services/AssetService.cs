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
        /// Sets an Assets archive status to true in the DB
        /// </summary>
        /// <param name="projectId">The project owning the Asset</param>
        /// <param name="assetId">The Asset Id of the asset to archive</param>
        /// <returns>A Task result</returns>
        public async Task ArchiveAssetAsync(int projectId, int assetId, string remoteIpAddress)
        {
            // Validate
            if (projectId < 0) throw new ArgumentException("You must pass a valid project id.");
            if (assetId < 0) throw new ArgumentException("You must pass a valid asset id.");

            // Validate current user
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed."); ;

            // Get Asset from DB with a permission and project check
            var retrievedAsset = await _dbContext.Assets.Where(a =>
                a.Id == assetId &&
                a.IsArchived == false &&
                a.Project.Id == projectId &&
                a.Project.AccessIdentifiers.Any(ai => ai.Identity.Id == currentUser.Id))
                .Include(p => p.Project.AccessIdentifiers)
                .ThenInclude(p => p.Identity) // NOTE: intellisense doesn't work here (23.09.2017) https://github.com/dotnet/roslyn/issues/8237
                .FirstOrDefaultAsync();

            if (retrievedAsset == null) throw new Exception("The asset was not found or the current user does not have access to it.");

            // Refresh the entity to discard changes and avoid saving a decrypted project
            _dbContext.Entry(retrievedAsset).State = EntityState.Unchanged;
            retrievedAsset.IsArchived = true;

            // Set modified time/user
            retrievedAsset.Modified = DateTime.UtcNow;
            retrievedAsset.ModifiedBy = currentUser;

            // LOG event

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Adds an instantiated asset to the database.
        /// Internally, this will encyrpt the asset.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public async Task<Asset> AddAssetToProjectAsync(int projectId, Asset asset, string remoteIpAddress)
        {
            // Validate
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            if (asset.Id != 0) throw new Exception("Updates are not allowed, the asset must be a new asset.");

            // the project must be kept encrypted
            var retrievedProjectEncrypted = await _projectService.GetProject(projectId, true);

            var currentUser = await _applicationIdentityService.GetCurrentUser();
            asset.Project = retrievedProjectEncrypted;
            asset.Created = DateTime.UtcNow;
            asset.CreatedBy = currentUser ?? throw new Exception("Unauthorised requests are not allowed."); // cannot be null here

            // encrypt asset
            EncryptAsset(asset);

            if (asset.Project == null) throw new ArgumentNullException(nameof(asset.Project));

            // persist through context
            await _dbContext.Assets.AddAsync(asset);
            await _dbContext.SaveChangesAsync();

            // LOG event
            await _eventService.LogArchiveProjectEventAsync(projectId, currentUser.Id, remoteIpAddress);
            return asset;
        }

        public async Task<Asset> GetAssetAsync(int projectId, int assetId)
        {
            if (projectId < 0) throw new ArgumentException("You must pass a valid project id.");
            if (assetId < 0) throw new ArgumentException("You must pass a valid asset id.");

            // Validate current user
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed."); ;

            // Get assets with appropriate access checks
            var retrievedAsset = await _dbContext.Assets.Where(a => 
                a.Id == assetId &&
                a.IsArchived == false &&
                a.Project.Id == projectId &&
                a.Project.AccessIdentifiers.Any(ai => ai.Identity.Id == currentUser.Id))
                .Include(p => p.Project.AccessIdentifiers)
                .ThenInclude(p => p.Identity) // NOTE: intellisense doesn't work here (23.09.2017) https://github.com/dotnet/roslyn/issues/8237
                .FirstOrDefaultAsync();

            // LOG access asset

            // decrypt
            if (retrievedAsset == null) return null; 
            DecryptAsset(retrievedAsset);

            return retrievedAsset;
        }

        public async Task<List<Asset>> GetAssetsAsync(int projectId)
        {
            // Validate
            if (projectId < 0) throw new ArgumentException("You must pass a valid project id.");

            // Validate current user
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed."); ;

            // Get assets with appropriate access checks
            var retrievedAssets = await _dbContext.Assets.Where(a =>
                a.IsArchived == false &&
                a.Project.Id == projectId &&
                a.Project.AccessIdentifiers.Any(ai => ai.Identity.Id == currentUser.Id))
                .Include(p => p.Project.AccessIdentifiers)
                .ThenInclude(p => p.Identity) // NOTE: intellisense doesn't work here (23.09.2017) https://github.com/dotnet/roslyn/issues/8237
                .ToListAsync();

            // LOG access asset

            foreach (var asset in retrievedAssets)
            {
                DecryptAsset(asset);
            }

            return retrievedAssets;
        }

        public async Task<Asset> UpdateAssetAsync(int projectId, Asset asset)
        {
            // Validate
            if (projectId < 0) throw new ArgumentException("You must pass a valid project id.");
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            // Encrypt
            EncryptAsset(asset);

            // Validate current user
            var currentUser = await _applicationIdentityService.GetCurrentUser();
            if (currentUser == null) throw new Exception("Unauthorised requests are not allowed."); ;

            // Set modified times
            asset.Modified = DateTime.UtcNow;
            asset.ModifiedBy = currentUser;

            // Persist in DB
            _dbContext.Assets.Update(asset);
            await _dbContext.SaveChangesAsync();

            // LOG Event

            return asset;
        }

        /// <summary>
        /// Performs in-memory encryption of the sensitive properties of an Asset object
        /// </summary>
        /// <param name="asset">The asset to encrypt</param>
        public void EncryptAsset(Asset asset)
        {
            if (asset.GetType() == typeof(Credential))
            {
                var credential = asset as Credential;
                credential.Login = _encryptionService.EncryptString(credential.Login);
                credential.Domain = _encryptionService.EncryptString(credential.Domain);
                credential.Password = _encryptionService.EncryptString(credential.Password);
            }
            else if (asset.GetType() == typeof(Note))
            {
                var note = asset as Note;
                note.Title = _encryptionService.EncryptString(note.Title);
                note.Body = _encryptionService.EncryptString(note.Body);
            }
        }

        /// <summary>
        /// Decrypts a passed Asset object
        /// </summary>
        /// <param name="asset"></param>
        public void DecryptAsset(Asset asset)
        {
            if (asset.GetType() == typeof(Credential))
            {
                var credential = asset as Credential;
                credential.Login = _encryptionService.DecryptString(credential.Login);
                credential.Domain = _encryptionService.DecryptString(credential.Domain);
                credential.Password = _encryptionService.DecryptString(credential.Password);
            }
            else if (asset.GetType() == typeof(Note))
            {
                var note = asset as Note;
                note.Title = _encryptionService.DecryptString(note.Title);
                note.Body = _encryptionService.DecryptString(note.Body);
            }
        }
    }
}
