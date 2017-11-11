namespace TeamStore.Keeper.DataAccess
{
    using Microsoft.EntityFrameworkCore;
    using TeamStore.Keeper.Models;

    // NOTE: the decision is to keep the event audit context seperate, so we don't complicate
    // context.SaveChanges() calls and don't accidently save property changes on other entities

    public class EventDbContext : DbContext
    {
        public EventDbContext(DbContextOptions<EventDbContext> options, bool ensureFullDbCreated = false)
            : base(options)
        {
            if (Database != null && Database.IsSqlite())
            {
                Database.Migrate();
            }
        }

        public DbSet<Event> Events { get; set; }
    }
}
