namespace TeamStore.Keeper.Services.Logging
{
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;

    public class SQLiteLoggingOptions
    {
        public Dictionary<string, LogLevel> Filters { get; set; }
    }
}
