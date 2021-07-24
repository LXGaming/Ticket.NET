using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LXGaming.Ticket.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace LXGaming.Ticket.Server.Storage {

    public class StorageContext : DbContext {

        public DbSet<Identifier> Identifiers { get; init; }
        public DbSet<Issue> Issues { get; init; }
        public DbSet<IssueComment> IssueComments { get; init; }
        public DbSet<Project> Projects { get; init; }
        public DbSet<Token> Tokens { get; init; }
        public DbSet<User> Users { get; init; }
        public DbSet<UserIdentifier> UserIdentifiers { get; init; }
        public DbSet<UserProject> UserProjects { get; init; }

        protected StorageContext() {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            // Fixes
            modelBuilder.Entity<Issue>().Property(model => model.Status).HasConversion<string>();
            modelBuilder.Entity<UserIdentifier>().HasIndex(model => new {model.IdentifierId, model.UserId}).IsUnique();
            modelBuilder.Entity<UserIdentifier>().HasIndex(model => new {model.IdentifierId, model.Value}).IsUnique();
            modelBuilder.Entity<UserProject>().HasIndex(model => new {model.ProjectId, model.UserId}).IsUnique();

            // Defaults
            modelBuilder.Entity<Project>().HasData(
                new Project {Id = "gta5", Name = "Grand Theft Auto V"},
                new Project {Id = "minecraft", Name = "Minecraft: Java Edition"},
                new Project {Id = "rdr3", Name = "Red Dead Redemption 2"});

            modelBuilder.Entity<Identifier>().HasData(
                new Identifier {Id = "discord", Name = "Discord"},
                new Identifier {Id = "gta5", Name = "Grand Theft Auto V"},
                new Identifier {Id = "minecraft", Name = "Minecraft: Java Edition"},
                new Identifier {Id = "rdr3", Name = "Red Dead Redemption 2"},
                new Identifier {Id = "steam", Name = "Steam"});
        }

        public override int SaveChanges() {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps() {
            var now = DateTime.Now;
            var entities = ChangeTracker.Entries();
            foreach (var entity in entities) {
                var createdAt = entity.Properties.SingleOrDefault(entry => entry.Metadata.Name.Equals("CreatedAt"));
                if (createdAt != null && entity.State == EntityState.Added) {
                    if (createdAt.CurrentValue == null || createdAt.CurrentValue.Equals(default(DateTime))) {
                        createdAt.CurrentValue = now;
                    }
                }

                var updatedAt = entity.Properties.SingleOrDefault(entry => entry.Metadata.Name.Equals("UpdatedAt"));
                if (updatedAt != null && (entity.State == EntityState.Added || entity.State == EntityState.Modified)) {
                    updatedAt.CurrentValue = now;
                }
            }
        }
    }
}