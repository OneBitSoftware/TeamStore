using System;
using System.Collections.Generic;
using System.Text;
using TeamStore.Keeper.Models;
using TeamStore.Keeper.Services;
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
        
        [Fact]
        public async void IsCurrentUserAdmin_ShouldReturnFalse()
        {
            // Arrange

            // Act
            var result = await _applicationIdentityService.IsCurrentUserAdmin();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async void IsCurrentUserAdmin_ShouldReturnTrue()
        {
            // Arrange
            _fakeHttpContextItems.Add(ApplicationIdentityService.CURRENTUSERKEY, _testUser);

            // Act
            var setResult = await _applicationIdentityService.SetSystemAdministrator(_testUser);
            var result = await _applicationIdentityService.IsCurrentUserAdmin();

            // Assert
            Assert.True(setResult);
            Assert.True(result);

            // Cleanup - remove then check and assert again
            var removeResult = await _applicationIdentityService.RemoveSystemAdministrator(_testUser);
            var resultAfterCleanup = await _applicationIdentityService.IsCurrentUserAdmin();
            Assert.True(removeResult);
            Assert.False(resultAfterCleanup);
        }
    }
}
