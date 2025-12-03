using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using MyFinance.Models;
using MyFinance.Services;
using MyFinance.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MyFinance.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly AppDbContext _db;

        public SeriesCollection PieSeries { get; set; }
        public ObservableCollection<Transaction> Transactions { get; set; } = new();

        [ObservableProperty]
        private decimal _totalIncome;

        [ObservableProperty]
        private decimal _totalExpenses;

        [ObservableProperty]
        private decimal _totalBalance;

        [ObservableProperty]
        private Transaction _selectedTransaction;

        public MainViewModel()
        {
            _db = new AppDbContext();
            _db.Database.EnsureCreated();

            LoadTransactions();
        }

        private void LoadTransactions()
        {
            Transactions.Clear();

            var transactions = _db.Transactions
                .Include(t => t.Category)   // подтягиваем категорию
                .Include(t => t.Account)    // подтягиваем счет
                .OrderByDescending(t => t.Date)
                .ToList();

            foreach (var tx in transactions)
                Transactions.Add(tx);

            UpdateTotals();
        }

        [RelayCommand]
        private void AddTransaction()
        {
            var window = new Views.AddTransactionWindow();
            window.Owner = App.Current.MainWindow;
            if (window.ShowDialog() == true)
            {
                var tx = window.NewTransaction;
                _db.Transactions.Add(tx);
                _db.SaveChanges();
                Transactions.Insert(0, tx); // вставляем сверху
                LoadTransactions();
            }
        }


        [RelayCommand]
        private void EditTransaction(Transaction transaction)
        {
            if (transaction == null) return;

            // Создаём окно и передаём существующую транзакцию
            var editWindow = new AddTransactionWindow(transaction); // нужно добавить конструктор с параметром
            editWindow.Title = "Редактировать транзакцию";

            if (editWindow.ShowDialog() == true)
            {
                // Если пользователь нажал OK, обновляем данные
                var updatedTransaction = editWindow.NewTransaction;

                // Находим транзакцию в БД
                var txInDb = _db.Transactions.FirstOrDefault(t => t.Id == transaction.Id);
                if (txInDb != null)
                {
                    txInDb.Type = updatedTransaction.Type;
                    txInDb.Name = updatedTransaction.Name;
                    txInDb.CategoryId = updatedTransaction.CategoryId;
                    txInDb.AccountId = updatedTransaction.AccountId;
                    txInDb.Amount = updatedTransaction.Amount;
                    txInDb.Note = updatedTransaction.Note;
                    txInDb.Date = updatedTransaction.Date;

                    _db.SaveChanges();
                }

                // Перезагружаем коллекцию
                LoadTransactions();
            }
        }

        [RelayCommand]
        private void DeleteTransaction(Transaction transaction)
        {
            if (transaction == null) return;

            // Найти сущность в контексте
            var trackedTransaction = _db.Transactions.FirstOrDefault(t => t.Id == transaction.Id);
            if (trackedTransaction == null) return; // уже удалена

            // Найти счёт
            var account = _db.Accounts.FirstOrDefault(a => a.Id == trackedTransaction.AccountId);
            if (account != null)
            {
                if (trackedTransaction.Type == "Income")
                    account.Balance -= trackedTransaction.Amount; // уменьшаем доход
                else
                    account.Balance += trackedTransaction.Amount; // возвращаем расход
            }

            _db.Transactions.Remove(trackedTransaction);
            _db.SaveChanges();

            Transactions.Remove(transaction);
            LoadTransactions();
        }

        [RelayCommand]
        private void OpenCategory()
        {
            var window = new Views.ManageCategoriesWindow();
            window.Owner = App.Current.MainWindow;
            window.ShowDialog();
        }

        [RelayCommand]
        private void OpenAccount()
        {
            var window = new Views.ManageAccountsWindow();
            window.Owner = App.Current.MainWindow;
            window.ShowDialog();
        }

        private void UpdateTotals()
        {
            TotalIncome = Transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            TotalExpenses = Transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            TotalBalance = TotalIncome - TotalExpenses;

            UpdatePieSeries();
        }

        private void UpdatePieSeries()
        {
            PieSeries = new SeriesCollection
    {
        new PieSeries
        {
            Title = "Доходы",
            Values = new ChartValues<double> { (double)TotalIncomeCurrentMonth },
            PushOut = 0,
            Fill = new RadialGradientBrush(
                Color.FromRgb(144,238,144), // светлый
                Color.FromRgb(0,128,0))     // тёмный
            { GradientOrigin = new System.Windows.Point(0.3,0.3), Center = new System.Windows.Point(0.5,0.5), RadiusX=0.7, RadiusY=0.7 },
            Stroke = Brushes.DarkGreen,
            StrokeThickness = 1
        },
        new PieSeries
        {
            Title = "Расходы",
            Values = new ChartValues<double> { (double)TotalExpensesCurrentMonth },
            PushOut = 0,
            Fill = new RadialGradientBrush(
                Color.FromRgb(255,160,122),
                Color.FromRgb(178,34,34))
            { GradientOrigin = new System.Windows.Point(0.3,0.3), Center = new System.Windows.Point(0.5,0.5), RadiusX=0.7, RadiusY=0.7 },
            Stroke = Brushes.DarkRed,
            StrokeThickness = 1,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Gray,
                BlurRadius = 8,
                Direction = 320,
                ShadowDepth = 3
            }
        }
    };
            OnPropertyChanged(nameof(PieSeries));
        }

        public decimal TotalIncomeCurrentMonth =>
    Transactions
        .Where(t => t.Type == "Income" && t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year)
        .Sum(t => t.Amount);

        public decimal TotalExpensesCurrentMonth =>
            Transactions
                .Where(t => t.Type == "Expense" && t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year)
                .Sum(t => t.Amount);
        public string CurrentMonthName => DateTime.Now.ToString("MMMM", new CultureInfo("ru-RU"));
    }
}