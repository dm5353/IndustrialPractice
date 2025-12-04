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

        public AppDbContext()
        {
            if (!Accounts.Any()) // добавляем только если нет записей
            {
                Accounts.Add(new Account { Name = "Карта", Balance = 0 });
                Accounts.Add(new Account { Name = "Наличные", Balance = 0 });
            }

            if (!Categories.Any())
            {
                // Доходы
                Categories.Add(new Category { Name = "Зарплата", Type = "Income", Color = "#32CD32" });
                Categories.Add(new Category { Name = "Подарок", Type = "Income", Color = "#00CED1" });
                Categories.Add(new Category { Name = "Проценты по вкладу", Type = "Income", Color = "#3CB371" });
                Categories.Add(new Category { Name = "Кешбек", Type = "Income", Color = "#2E8B57" });

                // Расходы
                Categories.Add(new Category { Name = "Еда", Type = "Expense", Color = "#FF6347" });
                Categories.Add(new Category { Name = "Транспорт", Type = "Expense", Color = "#FF4500" });
                Categories.Add(new Category { Name = "Развлечения", Type = "Expense", Color = "#FF8C00" });
                Categories.Add(new Category { Name = "Одежда", Type = "Expense", Color = "#FF69B4" });
                Categories.Add(new Category { Name = "Медицина", Type = "Expense", Color = "#DC143C" });
                Categories.Add(new Category { Name = "Коммунальные платежи", Type = "Expense", Color = "#B22222" });
                Categories.Add(new Category { Name = "Образование", Type = "Expense", Color = "#FF1493" });
            }

            SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var dbPath = Path.Combine(folder, "finance.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transaction>()
    .HasOne(t => t.Account)
    .WithMany(a => a.Transactions)
    .HasForeignKey(t => t.AccountId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaction>()
    .HasOne(t => t.Category)
    .WithMany(c => c.Transactions)
    .HasForeignKey(t => t.CategoryId)
    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
