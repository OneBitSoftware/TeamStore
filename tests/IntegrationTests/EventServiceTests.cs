namespace IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TeamStore.Keeper.Services;
    using Xunit;

    public class EventServiceTests : IntegrationTestBase
    {
        public EventServiceTests()
        {
            _eventService = new EventService(_eventDbContext, _applicationIdentityService);
        }

        [Fact]
        public async void StoreEvent_ShouldSucceed()
        {
            var randomTicks = DateTime.UtcNow.Ticks;
            var randomString = "1.1.1.1" + randomTicks;
            await _eventService.LogCustomEventAsync(_testUser.Id.ToString(), randomString);

            var customEventLogged = _eventDbContext.Events
                .Where(a => a.Data == randomString)
                .FirstOrDefault();

            Assert.NotNull(customEventLogged);
            Assert.Equal(customEventLogged.Data,randomString);
            Assert.Equal(customEventLogged.ActedByUser, _testUser.Id.ToString());
            Assert.True(customEventLogged.ActedByUser != "0"); // in case the user has no primary key before commit
        }
    }
}
