using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TeamStore.Keeper.Models;
using TeamStore.Keeper.Services;
using Xunit;

namespace IntegrationTests
{
    public class AssetTests : IntegrationTestBase
    {
        public AssetTests()
        {
            _fakeHttpContextItems[ApplicationIdentityService.CURRENTUSERKEY] = _testUser;
        }

        [Fact]
        public async void CreateAssets_ShouldReturnCorrectProperties()
        {
            // Arrange
            var allProjects = await _projectsService.GetProjects(true);
            var newProjectId = await _projectsService.CreateProject(CreateTestProject());
            var newCredential = CreateTestCredential();
            var newNote = CreateTestNote();

            // Act
            var retrievedProject = await _projectsService.GetProject(newProjectId);
            var persistedAsset = await _assetService.AddAssetToProjectAsync(newProjectId, newCredential, "127.0.1.1");
            var persistedNote = await _assetService.AddAssetToProjectAsync(newProjectId, newNote, "127.0.1.1");
            var retrievedAsset = await _assetService.GetAssetAsync(newProjectId, persistedAsset.Id, "127.0.1.1");
            var retrievedAssetNote = await _assetService.GetAssetAsync(newProjectId, persistedNote.Id, "127.0.1.1");
            var retrievedCredential = (Credential)retrievedAsset;
            var retrievedNote = (Note)retrievedAssetNote;

            // Assert
            Assert.NotNull(retrievedCredential);
            Assert.NotNull(retrievedNote);
            Assert.NotNull(persistedAsset);
            Assert.NotNull(persistedNote);
            Assert.NotNull(retrievedAsset);
            Assert.NotNull(retrievedAssetNote);

            Assert.Equal("Login123", retrievedCredential.Login);
            //Assert.Equal("Password", retrievedCredential.Password);
            Assert.Equal("DOMAIN", retrievedCredential.Domain);

            Assert.Equal("Test note 12345", retrievedNote.Title);
            Assert.Equal("Test body test body Test body test body Test body test body Test body test body", retrievedNote.Body);
            Assert.Equal(2, retrievedProject.Assets.Count);

            // test created/modified times
            Assert.Equal("TestAdObjectId11234567890", retrievedCredential.CreatedBy.AzureAdObjectIdentifier);
            Assert.Equal("TestAdObjectId11234567890", retrievedNote.CreatedBy.AzureAdObjectIdentifier);
            System.Threading.Thread.Sleep(1);
            Assert.True(retrievedCredential.Created < DateTime.UtcNow);
            Assert.True(retrievedNote.Created < DateTime.UtcNow);

            // Cleanup
            await _projectsService.ArchiveProject(retrievedProject, "127.0.1.1");
            var archivedProject = await _projectsService.GetProject(newProjectId);
            Assert.Null(archivedProject);
        }

        [Theory,
            InlineData("", "", ""),
            InlineData("123456", "asdfdsafa dfdsa fdsa fdsa fdsa fdsa fs", "asdsadsadsadsad\\")]
        public async void CreateCredential_ShouldEncryptAsset(string login, string password, string domain)
        {
            // Arrange
            var allProjects = await _projectsService.GetProjects(true);
            var newProjectId = await _projectsService.CreateProject(CreateTestProject());
            var newCredential = new Credential();
            newCredential.Login = login;
            newCredential.Password = password;
            newCredential.Domain = domain;

            // Act
            var retrievedProject = await _projectsService.GetProject(newProjectId);
            var persistedAsset = await _assetService.AddAssetToProjectAsync(newProjectId, newCredential, "127.0.1.1");
            var retrievedAsset = _dbContext.Assets.Where(a => a.Id == newCredential.Id).FirstOrDefault();
            var retrievedCredential = (Credential)retrievedAsset;

            // Assert
            Assert.NotEqual(retrievedCredential.Login, login);
            Assert.NotEqual(retrievedCredential.Password, password);
            Assert.NotEqual(retrievedCredential.Domain, domain);
            Assert.False(string.IsNullOrWhiteSpace(retrievedCredential.Login));
            Assert.False(string.IsNullOrWhiteSpace(retrievedCredential.Password));
            Assert.False(string.IsNullOrWhiteSpace(retrievedCredential.Domain));

            // Cleanup
            await _projectsService.ArchiveProject(retrievedProject, "127.0.1.1");
            var archivedProject = await _projectsService.GetProject(newProjectId);
            Assert.Null(archivedProject);
        }

        [Theory,
            InlineData("", ""),
            InlineData("123456", "asdfdsafa dfdsa fdsa fdsa fdsa fdsa fs")]
        public async void CreateNote_ShouldEncryptAsset(string title, string body)
        {
            // Arrange
            var allProjects = await _projectsService.GetProjects(true);
            var newProjectId = await _projectsService.CreateProject(CreateTestProject());
            var newNote = new Note();
            newNote.Title = title;
            newNote.Body = body;

            // Act
            var retrievedProject = await _projectsService.GetProject(newProjectId);
            var persistedNote = await _assetService.AddAssetToProjectAsync(newProjectId, newNote, "127.0.1.1");
            var retrievedAssetNote = _dbContext.Assets.Where(a => a.Id == newNote.Id).FirstOrDefault();
            var retrievedNote = (Note)retrievedAssetNote;

            // Assert
            Assert.NotEqual(retrievedNote.Title, title);
            Assert.NotEqual(retrievedNote.Body, body);
            Assert.False(string.IsNullOrWhiteSpace(retrievedNote.Title));
            Assert.False(string.IsNullOrWhiteSpace(retrievedNote.Body));

            // Cleanup
            await _projectsService.ArchiveProject(retrievedProject, "127.0.1.1");
            var archivedProject = await _projectsService.GetProject(newProjectId);
            Assert.Null(archivedProject);
        }

        [Fact]
        public async void CreateUpdateNote_ShouldReturnCorrectProperties()
        {
            // Arrange
            var newProjectId = await _projectsService.CreateProject(CreateTestProject());
            var newNote = CreateTestNote();

            // Act
            var retrievedProject = await _projectsService.GetProject(newProjectId);
            await _assetService.AddAssetToProjectAsync(newProjectId, newNote, "127.0.1.1");
            var createdAsset = await _assetService.GetAssetAsync(newProjectId, newNote.Id, "127.0.1.1");
            var createdNote = createdAsset as Note;
            createdNote.Title = "NewTitle";
            createdNote.Body = "NewBody body body";
            var updatedAsset = await _assetService.UpdateAssetAsync(newProjectId, createdNote);
            var retrievedAsset = await _assetService.GetAssetAsync(newProjectId, updatedAsset.Id, "127.0.1.1");
            var retrievedNote = retrievedAsset as Note;

            // Assert
            Assert.Equal("NewTitle", retrievedNote.Title);
            Assert.Equal("NewBody body body", retrievedNote.Body);

            // Cleanup
            await _projectsService.ArchiveProject(retrievedProject, "127.0.1.1");
            var archivedProject = await _projectsService.GetProject(newProjectId);
            Assert.Null(archivedProject);
        }

        [Fact]
        public async void CreateUpdateCredential_ShouldReturnCorrectProperties()
        {
            // Arrange
            var newProjectId = await _projectsService.CreateProject(CreateTestProject());
            var newCredential = CreateTestCredential();

            // Act
            var retrievedProject = await _projectsService.GetProject(newProjectId);
            await _assetService.AddAssetToProjectAsync(newProjectId, newCredential, "127.0.1.1");
            var createdAsset = await _assetService.GetAssetAsync(newProjectId, newCredential.Id, "127.0.1.1");
            var createdCredential = createdAsset as Credential;
            createdCredential.Login = "NewLogin";
            createdCredential.Domain = "NewDomain";
            //createdCredential.Password = "NewPass";
            var updatedAsset = await _assetService.UpdateAssetAsync(newProjectId, createdCredential);
            var retrievedAsset = await _assetService.GetAssetAsync(newProjectId, updatedAsset.Id, "127.0.1.1");
            var retrievedCredential = retrievedAsset as Credential;

            // Assert
            Assert.Equal("NewLogin", retrievedCredential.Login);
            Assert.Equal("NewDomain", retrievedCredential.Domain);
            //Assert.Equal("NewPass", retrievedCredential.Password);

            // Cleanup
            await _projectsService.ArchiveProject(retrievedProject, "127.0.1.1");
            var archivedProject = await _projectsService.GetProject(newProjectId);
            Assert.Null(archivedProject);
        }

        [Fact]
        public async void CreateArchiveCredential_ShouldNotReturnAsset()
        {
            // Arrange
            var newProjectId = await _projectsService.CreateProject(CreateTestProject());
            var newCredential = CreateTestCredential();

            // Act
            var retrievedProject = await _projectsService.GetProject(newProjectId);
            await _assetService.AddAssetToProjectAsync(newProjectId, newCredential, "127.0.1.1");
            var createdAsset = await _assetService.GetAssetAsync(newProjectId, newCredential.Id, "127.0.1.1");
            var createdCredential = createdAsset as Credential;
            await _assetService.ArchiveAssetAsync(newProjectId, createdCredential.Id, "127.0.1.1");
            var archivedAsset = await _assetService.GetAssetAsync(newProjectId, createdCredential.Id, "127.0.1.1");

            // Assert
            Assert.Null(archivedAsset);
        }

        [Fact]
        public async void CreateArchiveNote_ShouldNotReturnAsset()
        {
            // Arrange
            var newProjectId = await _projectsService.CreateProject(CreateTestProject());
            var newNote = CreateTestNote();

            // Act
            var retrievedProject = await _projectsService.GetProject(newProjectId);
            await _assetService.AddAssetToProjectAsync(newProjectId, newNote, "127.0.1.1");
            var createdAsset = await _assetService.GetAssetAsync(newProjectId, newNote.Id, "127.0.1.1");
            var createdNote = createdAsset as Note;
            await _assetService.ArchiveAssetAsync(newProjectId, createdNote.Id, "127.0.1.1");
            var archivedAsset = await _assetService.GetAssetAsync(newProjectId, createdNote.Id, "127.0.1.1");

            // Assert
            Assert.Null(archivedAsset);
        }
    }
}
