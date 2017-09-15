namespace UnitTests
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Security.Claims;
    using System.Text;
    using TeamStore.Interfaces;
    using TeamStore.Models;
    using TeamStore.Services;
    using Xunit;

    public class PermissionServiceTests
    {
        IPermissionService _permissionService;
        IConfigurationRoot _configuration;
        IGraphService _graphService;

        HttpContext _testHttpContext;
        IHttpContextAccessor _httpContextAccessor;

        public PermissionServiceTests()
        {
            BuildTestConfiguration();

            var memoryCache = new MemoryCache(new MemoryCacheOptions() { });
            _graphService = new GraphService(memoryCache, _configuration);
            _testHttpContext = new DefaultHttpContext();
            _httpContextAccessor = new HttpContextAccessor();
            _httpContextAccessor.HttpContext = _testHttpContext;
            _permissionService = new PermissionService(_graphService, _httpContextAccessor);

            SetApplicationUser();
        }

        private void BuildTestConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("testsettings.json");

            //            builder.AddUserSecrets(Assembly.GetExecutingAssembly());

            _configuration = builder.Build();
        }

        private void SetApplicationUser()
        {
            var newApplicationUser = new ApplicationUser();
            newApplicationUser.AzureAdNameIdentifier = "Unit Tests 1234 - Name ID";
            newApplicationUser.AzureAdObjectIdentifier = "Unit Test 1234 - Object ID";
            newApplicationUser.TenantId = "Unit Test 1234 - Tenant ID";
            newApplicationUser.Upn = "test@testupn.integration";

            _testHttpContext.Items[PermissionService.CURRENTUSERKEY] = newApplicationUser;
        }

        [Fact]
        public void GetCurrentUser_ShouldReturnArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                PermissionService permissionService = new PermissionService(_graphService, null);
            });
        }

        [Fact]
        public void GetCurrentUser_UnauthenticatedIdentityShouldReturnNull()
        {
            // Arrange
            ApplicationUser nullUser;
            PermissionService permissionService = new PermissionService(_graphService, new HttpContextAccessor());
            _testHttpContext.Items[PermissionService.CURRENTUSERKEY] = null; // need to clear singleton context

            // Act
            nullUser = permissionService.GetCurrentUser();

            // Assert
            Assert.Null(nullUser);
        }

        [Fact]
        public void GetCurrentUser_NullIdentityShouldReturnNull()
        {
            // Arrange
            ApplicationUser nullUser;
            PermissionService permissionService = new PermissionService(_graphService, new HttpContextAccessor());

            // Act
            nullUser = permissionService.GetCurrentUser(null);

            // Assert
            Assert.Null(nullUser);
        }

        [Fact]
        public void GetCurrentUser_ShouldReturnUser()
        {
            // Arrange
            var newApplicationUser = new ApplicationUser();
            newApplicationUser.AzureAdNameIdentifier = "Unit Tests 1234 - Name ID";
            newApplicationUser.AzureAdObjectIdentifier = "Unit Test 1234 - Object ID";
            newApplicationUser.TenantId = "Unit Test 1234 - Tenant ID";
            newApplicationUser.Upn = "test@testupn.integration";

            _testHttpContext.Items[PermissionService.CURRENTUSERKEY] = newApplicationUser;

            ApplicationUser returnedUser;
            PermissionService permissionService = new PermissionService(_graphService, _httpContextAccessor);

            // Act
            returnedUser = permissionService.GetCurrentUser();

            // Assert
            Assert.NotNull(returnedUser);
            Assert.Equal(newApplicationUser.AzureAdNameIdentifier, returnedUser.AzureAdNameIdentifier);
            Assert.Equal(newApplicationUser.AzureAdObjectIdentifier, returnedUser.AzureAdObjectIdentifier);
            Assert.Equal(newApplicationUser.TenantId, returnedUser.TenantId);
            Assert.Equal(newApplicationUser.Upn, returnedUser.Upn);
        }

        [Fact]
        public void GetCurrentUser_ShouldReturnCorrectClaimsIdentity()
        {
            // Arrange
            Claim objectIdClaim = new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "my unit test object id");
            ClaimsIdentity newClaimsIdentity = new ClaimsIdentity();
            newClaimsIdentity.AddClaim(new Claim(ClaimTypes.Name, "my unit test name"));
            newClaimsIdentity.AddClaim(objectIdClaim);

            // TODO add claims

            var mockContext = new Mock<ClaimsIdentity>();
            mockContext.SetupGet(p => p.IsAuthenticated).Returns(true);
            mockContext.Object.AddClaim(new Claim(ClaimTypes.Name, "my unit test name"));
            mockContext.Object.AddClaim(objectIdClaim);


            var memoryCache = new MemoryCache(new MemoryCacheOptions() { });
            GraphService graphService = new GraphService(memoryCache, _configuration);
            PermissionService permissionService = new PermissionService(graphService, _httpContextAccessor);

            // Act
            ApplicationUser retrievedUser = permissionService.GetCurrentUser(mockContext.Object);

            // Assert
            Assert.Equal("my unit test object id", retrievedUser.AzureAdObjectIdentifier);
            Assert.Equal("my unit test name", retrievedUser.AzureAdNameIdentifier);
        }
    }
}
