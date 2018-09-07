using System;
using System.Collections.Generic;
using TeamStore.Factories;
using TeamStore.Keeper.Models;
using TeamStore.DevUI.ViewModels;
using Xunit;

namespace UnitTests
{
    public class AssetFactoryTests
    {
        public AssetFactoryTests()
        {
        }

        [Fact]
        public void ConvertAssetSearch_ShouldNotFailOnNullArgument()
        {
            try
            {
                AssetFactory.ConvertAssetSearch(null);
            }
            catch (NullReferenceException)
            {
                Assert.False(true);
            }

            Assert.True(true);
        }

        [Theory,
            InlineData(-1),
            InlineData(-1239),
            InlineData(0)]
        public void CreateAssetSearchViewModel_InvalidKeysShouldGetException(int id)
        {
            Asset asset = new Note() { Id = id, ProjectForeignKey = 1};
            try
            {
                AssetFactory.CreateAssetSearchViewModel(asset);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }

            asset = new Credential() { Id = 1, ProjectForeignKey = id };
            try
            {
                AssetFactory.CreateAssetSearchViewModel(asset);
            }
            catch (ArgumentException)
            {
                Assert.True(true);
            }
        }
    }
}
