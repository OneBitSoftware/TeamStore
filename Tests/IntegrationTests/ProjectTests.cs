namespace IntegrationTests
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IO;
    using System.Linq;

    using TeamStore.DataAccess;
    using TeamStore.Interfaces;
    using TeamStore.Models;
    using TeamStore.Services;
    using Xunit;

    public class ProjectTests
    {
        IConfigurationRoot _configuration;
        IEncryptionService _encryptionService;
        IProjectsService _projectsService;
        IPermissionService _permissionService;
        IGraphService _graphService;

        HttpContext _testHttpContext;
        IHttpContextAccessor _httpContextAccessor;

        public ProjectTests()
        {
            BuildTestConfiguration();
            var dbContext = GetDbContext();
            _testHttpContext = new DefaultHttpContext();
            _httpContextAccessor = new HttpContextAccessor();
            _httpContextAccessor.HttpContext = _testHttpContext;

            var memoryCache = new MemoryCache(new MemoryCacheOptions() { } );
            _graphService = new GraphService(memoryCache, _configuration);
            _encryptionService = new EncryptionService();
            _permissionService = new PermissionService(_graphService, _httpContextAccessor);
            _projectsService = new ProjectsService(dbContext, _encryptionService, _permissionService);
        }

        [Fact]
        public async void CreateProject_ShouldReturnCorrectData()
        {
            // Arrange
            string testTitle = "Project 1234 Test";
            string testDescription = "Created during integration tests";
            string testCategory = "Integration Tests";
            Project newDecryptedProject = new Project();
            newDecryptedProject.Title = testTitle;
            newDecryptedProject.Description = testDescription;
            newDecryptedProject.Category = testCategory;

            // Act
            var createdProjectId = await _projectsService.CreateProject(newDecryptedProject);
            var retrievedProject = await _projectsService.GetProject(createdProjectId);

            // Assert
            Assert.Equal(retrievedProject.Title, testTitle);
            Assert.Equal(retrievedProject.Description, testDescription);
            Assert.Equal(retrievedProject.Category, testCategory);
            Assert.True(retrievedProject.Id > 0);

            // Cleanup
            await _projectsService.ArchiveProject(retrievedProject);
            var archivedProject = await _projectsService.GetProject(createdProjectId);
            Assert.Null(archivedProject);
        }

        [Fact]
        public async void CreateProject_ShouldReturnCorrectAccess()
        {
            // Arrange
            string testTitle = "Project 1234 Access Test";
            string testDescription = "Created during integration tests";
            string testCategory = "Access Tests";
            Project newDecryptedProject = new Project();
            newDecryptedProject.Title = testTitle;
            newDecryptedProject.Description = testDescription;
            newDecryptedProject.Category = testCategory;

            AccessIdentifier accessIdentifier1 = new AccessIdentifier();
            accessIdentifier1.Role = "Test";
            accessIdentifier1.Identity = new ApplicationUser()
            {
                AzureAdNameIdentifier = "TestAdIdentity1",
                AzureAdObjectIdentifier = "TestAdObjectId1",
                TenantId = "1234",
                Upn = "test@test.com"
            };

            AccessIdentifier accessIdentifier2 = new AccessIdentifier();
            accessIdentifier2.Role = "Admin";
            accessIdentifier2.Identity = new ApplicationGroup()
            {
                DisplayName = "TestGroup1",
                AzureAdObjectIdentifier = "TestAdObjectId1",
                TenantId = "1234"
            };

            newDecryptedProject.AccessIdentifiers.Add(accessIdentifier1);
            newDecryptedProject.AccessIdentifiers.Add(accessIdentifier2);

            // Act
            var createdProjectId = await _projectsService.CreateProject(newDecryptedProject);
            var retrievedProject = await _projectsService.GetProject(createdProjectId);

            // Assert
            Assert.True(retrievedProject.Id > 0);
            Assert.False(retrievedProject.IsArchived);
            Assert.Equal(2, retrievedProject.AccessIdentifiers.Count);
            Assert.NotNull(retrievedProject.AccessIdentifiers.First());
            Assert.NotNull(retrievedProject.AccessIdentifiers.First().Project);
            Assert.True(retrievedProject.AccessIdentifiers.First().ProjectForeignKey > 0);
            Assert.Equal("Test", retrievedProject.AccessIdentifiers.First().Role);
            Assert.NotEqual("Test1234", retrievedProject.AccessIdentifiers.First().Role);
            Assert.IsType<ApplicationUser>(retrievedProject.AccessIdentifiers.First().Identity);
            Assert.Equal("1234", retrievedProject.AccessIdentifiers.First().Identity.TenantId);
            Assert.Equal("TestAdObjectId1", retrievedProject.AccessIdentifiers.First().Identity.AzureAdObjectIdentifier);
            Assert.Equal("test@test.com", ((ApplicationUser)retrievedProject.AccessIdentifiers.First().Identity).Upn);
            Assert.Equal("TestAdIdentity1", ((ApplicationUser)retrievedProject.AccessIdentifiers.First().Identity).AzureAdNameIdentifier);
            
            // Cleanup
            await _projectsService.ArchiveProject(retrievedProject);
            var archivedProject = await _projectsService.GetProject(createdProjectId);
            Assert.Null(archivedProject);
        }

        private void BuildTestConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("testsettings.json");

            _configuration = builder.Build();
        }

        private ApplicationDbContext GetDbContext()
        {
            var fileName = _configuration["DataAccess:SQLiteDbFileName"];
            var connectionString = "Data Source=" + fileName;

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            var dbContext = new ApplicationDbContext(optionsBuilder.Options);

            // Set up the DbContext for data access
            return dbContext;
        }
    }
}
