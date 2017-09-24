namespace TeamStore.Keeper.Services.Logging
{
    using Microsoft.Extensions.Logging;
    using System;
    using TeamStore.Keeper.DataAccess;

    public class SQLiteLoggerProvider<TDbContext, TLogEntity> : ILoggerProvider
        where TLogEntity : Models.Log, new()
        where TDbContext : ApplicationDbContext
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly IServiceProvider _serviceProvider;

        public SQLiteLoggerProvider(IServiceProvider serviceProvider, Func<string, LogLevel, bool> filter)
        {
            _filter = filter;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new SQLiteLogger<TDbContext, TLogEntity>("SQLiteLogger", _filter, _serviceProvider);
        }

        public void Dispose()
        {

        }
    }
}