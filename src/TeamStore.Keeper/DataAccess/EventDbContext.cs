namespace TeamStore.Keeper.DataAccess
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using TeamStore.Keeper.Models;

    // NOTE: the decision is to keep the event audit context seperate, so we don't complicate
    // context.SaveChanges() calls and don't accidently save property changes on other entities
    // This means that calls must not be left open when doing operations that span both contexts to
    // avoid database locked errors and concurrency issues

    public class EventDbContext : DbContext
    {
        public EventDbContext(
            DbContextOptions<EventDbContext> options,
            bool ensureFullDbCreated = false,
            bool applyMigrations = false,
            bool validateMigrationStatus = true)
            : base(options)
        {
            if (Database != null && Database.IsSqlite() && applyMigrations)
            {
                Console.WriteLine("EventDbContext constructor called with applyMigrations " + applyMigrations.ToString());
            }

            Database.Migrate();


            if (Database != null && Database.IsSqlite())
            {
                var migrationsPending = Database.GetPendingMigrations() as ICollection<String>;

                if (validateMigrationStatus && migrationsPending != null && migrationsPending.Count > 0)
                    throw new ApplicationException("The current database is not up-to-date with the latest EventDbContext migrations. Run dotnet ef database update --context EventDbContext");
            }
        }

        public DbSet<Event> Events { get; set; }
    }
}
