using Microsoft.EntityFrameworkCore;
using MyFinance.Models;
using System.IO;

namespace MyFinance.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var dbPath = Path.Combine(folder, "finance.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
