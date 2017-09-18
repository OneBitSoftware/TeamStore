using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TeamStore.DataAccess;
using TeamStore.Interfaces;

namespace IntegrationTests
{
    public abstract class IntegrationTestBase
    {
        protected IConfigurationRoot _configuration;
        protected IEncryptionService _encryptionService;
        protected IProjectsService _projectsService;
        protected IPermissionService _permissionService;
        protected IApplicationIdentityService _applicationIdentityService;
        protected IGraphService _graphService;
        protected IEventService _eventService;

        protected ApplicationDbContext _dbContext;

        public IntegrationTestBase()
        {
            BuildTestConfiguration();
            _dbContext = GetDbContext();
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

            // When in tests, we use context.EnsureCreated() instead of migrations
            var dbContext = new ApplicationDbContext(optionsBuilder.Options, true);

            // Set up the DbContext for data access
            return dbContext;
        }
    }
}
