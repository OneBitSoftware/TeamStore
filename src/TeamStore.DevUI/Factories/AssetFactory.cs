namespace TeamStore.Factories
{
    using System.Collections.Generic;
    using TeamStore.DevUI.ViewModels;
    using TeamStore.Keeper.Models;

    public static class AssetFactory
    {
        public static List<AssetSearchViewModel> ConvertAssetSearch(List<Asset> assets)
        {
            var result = new List<AssetSearchViewModel>(assets?.Count ?? 0);
            if (assets != null)
            {
                foreach (var asset in assets)
                {
                    result.Add(CreateAssetSearchViewModel(asset));
                }
            }

            return result;
        }

        public static AssetSearchViewModel CreateAssetSearchViewModel(Asset result)
        {
            var viewModel = new AssetSearchViewModel();
            viewModel.AssetId = result.Id;
            viewModel.DisplayTitle = result.Title;
            viewModel.ProjectId = result.ProjectForeignKey;

            return viewModel;
        }
    }
}
