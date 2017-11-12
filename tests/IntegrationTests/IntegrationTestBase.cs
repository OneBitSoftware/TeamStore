using IntegrationTests.Framework;
using IntegrationTests.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TeamStore.Keeper.DataAccess;
using TeamStore.Keeper.Interfaces;
using TeamStore.Keeper.Models;
using TeamStore.Keeper.Services;

namespace IntegrationTests
{
    /// <summary>
    /// Base class for integration tests. Sets up HttpContext, a test user and the DB Context.
    /// </summary>
    public abstract class IntegrationTestBase
    {
        protected IConfigurationRoot _configuration;
        protected IEncryptionService _encryptionService;
        protected IProjectsService _projectsService;
        protected IPermissionService _permissionService;
        protected IApplicationIdentityService _applicationIdentityService;
        protected IGraphService _graphService;
        protected IEventService _eventService;
        protected IAssetService _assetService;
        protected ApplicationUser _testUser;
        protected ApplicationDbContext _dbContext;
        protected EventDbContext _eventDbContext;
        protected HttpContext _testHttpContext;
        protected IHttpContextAccessor _httpContextAccessor;
        protected IDictionary<object, object> _fakeHttpContextItems;
        protected IAccessTokenRetriever accessTokenRetriever;

        public IntegrationTestBase()
        {
            BuildTestConfiguration();
            _dbContext = GetDbContext();
            _eventDbContext = GetEventDbContext();

            _testHttpContext = new DefaultHttpContext();
            _httpContextAccessor = new HttpContextAccessor();
            _httpContextAccessor.HttpContext = _testHttpContext;
            _fakeHttpContextItems = new Dictionary<object, object>();

            var memoryCache = new MemoryCache(new MemoryCacheOptions() { });
            var accessTokenRetriever = new TestAccessTokenRetriever();
            _graphService = new MockGraphService();
            _encryptionService = new EncryptionService();
            _applicationIdentityService = new ApplicationIdentityService(_dbContext, _graphService, _httpContextAccessor, _fakeHttpContextItems);
            _eventService = new EventService(_eventDbContext, _applicationIdentityService);
            _permissionService = new PermissionService(_dbContext, _graphService, _eventService, _applicationIdentityService);
            _projectsService = new ProjectsService(_dbContext, _encryptionService, _eventService, _applicationIdentityService, _permissionService);
            _assetService = new AssetService(_dbContext, _projectsService, _encryptionService, _eventService, _applicationIdentityService);

            _testUser = _applicationIdentityService.FindUserAsync(u=>u.AzureAdObjectIdentifier == "TestAdObjectId11234567890").Result;
            if (_testUser == null)
            {
                _testUser = new ApplicationUser()
                {
                    DisplayName = "Test User 123456789",
                    AzureAdObjectIdentifier = "TestAdObjectId11234567890",
                    TenantId = "1234-12345-123",
                    AzureAdName = "Test User Name",
                    AzureAdNameIdentifier = "123123kl21j3lk12j31",
                    Upn = "123123123@12312312.com",
                };

                _dbContext.ApplicationIdentities.Add(_testUser);
                _dbContext.SaveChanges();
            }
        }

        protected void BuildTestConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("testsettings.json");

            builder.AddUserSecrets(Assembly.GetExecutingAssembly());

            _configuration = builder.Build();
        }

        protected ApplicationDbContext GetDbContext()
        {
            var fileName = _configuration["DataAccess:SQLiteDbFileName"];
            var connectionString = "Data Source=" + fileName;

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            // When in tests, we use context.EnsureCreated() instead of migrations OLD COMMENT
            var dbContext = new ApplicationDbContext(optionsBuilder.Options, true, true);

            // Set up the DbContext for data access
            return dbContext;
        }

        protected EventDbContext GetEventDbContext()
        {
            var fileName = _configuration["DataAccess:SQLiteDbFileName"];
            var connectionString = "Data Source=" + fileName;

            var optionsBuilder = new DbContextOptionsBuilder<EventDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            // When in tests, we use context.EnsureCreated() instead of migrations
            var dbContext = new EventDbContext(optionsBuilder.Options, true, true);

            // Set up the DbContext for data access
            return dbContext;
        }

        /// <summary>
        /// Creates a project for testing purposes
        /// </summary>
        /// <remarks>
        /// Sets title, description & category properties
        /// </remarks>
        /// <returns></returns>
        protected Project CreateTestProject()
        {
            string testTitle = "Project 1234 Test";
            string testDescription = "Created during integration tests";
            string testCategory = "Category Tests";

            Project newDecryptedProject = new Project();
            newDecryptedProject.Title = testTitle;
            newDecryptedProject.Description = testDescription;
            newDecryptedProject.Category = testCategory;

            return newDecryptedProject;
        }

        protected Credential CreateTestCredential()
        {
            var login = "Login123";
            var pass = "Password";
            var domain = "DOMAIN";

            var testCredential = new Credential();
            testCredential.Login = login;
            testCredential.Password = pass;
            testCredential.Domain = domain;
            testCredential.IsArchived = false;

            return testCredential;
        }

        protected Note CreateTestNote()
        {
            var title = "Test note 12345";
            var body = "Test body test body Test body test body Test body test body Test body test body";

            var testNote = new Note();
            testNote.Title = title;
            testNote.Body = body;
            testNote.IsArchived = false;

            return testNote;
        }
    }
}
