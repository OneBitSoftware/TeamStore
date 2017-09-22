namespace IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using TeamStore.Interfaces;
    using TeamStore.Services;

    public class EventServiceTests : IntegrationTestBase
    {
        public EventServiceTests()
        {
            _eventService = new EventService(_dbContext, _applicationIdentityService);
        }

        public void StoreEvent_ShouldSucceed()
        {
            // TODO: better testing here
            _eventService.StoreLoginEventAsync(null, "1.1.1.1");
        }
    }
}
