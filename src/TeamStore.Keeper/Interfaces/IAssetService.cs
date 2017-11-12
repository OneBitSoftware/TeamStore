namespace TeamStore.Keeper.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TeamStore.Keeper.Models;

    public interface IAssetService
    {
        Task<Asset> AddAssetToProjectAsync(int projectId, Asset asset, string remoteIpAddress);

        Task<Asset> UpdateAssetAsync(int projectId, Asset asset);

        Task ArchiveAssetAsync(int projectId, int assetId, string remoteIpAddress);

        Task<Asset> GetAssetAsync(int projectId, int assetId);

        Task<List<Asset>> GetAssetsAsync(int projectId);

        void EncryptAsset(Asset asset);

        void DecryptAsset(Asset asset);
    }
}
