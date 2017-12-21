namespace TeamStore.Factories
{
    using System;
    using System.Collections.Generic;
    using TeamStore.DevUI.ViewModels;
    using TeamStore.Keeper.Models;
    using Microsoft.ApplicationInsights;

    public static class AssetFactory
    {
        public static List<AssetSearchViewModel> ConvertAssetSearch(List<Asset> assets)
        {
            var result = new List<AssetSearchViewModel>(assets?.Count ?? 0);
            if (assets != null)
            {
                foreach (var asset in assets)
                {
                    try
                    {
                        var assetViewModel = CreateAssetSearchViewModel(asset);
                        result.Add(assetViewModel);
                    }
                    catch (ArgumentException ex)
                    {
                        var t = new TelemetryClient();
                        t.TrackException(ex);
                    }

                }
            }

            return result;
        }

        public static AssetSearchViewModel CreateAssetSearchViewModel(Asset result)
        {
            if (result.Id < 1)
            {
                throw new ArgumentException($"Invalid Asset primary key: {result.Id}");
            }

            if (result.ProjectForeignKey < 1)
            {
                throw new ArgumentException($"Invalid Asset foreign key: {result.ProjectForeignKey}");
            }

            var viewModel = new AssetSearchViewModel();
            viewModel.AssetId = result.Id;
            viewModel.DisplayTitle = result.Title;
            viewModel.ProjectId = result.ProjectForeignKey;

            return viewModel;
        }
    }
}
