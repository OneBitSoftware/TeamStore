namespace IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using TeamStore.Models;
    using TeamStore.Services;
    using Xunit;

    public class PermissionTests : IntegrationTestBase
    {
        public PermissionTests()
        {

        }

        [Fact]
        public async void GrantAccess_ShouldReturnCorrectACL()
        {
            // Arrange
            Project newDecryptedProject = new Project();
            newDecryptedProject.Title = "Permission Test Project 1234";
            _fakeHttpContextItems[ApplicationIdentityService.CURRENTUSERKEY] = _testUser;

            // Act - Create and Get Project
            var createdProjectId = await _projectsService.CreateProject(newDecryptedProject);
            var retrievedProject = await _projectsService.GetProject(createdProjectId);

            // Assert before Grant - should be 1 default user
            Assert.Equal(1, retrievedProject.AccessIdentifiers.Count);

            // Act - Grant access
            await _permissionService.GrantAccessAsync(retrievedProject.Id, "1234123412-1234-1312-1234-12341234", "Edit", _testUser, "127.0.0.1", _projectsService);
            await _permissionService.GrantAccessAsync(retrievedProject.Id, "4444555511-6666-7777-8888-12345678", "Edit", _testUser, "127.0.0.1", _projectsService);
            await _permissionService.GrantAccessAsync(retrievedProject.Id, "4444555511-6666-7777-8888-12345678", "Read", _testUser, "127.0.0.1", _projectsService);

            Assert.Equal(4, retrievedProject.AccessIdentifiers.Count);

            await _permissionService.RevokeAccessAsync(retrievedProject.Id, "4444555511-6666-7777-8888-12345678", "Edit", _testUser, "127.0.1.1", _projectsService);
            retrievedProject = await _projectsService.GetProject(createdProjectId);

            Assert.Equal(3, retrievedProject.AccessIdentifiers.Count);

        }
    }
}
