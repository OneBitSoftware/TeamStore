namespace UnitTests
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Claims;
    using TeamStore.Keeper.DataAccess;
    using TeamStore.Keeper.Interfaces;
    using TeamStore.Keeper.Models;
    using TeamStore.Keeper.Services;
    using Xunit;

    public class ApplicationIdentityServiceTests
    {
        IApplicationIdentityService _applicationIdentityService;
        IConfigurationRoot _configuration;
        ApplicationDbContext _dbContext;
        HttpContext _testHttpContext;
        IHttpContextAccessor _httpContextAccessor;
        IGraphService _graphService;

        public ApplicationIdentityServiceTests()
        {
            BuildTestConfiguration();

            var memoryCache = new MemoryCache(new MemoryCacheOptions() { });
            _graphService = new GraphService(memoryCache, _configuration);
            _testHttpContext = new DefaultHttpContext();
            _httpContextAccessor = new HttpContextAccessor();
            _httpContextAccessor.HttpContext = _testHttpContext;

            var dbContextOptionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            dbContextOptionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            _dbContext = new ApplicationDbContext(dbContextOptionsBuilder.Options);

            _applicationIdentityService = new ApplicationIdentityService(_dbContext, _graphService, _httpContextAccessor);

            SetApplicationUser();
        }

        /// <summary>Microsoft.EntityFrameworkCore.InMemory
        /// Reads testsettings.json and builds out the configuration
        /// </summary>
        private void BuildTestConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("testsettings.json");

            _configuration = builder.Build();
        }

        /// <summary>
        /// Fakes an ApplicationUser in the HttpContext of the current request
        /// </summary>
        private void SetApplicationUser()
        {
            var newApplicationUser = new ApplicationUser();
            newApplicationUser.AzureAdNameIdentifier = "Unit Tests 1234 - Name ID";
            newApplicationUser.AzureAdObjectIdentifier = "Unit Test 1234 - Object ID";
            newApplicationUser.TenantId = "Unit Test 1234 - Tenant ID";
            newApplicationUser.Upn = "test@testupn.integration";

            _testHttpContext.Items[ApplicationIdentityService.CURRENTUSERKEY] = newApplicationUser;
        }

        /// <summary>
        /// Ensures we have a valid Http Context Accessor as this service depends on HttpContext
        /// </summary>
        [Fact]
        public void GetCurrentUser_ShouldReturnArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ApplicationIdentityService applicationIdentityService = new ApplicationIdentityService(_dbContext, _graphService, null);
            });
        }

        /// <summary>
        /// Validates that a null item in the items collection returns null and doesn't throw
        /// </summary>
        [Fact]
        public async void GetCurrentUser_UnauthenticatedIdentityShouldReturnNull()
        {
            // Arrange
            ApplicationUser nullUser;
            ApplicationIdentityService applicationIdentityService = new ApplicationIdentityService(_dbContext, _graphService, new HttpContextAccessor());
            _testHttpContext.Items[ApplicationIdentityService.CURRENTUSERKEY] = null; // need to clear singleton context

            // Act
            nullUser = await applicationIdentityService.GetCurrentUser();

            // Assert
            Assert.Null(nullUser);
        }

        /// <summary>
        /// Checks that a null parameter returns null and doesn't throw
        /// </summary>
        [Fact]
        public async void GetCurrentUser_NullIdentityShouldReturnNull()
        {
            // Arrange
            ApplicationUser nullUser;
            ApplicationIdentityService applicationIdentityService = new ApplicationIdentityService(_dbContext, _graphService, new HttpContextAccessor());

            // Act
            nullUser = await applicationIdentityService.GetCurrentUser(null);

            // Assert
            Assert.Null(nullUser);
        }

        /// <summary>
        /// Fakes an ApplicationUser, which is then retrieved by GetCurrentUser
        /// </summary>
        [Fact]
        public async void GetCurrentUser_ShouldReturnUser()
        {
            // Arrange
            var newApplicationUser = new ApplicationUser();
            newApplicationUser.AzureAdNameIdentifier = "Unit Tests 1234 - Name ID";
            newApplicationUser.AzureAdObjectIdentifier = "Unit Test 1234 - Object ID";
            newApplicationUser.TenantId = "Unit Test 1234 - Tenant ID";
            newApplicationUser.Upn = "test@testupn.integration";

            _testHttpContext.Items[ApplicationIdentityService.CURRENTUSERKEY] = newApplicationUser;

            ApplicationUser returnedUser;
            ApplicationIdentityService applicationIdentityService = new ApplicationIdentityService(_dbContext, _graphService, _httpContextAccessor);

            // Act
            returnedUser = await applicationIdentityService.GetCurrentUser();

            // Assert
            Assert.NotNull(returnedUser);
            Assert.Equal(newApplicationUser.AzureAdNameIdentifier, returnedUser.AzureAdNameIdentifier);
            Assert.Equal(newApplicationUser.AzureAdObjectIdentifier, returnedUser.AzureAdObjectIdentifier);
            Assert.Equal(newApplicationUser.TenantId, returnedUser.TenantId);
            Assert.Equal(newApplicationUser.Upn, returnedUser.Upn);
        }

        /// <summary>
        /// Mocks a ClaimsIdentity, then tests the conversion to ApplicationUser.
        /// Asserts all parameters are the same.
        /// </summary>
        [Fact]
        public async void GetCurrentUser_ShouldReturnCorrectClaimsIdentity()
        {
            // Arrange
            List<Claim> claimsList = new List<Claim>();
            claimsList.Add(new Claim(
                "http://schemas.microsoft.com/identity/claims/objectidentifier",
                "my unit test object id"));
            claimsList.Add(new Claim(ClaimTypes.NameIdentifier, "Name Identifier Test"));
            claimsList.Add(new Claim(ClaimTypes.Name, "Display Name Claim Test"));
            claimsList.Add(new Claim("name", "Simple Name Claim Test"));
            claimsList.Add(new Claim("ipaddr", "1.2.3.4"));
            claimsList.Add(new Claim(ClaimTypes.Upn, "upn@rtest.com"));
            claimsList.Add(new Claim("http://schemas.microsoft.com/identity/claims/tenantid", "12345678-1234-1234-1234-123982828122"));

            var mockContext = new Mock<ClaimsIdentity>();
            mockContext.SetupGet(p => p.IsAuthenticated).Returns(true);
            mockContext.SetupGet(p => p.Claims).Returns(claimsList);

            ApplicationIdentityService applicationIdentityService;
            ApplicationUser retrievedUser;

            // Act
            applicationIdentityService = new ApplicationIdentityService(_dbContext, _graphService, _httpContextAccessor);
            retrievedUser = await applicationIdentityService.GetCurrentUser(mockContext.Object);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.Equal("my unit test object id", retrievedUser.AzureAdObjectIdentifier);
            Assert.Equal("Name Identifier Test", retrievedUser.AzureAdNameIdentifier);
            Assert.Equal("Display Name Claim Test", retrievedUser.AzureAdName);
            Assert.Equal("Simple Name Claim Test", retrievedUser.DisplayName);
            Assert.Equal("upn@rtest.com", retrievedUser.Upn);
            Assert.Equal("12345678-1234-1234-1234-123982828122", retrievedUser.TenantId);
        }

        [Fact]
        public async void FindUserByRandomString_ShouldReturnNull()
        {
            // Arrange
            ApplicationIdentityService applicationIdentityService = 
                new ApplicationIdentityService(_dbContext, _graphService, _httpContextAccessor);

            // Act
            ApplicationUser retrievedUser = await applicationIdentityService.FindUserAsync(Guid.NewGuid().ToString());

            // Assert
            Assert.Null(retrievedUser);
        }
    }
}
