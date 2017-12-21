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
    }
}
