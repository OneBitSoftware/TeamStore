namespace TeamStore.Keeper.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface ITelemetryService
    {
        void TraceEvent(string message, string key1, string value1);
        void TrackException(Exception ex);
    }
}
