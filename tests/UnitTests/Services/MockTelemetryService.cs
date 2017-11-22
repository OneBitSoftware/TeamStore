using System;
using System.Collections.Generic;
using System.Text;
using TeamStore.Keeper.Interfaces;

namespace UnitTests.Services
{
    internal class MockTelemetryService : ITelemetryService
    {
        public void TraceEvent(string message, string key1, string value1)
        {
            // do nothing
        }

        public void TrackException(Exception ex)
        {
            // do nothing
        }
    }
}
