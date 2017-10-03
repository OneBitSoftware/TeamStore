using System;
using System.Collections.Generic;
using System.Text;
using TeamStore.Keeper.Models;
using Xunit;

namespace IntegrationTests
{
    public class ApplicationIdentityServiceTests : IntegrationTestBase
    {
        [Fact]
        public async void FindUserAsync_ShouldReturnNullUser()
        {
            var findNonExistantResult = await _applicationIdentityService
                .FindUserAsync(ai => ((ApplicationUser)ai).Upn == "notexistant");

            Assert.Null(findNonExistantResult);
        }
    }
}
