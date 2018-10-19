﻿namespace TeamStore.Keeper.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TeamStore.Keeper.Models;

    public interface IAssetService
    {
        Task<Asset> AddAssetToProjectAsync(int projectId, Asset asset, string remoteIpAddress);

        Task<Asset> UpdateAssetAsync(int projectId, Asset asset, string remoteIpAddress);

        Task<Asset> UpdateAssetPasswordAsync(int projectId, int assetId, string password, string remoteIpAddress);

        Task ArchiveAssetAsync(int projectId, int assetId, string remoteIpAddress);

        Task<Asset> GetAssetAsync(int projectId, int assetId, string remoteIpAddress);

        Task<List<Asset>> GetAssetsAsync(int projectId);

        Task<List<Asset>> GetAssetResultsAsync(string searchPrefix, int maxResults);

        /// <summary>
        /// Returns list of all assets that are not archived.
        /// </summary>
        Task<List<Asset>> GetAllAssetsAsync();

        /// <summary>
        /// Loads the Assets for a given Project explicitly. Used when the initial Projects query does not
        /// explicitly include them.
        /// </summary>
        /// <param name="project">The Project for which to populate the assets</param>
        /// <returns>The populated Project</returns>
        Task LoadAssetsAsync(Project project);

        void EncryptAsset(Asset asset);

        /// <summary>
        /// Decrypts all properties of given asset, excluding the Password.
        /// </summary>
        /// <param name="asset">The Asset to decrypt</param>
        void DecryptAsset(Asset asset);

        /// <summary>
        /// Decrypts a given password.
        /// </summary>
        /// <param name="password">The password to decrypt</param>
        /// <returns>The decrypted password</returns>
        string DecryptPassword(string password);

        Task<List<Asset>> GetAllStaleAssets(DateTime staleDate);
        Task<List<Asset>> GetAllUnusedAssets(DateTime staleDate);
    }
}