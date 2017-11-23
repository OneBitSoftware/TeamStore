namespace IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TeamStore.Keeper.Models;
    using TeamStore.Keeper.Services;
    using UnitTests.Services;
    using Xunit;

    public class PermissionTests : IntegrationTestBase
    {
        public PermissionTests()
        {

        }

        /// <summary>
        /// Creates a project, retrieves it. Grants access, revokes access. Cleans up.
        /// </summary>
        [Fact]
        public async void GrantRevokeAccess_ShouldReturnCorrectACL()
        {
            // Arrange
            Project newDecryptedProject = new Project();
            newDecryptedProject.Title = "Permission Test Project 1234";
            _fakeHttpContextItems[ApplicationIdentityService.CURRENTUSERKEY] = _testUser;
            var _mockGraphService = _graphService as MockGraphService;
            _mockGraphService.AddUserToInternalList(new ApplicationUser()
            {
                Id = 81,
                AzureAdName = "Mock User 81",
                AzureAdNameIdentifier = "AzureAdNameId-81",
                AzureAdObjectIdentifier = "AzureAdObjectId-81",
                DisplayName = "Mock User Display Name 81",
                TenantId = "Mock Tenant Id 81",
                Upn = "mock@upn.com-81"
            });
            _mockGraphService.AddUserToInternalList(new ApplicationUser()
            {
                Id = 82,
                AzureAdName = "Mock User 82",
                AzureAdNameIdentifier = "AzureAdNameId-82",
                AzureAdObjectIdentifier = "AzureAdObjectId-82",
                DisplayName = "Mock User Display Name 82",
                TenantId = "Mock Tenant Id 82",
                Upn = "mock@upn.com-82"
            });

            // Act - Create and Get Project
            var createdProjectId = await _projectsService.CreateProject(newDecryptedProject);
            var retrievedProject = await _projectsService.GetProject(createdProjectId);

            // Assert before Grant - should be 1 default user
            Assert.NotNull(retrievedProject);
            Assert.Equal(1, retrievedProject.AccessIdentifiers.Count);

            // Act - Grant access
            await _permissionService.GrantAccessAsync(retrievedProject.Id, "mock@upn.com-81", TeamStore.Keeper.Enums.Role.Editor, "127.0.0.1", _projectsService);
            await _permissionService.GrantAccessAsync(retrievedProject.Id, "mock@upn.com-82", TeamStore.Keeper.Enums.Role.Editor, "127.0.0.1", _projectsService);
            await _permissionService.GrantAccessAsync(retrievedProject.Id, "mock@upn.com-82", TeamStore.Keeper.Enums.Role.Editor, "127.0.0.1", _projectsService);
            await _permissionService.GrantAccessAsync(retrievedProject.Id, "mock@upn.com-82", TeamStore.Keeper.Enums.Role.Editor, "127.0.0.1", _projectsService);
            await _permissionService.GrantAccessAsync(retrievedProject.Id, "mock@upn.com-82", TeamStore.Keeper.Enums.Role.Reader, "127.0.0.1", _projectsService);

            // Assert - Grant access
            Assert.Equal(6, retrievedProject.AccessIdentifiers.Count); // 5 + the owner

            // Act - Revoke access
            await _permissionService.RevokeAccessAsync(retrievedProject.Id, "mock@upn.com-82", TeamStore.Keeper.Enums.Role.Editor, "127.0.1.1", _projectsService);
            retrievedProject = await _projectsService.GetProject(createdProjectId);

            // Assert - Revoke access
            Assert.Equal(3, retrievedProject.AccessIdentifiers.Count); // 2 + the owner
            Assert.Equal(_testUser, retrievedProject.AccessIdentifiers
                .FirstOrDefault(ai => ai.Identity.Id == 82 && ai.Role == TeamStore.Keeper.Enums.Role.Reader).CreatedBy);
            Assert.Equal(retrievedProject, retrievedProject.AccessIdentifiers
                .FirstOrDefault(ai => ai.Identity.Id == 81 && ai.Role == TeamStore.Keeper.Enums.Role.Editor).Project);

            // Cleanup
            await _projectsService.ArchiveProject(retrievedProject, "127.0.1.1");
            var archivedProject = await _projectsService.GetProject(createdProjectId, true);
            Assert.Null(archivedProject);
        }
    }
}
