using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LXGaming.Ticket.Server.Storage.MySql {

    public class MySqlStorageContext : StorageContext {

        private readonly string _connectionString;

        public MySqlStorageContext(IConfiguration configuration) {
            _connectionString = configuration.GetConnectionString("MySql");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString), builder => {
                builder.EnableStringComparisonTranslations();
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
        }
    }
}