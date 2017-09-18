namespace IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using TeamStore.Models;
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

            // Act - Create and Get Project
            var createdProjectId = await _projectsService.CreateProject(newDecryptedProject);
            var retrievedProject = await _projectsService.GetProject(createdProjectId);

            // Assert before Grant
            Assert.Equal(0, retrievedProject.AccessIdentifiers.Count);

            // Act - Grant access
            await _permissionService.GrantAccess(retrievedProject, "", null, null);
        }
    }
}
