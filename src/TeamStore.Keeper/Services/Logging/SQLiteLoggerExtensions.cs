namespace TeamStore.Keeper.Services.Logging
{
    using Microsoft.Extensions.Logging;
    using System;
    using TeamStore.Keeper.DataAccess;

    public static class SQLiteLoggerExtensions
    {
        public static ILoggerFactory AddSQLiteLogger<TDbContext, TLogEntity>(
            this ILoggerFactory factory,
            IServiceProvider serviceProvider,
            Func<string, LogLevel, bool> filter = null)
            where TDbContext : ApplicationDbContext
            where TLogEntity : Models.Log, new()

        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            factory.AddProvider(new SQLiteLoggerProvider<TDbContext, TLogEntity>(serviceProvider, filter));

            return factory;
        }
    }
}
