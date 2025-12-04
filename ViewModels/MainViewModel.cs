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
using System.Windows.Media;

namespace MyFinance.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly AppDbContext _db;

        public SeriesCollection PieSeries { get; set; }
        public ObservableCollection<Transaction> Transactions { get; set; } = new();

        public ObservableCollection<string> PeriodOptions { get; set; } = new ObservableCollection<string>
        {
            "День", "Неделя", "Месяц", "Год"
        };
        public ObservableCollection<string> AccountOptions { get; set; } = new ();
        public ObservableCollection<string> CategoryOptions { get; set; } = new ObservableCollection<string>
        {
            "Все"
        };

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

        public void LoadTransactions()
        {
            Transactions.Clear();

            var transactions = _db.Transactions
                .Include(t => t.Category)   // подтягиваем категорию
                .Include(t => t.Account)    // подтягиваем счет
                .OrderByDescending(t => t.Date)
                .ToList();

            foreach (var tx in transactions)
                Transactions.Add(tx);

            AccountOptions.Clear();
            AccountOptions.Add("Все");
            foreach (var account in _db.Accounts.OrderBy(a => a.Name))
            {
                AccountOptions.Add(account.Name);
            }

            UpdateTotals();
            UpdateTotalBalanceSeries();
            UpdateIncomeExpenseSeries();
            UpdateAccountBalanceLineSeries();
            UpdateCategorySeries();
            UpdateAccountBalancePieSeries();
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

                // обновление баланса
                var account = _db.Accounts.FirstOrDefault(a => a.Id == tx.AccountId);
                if (tx.Type == "Income")
                    account.Balance += tx.Amount;
                else
                    account.Balance -= tx.Amount;

                _db.SaveChanges();
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

                // обновление баланса
                var account = _db.Accounts.FirstOrDefault(a => a.Id == transaction.AccountId);
                if (transaction.Type == "Income")
                    account.Balance -= transaction.Amount;
                else
                    account.Balance += transaction.Amount;

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
                }

                // обновление баланса
                if (updatedTransaction.Type == "Income")
                    account.Balance += updatedTransaction.Amount;
                else
                    account.Balance -= updatedTransaction.Amount;

                _db.SaveChanges();
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
                if (trackedTransaction.Type == "Income")
                    account.Balance -= trackedTransaction.Amount; // уменьшаем доход
                else
                    account.Balance += trackedTransaction.Amount; // возвращаем расход

            _db.Transactions.Remove(trackedTransaction);
            _db.SaveChanges();
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

            TotalBalance = _db.Accounts.Sum(a => a.Balance);

            OnPropertyChanged(nameof(TotalIncomeCurrentMonth));
            OnPropertyChanged(nameof(TotalExpensesCurrentMonth));

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



        [ObservableProperty]
        private string _selectedPeriod = "День";

        [ObservableProperty]
        private string _selectedAccount = "Все";

        public ObservableCollection<string> TotalBalanceLabels { get; set; } = new();

        public SeriesCollection TotalBalanceSeries { get; set; } = new();
        public SeriesCollection IncomeExpenseSeries { get; set; } = new();
        public SeriesCollection CategorySeries { get; set; } = new();
        public SeriesCollection AccountBalancePieSeries { get; set; } = new();
        public SeriesCollection AccountBalanceLineSeries { get; set; } = new();

        public ObservableCollection<string> AccountLabels { get; set; } = new();

        public Func<double, string> AmountFormatter => val => val.ToString("N2");

        partial void OnSelectedPeriodChanged(string oldValue, string newValue)
        {
            UpdateTotalBalanceSeries();
        }

        partial void OnSelectedAccountChanged(string oldValue, string newValue)
        {
            UpdateIncomeExpenseSeries();
            UpdateCategorySeries();
        }

        private void UpdateTotalBalanceSeries()
        {
            TotalBalanceSeries.Clear();
            TotalBalanceLabels.Clear();

            if (!Transactions.Any()) return;

            DateTime startDate = Transactions.Min(t => t.Date).Date;
            DateTime endDate = Transactions.Max(t => t.Date).Date;
            decimal runningBalance = 0;

            var values = new ChartValues<double>();

            switch (SelectedPeriod)
            {
                case "День":
                    var dailySums = Transactions
                        .GroupBy(t => t.Date.Date)
                        .ToDictionary(g => g.Key, g => g.Sum(t => t.Type == "Income" ? t.Amount : -t.Amount));

                    for (var date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        if (dailySums.ContainsKey(date))
                            runningBalance += dailySums[date];

                        values.Add((double)runningBalance);
                        TotalBalanceLabels.Add(date.ToString("dd.MM"));
                    }
                    break;

                case "Неделя":
                    // получаем номер недели для каждой транзакции
                    var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
                    var weeklySums = Transactions
                        .GroupBy(t => calendar.GetWeekOfYear(t.Date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                        .OrderBy(g => g.Key)
                        .ToDictionary(g => g.Key, g => g.Sum(t => t.Type == "Income" ? t.Amount : -t.Amount));

                    int startWeek = calendar.GetWeekOfYear(startDate, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                    int endWeek = calendar.GetWeekOfYear(endDate, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);

                    for (int week = startWeek; week <= endWeek; week++)
                    {
                        if (weeklySums.ContainsKey(week))
                            runningBalance += weeklySums[week];

                        values.Add((double)runningBalance);
                        TotalBalanceLabels.Add("Неделя " + week);
                    }
                    break;

                case "Месяц":
                    var monthlySums = Transactions
                        .GroupBy(t => new { t.Date.Year, t.Date.Month })
                        .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                        .ToDictionary(g => g.Key, g => g.Sum(t => t.Type == "Income" ? t.Amount : -t.Amount));

                    for (var date = new DateTime(startDate.Year, startDate.Month, 1);
                         date <= new DateTime(endDate.Year, endDate.Month, 1);
                         date = date.AddMonths(1))
                    {
                        var key = new { date.Year, date.Month };
                        if (monthlySums.ContainsKey(key))
                            runningBalance += monthlySums[key];

                        values.Add((double)runningBalance);
                        TotalBalanceLabels.Add(date.ToString("MM.yyyy"));
                    }
                    break;

                case "Год":
                    var yearlySums = Transactions
                        .GroupBy(t => t.Date.Year)
                        .OrderBy(g => g.Key)
                        .ToDictionary(g => g.Key, g => g.Sum(t => t.Type == "Income" ? t.Amount : -t.Amount));

                    for (int year = startDate.Year; year <= endDate.Year; year++)
                    {
                        if (yearlySums.ContainsKey(year))
                            runningBalance += yearlySums[year];

                        values.Add((double)runningBalance);
                        TotalBalanceLabels.Add(year.ToString());
                    }
                    break;
            }

            TotalBalanceSeries.Add(new ColumnSeries
            {
                Title = "Общий баланс",
                Values = values,
                ColumnPadding = 0,
                MaxColumnWidth = 20
            });

            OnPropertyChanged(nameof(TotalBalanceLabels));
            OnPropertyChanged(nameof(TotalBalanceSeries));
        }

        private void UpdateIncomeExpenseSeries()
        {
            IncomeExpenseSeries.Clear();

            var filtered = SelectedAccount == "Все"
                ? Transactions
                : Transactions.Where(t => t.Account.Name == SelectedAccount);

            decimal totalIncome = filtered.Where(t => t.Type == "Income").Sum(t => t.Amount);
            decimal totalExpense = filtered.Where(t => t.Type == "Expense").Sum(t => t.Amount);

            IncomeExpenseSeries.Add(new PieSeries
            {
                Title = "Доходы",
                Values = new ChartValues<decimal> { totalIncome },
                DataLabels = true,
                Fill = Brushes.Green
            });

            IncomeExpenseSeries.Add(new PieSeries
            {
                Title = "Расходы",
                Values = new ChartValues<decimal> { totalExpense },
                DataLabels = true,
                Fill = Brushes.Red
            });

            OnPropertyChanged(nameof(IncomeExpenseSeries));
        }

        private void UpdateAccountBalancePieSeries()
        {
            AccountBalancePieSeries.Clear();

            foreach (var account in _db.Accounts.ToList())
            {
                AccountBalancePieSeries.Add(new PieSeries
                {
                    Title = account.Name,
                    Values = new ChartValues<double> { (double)account.Balance },
                    DataLabels = true
                });
            }

            OnPropertyChanged(nameof(AccountBalancePieSeries));
        }

        private void UpdateAccountBalanceLineSeries()
        {
            AccountBalanceLineSeries.Clear();
            AccountLabels.Clear();

            if (!Transactions.Any()) return;

            // Получаем список дат, отсортированных
            var dates = Transactions.Select(t => t.Date.Date).Distinct().OrderBy(d => d).ToList();
            foreach (var date in dates)
                AccountLabels.Add(date.ToString("dd.MM"));

            // Для каждого счета создаём LineSeries
            var accounts = _db.Accounts.ToList();
            foreach (var account in accounts)
            {
                var values = new ChartValues<double>();
                decimal runningBalance = 0;

                foreach (var date in dates)
                {
                    // Суммируем транзакции по данному счету на текущую дату
                    var dailyTransactions = Transactions
                        .Where(t => t.AccountId == account.Id && t.Date.Date == date);

                    foreach (var t in dailyTransactions)
                    {
                        runningBalance += t.Type == "Income" ? t.Amount : -t.Amount;
                    }

                    values.Add((double)runningBalance);
                }

                AccountBalanceLineSeries.Add(new LineSeries
                {
                    Title = account.Name,
                    Values = values,
                    PointGeometrySize = 5
                });
            }

            OnPropertyChanged(nameof(AccountBalanceLineSeries));
            OnPropertyChanged(nameof(AccountLabels));
        }

        private void UpdateCategorySeries()
        {
            CategorySeries.Clear();

            var filtered = SelectedAccount == "Все"
                ? Transactions
                : Transactions.Where(t => t.Account.Name == SelectedAccount);

            // Доходы
            var incomeGroups = filtered
                .Where(t => t.Type == "Income")
                .GroupBy(t => t.Category)
                .ToList();

            foreach (var group in incomeGroups)
            {
                var category = group.Key;
                CategorySeries.Add(new PieSeries
                {
                    Title = category?.Name ?? "Без категории",
                    Values = new ChartValues<decimal> { group.Sum(t => t.Amount) },
                    DataLabels = true,
                    Fill = category != null ? (SolidColorBrush)(new BrushConverter().ConvertFromString(category.Color)) : Brushes.Green
                });
            }

            // Расходы
            var expenseGroups = filtered
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .ToList();

            foreach (var group in expenseGroups)
            {
                var category = group.Key;
                CategorySeries.Add(new PieSeries
                {
                    Title = category?.Name ?? "Без категории",
                    Values = new ChartValues<decimal> { group.Sum(t => t.Amount) },
                    DataLabels = true,
                    Fill = category != null ? (SolidColorBrush)(new BrushConverter().ConvertFromString(category.Color)) : Brushes.Red
                });
            }

            OnPropertyChanged(nameof(CategorySeries));
        }
    }
}