using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TeamStore.Keeper.DataAccess;

namespace TeamStore.Keeper.Factories
{
    public class EventDbContextFactory : IDesignTimeDbContextFactory<EventDbContext>
    {
        public EventDbContext CreateDbContext(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var localConfigurationFile = configurationBuilder.Build();
            var fileName = localConfigurationFile["DataAccess:SQLiteDbFileName"];
            var connectionString = "Data Source=" + fileName;

            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Missing connection string in appsettings.json.");

            var builder = new DbContextOptionsBuilder<EventDbContext>();
            builder.UseSqlite(connectionString);

            return new EventDbContext(builder.Options);
        }
    }
}
