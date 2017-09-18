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

    public class ApplicationIdentityServiceTests
    {
        IApplicationIdentityService _applicationIdentityService;
        IConfigurationRoot _configuration;

        HttpContext _testHttpContext;
        IHttpContextAccessor _httpContextAccessor;

        public ApplicationIdentityServiceTests()
        {
            BuildTestConfiguration();

            var memoryCache = new MemoryCache(new MemoryCacheOptions() { });
            _testHttpContext = new DefaultHttpContext();
            _httpContextAccessor = new HttpContextAccessor();
            _httpContextAccessor.HttpContext = _testHttpContext;
            _applicationIdentityService = new ApplicationIdentityService(_httpContextAccessor);

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

            _testHttpContext.Items[ApplicationIdentityService.CURRENTUSERKEY] = newApplicationUser;
        }

        [Fact]
        public void GetCurrentUser_ShouldReturnArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ApplicationIdentityService applicationIdentityService = new ApplicationIdentityService(null);
            });
        }

        [Fact]
        public void GetCurrentUser_UnauthenticatedIdentityShouldReturnNull()
        {
            // Arrange
            ApplicationUser nullUser;
            ApplicationIdentityService applicationIdentityService = new ApplicationIdentityService(new HttpContextAccessor());
            _testHttpContext.Items[ApplicationIdentityService.CURRENTUSERKEY] = null; // need to clear singleton context

            // Act
            nullUser = applicationIdentityService.GetCurrentUser();

            // Assert
            Assert.Null(nullUser);
        }

        [Fact]
        public void GetCurrentUser_NullIdentityShouldReturnNull()
        {
            // Arrange
            ApplicationUser nullUser;
            ApplicationIdentityService applicationIdentityService = new ApplicationIdentityService(new HttpContextAccessor());

            // Act
            nullUser = applicationIdentityService.GetCurrentUser(null);

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

            _testHttpContext.Items[ApplicationIdentityService.CURRENTUSERKEY] = newApplicationUser;

            ApplicationUser returnedUser;
            ApplicationIdentityService applicationIdentityService = new ApplicationIdentityService(_httpContextAccessor);

            // Act
            returnedUser = applicationIdentityService.GetCurrentUser();

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
            ApplicationIdentityService applicationIdentityService = new ApplicationIdentityService(_httpContextAccessor);

            // Act
            ApplicationUser retrievedUser = applicationIdentityService.GetCurrentUser(mockContext.Object);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.Equal("my unit test object id", retrievedUser.AzureAdObjectIdentifier);
            Assert.Equal("my unit test name", retrievedUser.AzureAdNameIdentifier);
        }
    }
}
