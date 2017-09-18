namespace IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using TeamStore.Interfaces;
    using TeamStore.Services;

    public class EventServiceTests : IntegrationTestBase
    {
        IEventService _eventService;

        public EventServiceTests()
        {
            _eventService = new EventService(_dbContext, _applicationIdentityService);
        }

        public void StoreEvent_ShouldSucceed()
        {
            _eventService.StoreLoginEventAsync(null, "1.1.1.1");
        }
    }
}
