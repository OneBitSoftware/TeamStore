using System;
using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using TeamStore.Keeper.Models;
using TeamStore.Keeper.DataAccess;
using System.Collections.Generic;

namespace TeamStore.Keeper.Services.Logging
{
    public class SQLiteLogger<TDbContext, TLogEntity> : ILogger
        where TLogEntity : Models.Log, new()
        where TDbContext : ApplicationDbContext
    {
        private const int _indentation = 2;
        private readonly string _name;
        private readonly Func<string, LogLevel, bool> _filter;
        private IServiceProvider _services;

        public SQLiteLogger(string name, Func<string, LogLevel, bool> filter, IServiceProvider serviceProvider)
        {
            _name = name;
            _filter = filter ?? GetFilter(serviceProvider.GetService<IOptions<SQLiteLoggingOptions>>());
            _services = serviceProvider;
        }

        private Func<string, LogLevel, bool> GetFilter(IOptions<SQLiteLoggingOptions> options)
        {
            if (options != null)
            {
                return ((category, level) => GetFilter(options.Value, category, level));
            }
            else
                return ((category, level) => true);
        }

        private bool GetFilter(SQLiteLoggingOptions options, string category, LogLevel level)
        {
            if (options.Filters != null)
            {
                var filter = options.Filters.Keys.FirstOrDefault(p => category.StartsWith(p));
                if (filter != null)
                    return (int)options.Filters[filter] <= (int)level;
                else return true;
            }
            return true;
        }

        private static string Trim(string value, int maximumLength)
        {
            return value.Length > maximumLength ? value.Substring(0, maximumLength) : value;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _filter(_name, logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            var message = string.Empty;
            var values = state as IReadOnlyList<KeyValuePair<string, object>>;
            if (formatter != null)
            {
                message = formatter(state, exception);
            }
            else if (values != null)
            {
                var builder = new StringBuilder();
                FormatLogValues(
                    builder,
                    values,
                    level: 1,
                    bullet: false);
                message = builder.ToString();
                if (exception != null)
                {
                    message += Environment.NewLine + exception;
                }
            }
            else
            {
                message = "Boo";//LogFormatter.Formatter(state, exception);
            }
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var log = new TLogEntity
            {
                Message = Trim(message, Models.Log.MaximumMessageLength),
                Date = DateTime.UtcNow,
                Level = logLevel.ToString(),
                Logger = _name,
                Thread = eventId.ToString()
            };

            if (exception != null)
                log.Exception = Trim(exception.ToString(), Models.Log.MaximumExceptionLength);

            var httpContext = _services.GetRequiredService<IHttpContextAccessor>()?.HttpContext;

            if (httpContext != null)
            {
                log.Browser = httpContext.Request.Headers["User-Agent"];
                log.Username = httpContext.User.Identity.Name;
                try { log.HostAddress = httpContext.Connection.LocalIpAddress?.ToString(); }
                catch (ObjectDisposedException) { log.HostAddress = "Disposed"; }
                log.Url = httpContext.Request.Path;
            }

            var db = _services.GetRequiredService<TDbContext>();
            db.Set<TLogEntity>().Add(log);

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }


        }

        private void FormatLogValues(StringBuilder builder, IReadOnlyList<KeyValuePair<string, object>> logValues, int level, bool bullet)
        {
            var values = logValues;
            if (values == null)
            {
                return;
            }
            var isFirst = true;
            foreach (var kvp in values) // changed to build
            {
                builder.AppendLine();
                if (bullet && isFirst)
                {
                    builder.Append(' ', level * _indentation - 1)
                           .Append('-');
                }
                else
                {
                    builder.Append(' ', level * _indentation);
                }
                builder.Append(kvp.Key)
                       .Append(": ");
                if (kvp.Value is IEnumerable && !(kvp.Value is string))
                {
                    foreach (var value in (IEnumerable)kvp.Value)
                    {
                        if (value is IReadOnlyList<KeyValuePair<string, object>>)
                        {
                            throw new NotImplementedException();
                            //FormatLogValues(
                            //    builder,
                            //    (ILogValues)value,
                            //    level + 1,
                            //    bullet: true);
                        }
                        else
                        {
                            builder.AppendLine()
                                   .Append(' ', (level + 1) * _indentation)
                                   .Append(value);
                        }
                    }
                }
                else if (kvp.Value is IReadOnlyList<KeyValuePair<string, object>>)
                {
                    throw new NotImplementedException();
                    //FormatLogValues(
                    //    builder,
                    //    (ILogValues)kvp.Value,
                    //    level + 1,
                    //    bullet: false);
                }
                else
                {
                    builder.Append(kvp.Value);
                }
                isFirst = false;
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NoopDisposable();
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
