using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Text;
using TeamStore.Keeper.Interfaces;

namespace TeamStore.Keeper.Services
{
    public class TelemetryService : ITelemetryService
    {
        private TelemetryClient _telemetryClient;

        public TelemetryService()
        {
            _telemetryClient = new TelemetryClient();

        }

        public void TraceEvent(string message, string key1, string value1)
        {

            try
            {
                var dictionary = new Dictionary<string, string>();

                dictionary.Add(key1, value1);

                _telemetryClient.TrackEvent(message, dictionary);
            }
            catch            {

               // to avoid issues
            }
        }

        public void TrackException(Exception ex)
        {
            _telemetryClient.TrackException(ex);
        }
    }
}
