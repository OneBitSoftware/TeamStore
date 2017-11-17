namespace TeamStore.Keeper.DataAccess
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using TeamStore.Keeper.Models;

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            bool ensureFullDbCreated = false,
            bool applyMigrations = false,
            bool validateMigrationStatus = true) : base(options)
        {
            // EnsureCreated was removed due to EF Migrations, which fail if the DB is already created with EnsureCreated
            // EnsureCreated builds the DB based on this context, creating all tables. 
            // Migrations then fails on migrationBuilder.CreateTable(name: "ApplicationIdentities", because
            // the DB is already created with the schema from EnsureCreated.
            if (Database != null && Database.IsSqlite() && applyMigrations)
            {
                Console.WriteLine("ApplicationDbContext constructor called with applyMigrations " + applyMigrations.ToString());
                Database.Migrate();
            }
            Database.Migrate();

            if (Database != null && Database.IsSqlite())
            {
                var migrationsPending = Database.GetPendingMigrations() as ICollection<String>;

                if (validateMigrationStatus && migrationsPending != null && migrationsPending.Count > 0)
                    throw new ApplicationException("The current database is not up-to-date with the latest EventDbContext migrations. Run dotnet ef database update --context EventDbContext");

            }
        }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<Project> Projects { get; set; }
        //public DbSet<Event> Events { get; set; }
        public DbSet<AccessIdentifier> AccessIdentifiers { get; set; }
        public DbSet<ApplicationIdentity> ApplicationIdentities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Credential>();
            modelBuilder.Entity<Note>();
            modelBuilder.Entity<ApplicationIdentity>();
            modelBuilder.Entity<ApplicationUser>();
            modelBuilder.Entity<ApplicationGroup>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
