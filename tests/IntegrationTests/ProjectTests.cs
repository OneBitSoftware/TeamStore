namespace IntegrationTests
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using TeamStore.Keeper.Enums;
    using TeamStore.Keeper.Models;
    using TeamStore.Keeper.Services;
    using UnitTests.Services;
    using Xunit;

    public class ProjectTests : IntegrationTestBase
    {
        public ProjectTests()
        {

        }

        /// <summary>
        /// Creates a project, then retrieves it. Validates that the properties are the same.
        /// Goes through encryption
        /// </summary>
        [Fact]
        public async void CreateProject_ShouldReturnCorrectData()
        {
            // Arrange
            _fakeHttpContextItems.Add(ApplicationIdentityService.CURRENTUSERKEY, _testUser);
            string testTitle = "Project 1234 Test";
            string testDescription = "Created during integration tests";
            string testCategory = "Integration Tests";
            Project newDecryptedProject = new Project();
            newDecryptedProject.Title = testTitle;
            newDecryptedProject.Description = testDescription;
            newDecryptedProject.Category = testCategory;

            // Act
            var createdProjectId = await _projectsService.CreateProject(newDecryptedProject, "127.0.1.1");
            var retrievedProject = await _projectsService.GetProject(createdProjectId);

            // Assert
            Assert.Equal(retrievedProject.Title, testTitle);
            Assert.Equal(retrievedProject.Description, testDescription);
            Assert.Equal(retrievedProject.Category, testCategory);
            Assert.True(retrievedProject.Id > 0);

            // Cleanup
            await _projectsService.ArchiveProject(retrievedProject, "127.0.1.1");
            var archivedProject = await _projectsService.GetProject(createdProjectId);
            Assert.Null(archivedProject);
        }

        /// <summary>
        /// Validates that a project wiith no title throws.
        /// </summary>
        [Fact]
        public async void CreateProject_ShouldFailOnEmptyTitle()
        {
            // Arrange
            Project newDecryptedProject = new Project();

            // Act
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _projectsService.CreateProject(newDecryptedProject, "127.0.1.1");
            });
        }

        /// <summary>
        /// Tests projet access identifier storage and retrieval.
        /// </summary>
        [Fact]
        public async void CreateProject_ShouldReturnCorrectAccess()
        {
            // Arrange
            _fakeHttpContextItems.Add(ApplicationIdentityService.CURRENTUSERKEY, _testUser);

            var newDecryptedProject = CreateTestProject();

            AccessIdentifier accessIdentifier1 = new AccessIdentifier();
            accessIdentifier1.Role = TeamStore.Keeper.Enums.Role.Editor;
            accessIdentifier1.Identity = new ApplicationUser()
            {
                AzureAdNameIdentifier = "TestAdIdentity1",
                AzureAdObjectIdentifier = "TestAdObjectId1",
                TenantId = "1234",
                Upn = "test@test.com"
            };

            AccessIdentifier accessIdentifier2 = new AccessIdentifier();
            accessIdentifier2.Role = TeamStore.Keeper.Enums.Role.Owner;
            accessIdentifier2.Identity = new ApplicationGroup()
            {
                DisplayName = "TestGroup1",
                AzureAdObjectIdentifier = "TestAdObjectId1",
                TenantId = "1234"
            };

            newDecryptedProject.AccessIdentifiers.Add(accessIdentifier1);
            newDecryptedProject.AccessIdentifiers.Add(accessIdentifier2);

            // Act
            var createdProjectId = await _projectsService.CreateProject(newDecryptedProject, "127.0.1.1");
            var retrievedProject = await _projectsService.GetProject(createdProjectId);

            // Assert
            Assert.True(retrievedProject.Id > 0);
            Assert.False(retrievedProject.IsArchived);
            Assert.Equal(3, retrievedProject.AccessIdentifiers.Count); // +1 for the Owner
            Assert.NotNull(retrievedProject.AccessIdentifiers.First());
            Assert.NotNull(retrievedProject.AccessIdentifiers.First().Project);
            Assert.True(retrievedProject.AccessIdentifiers.First().ProjectForeignKey > 0);
            Assert.Equal(Role.Editor, retrievedProject.AccessIdentifiers.First().Role);
            Assert.NotEqual(Role.Reader, retrievedProject.AccessIdentifiers.First().Role);
            Assert.IsType<ApplicationUser>(retrievedProject.AccessIdentifiers.First().Identity);
            Assert.Equal("1234", retrievedProject.AccessIdentifiers.First().Identity.TenantId);
            Assert.Equal("TestAdObjectId1", retrievedProject.AccessIdentifiers.First().Identity.AzureAdObjectIdentifier);
            Assert.Equal("test@test.com", ((ApplicationUser)retrievedProject.AccessIdentifiers.First().Identity).Upn);
            Assert.Equal("TestAdIdentity1", ((ApplicationUser)retrievedProject.AccessIdentifiers.First().Identity).AzureAdNameIdentifier);

            // Cleanup
            await _projectsService.ArchiveProject(retrievedProject, "127.0.1.1");
            var archivedProject = await _projectsService.GetProject(createdProjectId);
            Assert.Null(archivedProject);
        }

        /// <summary>
        /// This test creates a test project, grants access to a completely new
        /// user, which is resolved through a mock graph service, then
        /// asserts that a call to GetProjects and GetProject returns only 1 project
        /// </summary>
        [Fact]
        public async void GetProjects_ShouldGiveProjectsWithRightAccess()
        {
            // Arrange
            _fakeHttpContextItems.Add(ApplicationIdentityService.CURRENTUSERKEY, _testUser);
            var newDecryptedProject = CreateTestProject();
            var newUpn = "mock@upn.com-99";
            var _mockGraphService = _graphService as MockGraphService;
            _mockGraphService.AddUserToInternalList(new ApplicationUser()
            {
                Id = 99,
                AzureAdName = "Mock User 99",
                AzureAdNameIdentifier = "AzureAdNameId-9",
                AzureAdObjectIdentifier = "AzureAdObjectId-9",
                DisplayName = "Mock User Display Name 1",
                TenantId = "Mock Tenant Id 1",
                Upn = "mock@upn.com-99"
            });

            // Act
            var createdProjectId = await _projectsService.CreateProject(newDecryptedProject, "127.0.1.1");
            var retrievedProject = await _projectsService.GetProject(createdProjectId);
            var accessResult = await _permissionService.GrantAccessAsync(retrievedProject.Id, newUpn, Role.Owner, "1.2.3.4", _projectsService);
            var retrievedUser = await _applicationIdentityService.FindUserByUpnAsync(newUpn);
            _fakeHttpContextItems.Remove(ApplicationIdentityService.CURRENTUSERKEY);
            _fakeHttpContextItems.Add(ApplicationIdentityService.CURRENTUSERKEY, retrievedUser);
            var allProjects = await _projectsService.GetProjects(true);
            var singleProject = await _projectsService.GetProject(retrievedProject.Id);

            // Assert
            Assert.Equal(true, accessResult.Success);
            Assert.True(retrievedProject.AccessIdentifiers.Any(ai => ai.Identity.Id == retrievedUser.Id));
            Assert.Equal(1, allProjects.Count);
            Assert.Equal(singleProject, allProjects.First());
            Assert.True(singleProject.AccessIdentifiers.Any(ai => ai.Identity.Id == retrievedUser.Id));

            // Cleanup
            await _projectsService.ArchiveProject(retrievedProject, "127.0.1.1");
            var archivedProject = await _projectsService.GetProject(createdProjectId);
            Assert.Null(archivedProject);
        }

        /// <summary>
        /// Ensures that a new test user cannot access any projects.
        /// Creates a test project with testuser1 and retrieves 0 projects with testuser2
        /// </summary>
        [Fact]
        public async void GetProjects_ShouldNotReturnAnything()
        {
            // Arrange
            var newDecryptedProject = CreateTestProject();
            _fakeHttpContextItems.Add(ApplicationIdentityService.CURRENTUSERKEY, _testUser);
            var newTestUser = new ApplicationUser()
            {
                Id = 91,
                AzureAdName = "Mock User 91",
                AzureAdNameIdentifier = "AzureAdNameId-91",
                AzureAdObjectIdentifier = "AzureAdObjectId-91",
                DisplayName = "Mock User Display Name 91",
                TenantId = "Mock Tenant Id 91",
                Upn = "mock@upn.com-91"
            };

            // Act
            var createdProjectId = await _projectsService.CreateProject(newDecryptedProject, "127.0.1.1");
            var retrievedProject = await _projectsService.GetProject(createdProjectId);

            _fakeHttpContextItems.Remove(ApplicationIdentityService.CURRENTUSERKEY); // change user
            _fakeHttpContextItems.Add(ApplicationIdentityService.CURRENTUSERKEY, newTestUser);

            var allProjects = await _projectsService.GetProjects(true);
            var singleProject = await _projectsService.GetProject(retrievedProject.Id);

            // Assert
            Assert.Equal(0, allProjects.Count);
            Assert.Null(singleProject);
            Assert.NotNull(await _applicationIdentityService.GetCurrentUser());
            Assert.Equal("mock@upn.com-91", (await _applicationIdentityService.GetCurrentUser()).Upn);

            // Cleanup
            _fakeHttpContextItems.Remove(ApplicationIdentityService.CURRENTUSERKEY); // change user
            _fakeHttpContextItems.Add(ApplicationIdentityService.CURRENTUSERKEY, _testUser);
            await _projectsService.ArchiveProject(retrievedProject, "127.0.1.1");
            var archivedProject = await _projectsService.GetProject(createdProjectId);
            Assert.Null(archivedProject);
        }

        [Fact]
        public async void UpdateProject_ShouldRetrieveNewData()
        {
            // Arrange
            _fakeHttpContextItems.Add(ApplicationIdentityService.CURRENTUSERKEY, _testUser);

            string testTitle = "Project 1234 Test";
            string testDescription = "Created during integration tests";
            string testCategory = "Integration Tests";
            Project newDecryptedProject = new Project();
            newDecryptedProject.Title = testTitle;
            newDecryptedProject.Description = testDescription;
            newDecryptedProject.Category = testCategory;
            newDecryptedProject.IsPublic = false;

            // Act
            var createdProjectId = await _projectsService.CreateProject(newDecryptedProject, "127.0.1.1");
            var retrievedProject = await _projectsService.GetProject(createdProjectId);

            retrievedProject.Title = "Updated title 1";
            retrievedProject.Description = "Updated description 1";
            retrievedProject.Category = "Updated category 1";
            retrievedProject.IsPublic = true;

            await _projectsService.UpdateProject(retrievedProject, "127.0.1.1");

            var updatedProject = await _projectsService.GetProject(createdProjectId);

            // Аssert
            Assert.Equal("Updated title 1", updatedProject.Title);
            Assert.Equal("Updated description 1", updatedProject.Description);
            Assert.Equal("Updated category 1", updatedProject.Category);
            Assert.Equal(true, updatedProject.IsPublic);
        }

        [Fact]
        public async void UpdateProject_ShouldPreventAccessListChanges()
        {
            // Arrange
            _fakeHttpContextItems.Add(ApplicationIdentityService.CURRENTUSERKEY, _testUser);

            string testTitle = "Project 1234 Test";
            string testDescription = "Created during integration tests";
            string testCategory = "Integration Tests";
            Project newDecryptedProject = new Project();
            newDecryptedProject.Title = testTitle;
            newDecryptedProject.Description = testDescription;
            newDecryptedProject.Category = testCategory;
            newDecryptedProject.IsPublic = false;

            // Act
            var createdProjectId = await _projectsService.CreateProject(newDecryptedProject, "127.0.1.1");
            var retrievedProject = await _projectsService.GetProject(createdProjectId);

            retrievedProject.Title = "Updated title 1";
            retrievedProject.Description = "Updated description 1";
            retrievedProject.Category = "Updated category 1";
            retrievedProject.IsPublic = true;

            retrievedProject.AccessIdentifiers.Add(new AccessIdentifier() { Identity = new ApplicationUser() {  DisplayName = "Test", Upn = "test@test.com"} });

            // Аssert
            var exception = await Assert.ThrowsAsync(typeof(Exception), 
                () => _projectsService.UpdateProject(retrievedProject, "127.0.1.1"));
            Assert.Equal("You cannot update a project's access list unless you are sharing access.", exception.Message);
        }
    }
}
