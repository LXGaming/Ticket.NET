using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LXGaming.Ticket.Server.Storage {

    public class StorageContext : DbContext {

        protected StorageContext() {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
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