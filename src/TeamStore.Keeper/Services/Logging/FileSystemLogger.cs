namespace TeamStore.Keeper.Services.Logging
{
    using System;
    using Microsoft.Extensions.Logging;
    using TeamStore.Keeper.Interfaces;

    public class FileSystemLogger : ILogger
    {
        public FileSystemLogger()
        {

        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            throw new NotImplementedException();
        }
    }
}
