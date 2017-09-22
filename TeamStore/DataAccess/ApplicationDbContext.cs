using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamStore.Models;

namespace TeamStore.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, bool ensureFullDbCreated = false) : base(options)
        {
            // EnsureCreated was removed due to EF Migrations, which fail if the DB is already created with EnsureCreated
            // EnsureCreated builds the DB based on this context, creating all tables. Migrations then fails on migrationBuilder.CreateTable(name: "ApplicationIdentities",
            if (ensureFullDbCreated)
            {
                var created = Database.EnsureCreated();
            }
        }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Event> Events { get; set; }
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
