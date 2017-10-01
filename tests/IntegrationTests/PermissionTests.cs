namespace IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TeamStore.Keeper.Models;
    using TeamStore.Keeper.Services;
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

            // Act - Create and Get Project
            var createdProjectId = await _projectsService.CreateProject(newDecryptedProject);
            var retrievedProject = await _projectsService.GetProject(createdProjectId);

            // Assert before Grant - should be 1 default user
            Assert.NotNull(retrievedProject);
            Assert.Equal(1, retrievedProject.AccessIdentifiers.Count);

            // Act - Grant access
            await _permissionService.GrantAccessAsync(retrievedProject.Id, "1234123412-1234-1312-1234-12341234", "Edit", "127.0.0.1", _projectsService);
            await _permissionService.GrantAccessAsync(retrievedProject.Id, "4444555511-6666-7777-8888-12345678", "Edit", "127.0.0.1", _projectsService);
            await _permissionService.GrantAccessAsync(retrievedProject.Id, "4444555511-6666-7777-8888-12345678", "Edit", "127.0.0.1", _projectsService);
            await _permissionService.GrantAccessAsync(retrievedProject.Id, "4444555511-6666-7777-8888-12345678", "Edit", "127.0.0.1", _projectsService);
            await _permissionService.GrantAccessAsync(retrievedProject.Id, "4444555511-6666-7777-8888-12345678", "Read", "127.0.0.1", _projectsService);

            // Assert - Grant access
            Assert.Equal(6, retrievedProject.AccessIdentifiers.Count); // 5 + the owner

            // Act - Revoke access
            await _permissionService.RevokeAccessAsync(retrievedProject.Id, "4444555511-6666-7777-8888-12345678", "Edit", "127.0.1.1", _projectsService);
            retrievedProject = await _projectsService.GetProject(createdProjectId);

            // Assert - Revoke access
            Assert.Equal(3, retrievedProject.AccessIdentifiers.Count); // 2 + the owner
            Assert.Equal(_testUser, retrievedProject.AccessIdentifiers
                .FirstOrDefault(ai => ai.Identity.AzureAdObjectIdentifier == "4444555511-6666-7777-8888-12345678" && ai.Role == "Read").CreatedBy);
            Assert.Equal(retrievedProject, retrievedProject.AccessIdentifiers
                .FirstOrDefault(ai => ai.Identity.AzureAdObjectIdentifier == "1234123412-1234-1312-1234-12341234" && ai.Role == "Edit").Project);

            // Cleanup
            await _projectsService.ArchiveProject(retrievedProject);
            var archivedProject = await _projectsService.GetProject(createdProjectId, true);
            Assert.Null(archivedProject);
        }
    }
}
