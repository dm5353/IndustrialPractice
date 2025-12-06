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
using System.Security.Principal;
using System.Windows.Media;

namespace MyFinance.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly AppDbContext _db;

        [ObservableProperty] private decimal _totalIncome;
        [ObservableProperty] private decimal _totalExpenses;
        [ObservableProperty] private decimal _totalBalance;

        [ObservableProperty] private Transaction _selectedTransaction;

        private string _selectedCategoriesText = "Категории";
        [ObservableProperty] private string _selectedPeriod = "По дням";
        [ObservableProperty] private string _selectedPeriodBalance = "За всё время";
        [ObservableProperty] private string _selectedAccounts = "Все";

        public SeriesCollection PieSeries { get; set; }
        public SeriesCollection TotalBalanceSeries { get; set; } = new();
        public SeriesCollection IncomeExpenseSeries { get; set; } = new();
        public SeriesCollection CategorySeries { get; set; } = new();
        public SeriesCollection AccountBalancePieSeries { get; set; } = new();
        public SeriesCollection AccountBalanceLineSeries { get; set; } = new();

        public ObservableCollection<Transaction> Transactions { get; set; } = new();
        public ObservableCollection<Transaction> ExpenseTransactions { get; set; } = new();
        public ObservableCollection<Transaction> IncomeTransactions { get; set; } = new();

        public ObservableCollection<CategorySelectItem> CategoryOptions { get; set; } = new ObservableCollection<CategorySelectItem>();
        public ObservableCollection<string> AccountOptions { get; set; } = new ();
        public ObservableCollection<string> TotalBalanceLabels { get; set; } = new();
        public ObservableCollection<string> AccountLabels { get; set; } = new();
        public ObservableCollection<string> PeriodOptions { get; set; } = new ObservableCollection<string>
        {
            "По дням", "По неделям", "По месяцам", "По годам"
        };
        public ObservableCollection<string> PeriodBalanceOptions { get; set; } = new ObservableCollection<string>
        {
            "За всё время", "Неделя", "Месяц", "Год"
        };

        public Func<double, string> AmountFormatter => val => val.ToString("N2");
        public object Dummy { get; set; }
        public bool IsCategoryDropdownOpen { get; set; }
        public string SelectedCategoriesText
        {
            get => _selectedCategoriesText;
            set
            {
                _selectedCategoriesText = value;
                OnPropertyChanged(nameof(SelectedCategoriesText));
            }
        }

        public MainViewModel()
        {
            _db = new AppDbContext();
            _db.Database.EnsureCreated();

            LoadTransactions();
            CategorySelectItem.CategoryChanged = OnCategorySelectionChanged;
        }

        private void OnCategorySelectionChanged()
        {
            UpdateSelectedCategoriesText();
            UpdateIncomeExpenseDiagram();
            UpdateIncomeExpenseCategoryDiagram();
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
            {
                Transactions.Add(tx);

                if (tx.Type == "Expense")
                    ExpenseTransactions.Add(tx);
                else IncomeTransactions.Add(tx);
            }

            AccountOptions.Clear();
            AccountOptions.Add("Все");
            foreach (var account in _db.Accounts.OrderBy(a => a.Name))
            {
                AccountOptions.Add(account.Name);
            }

            CategoryOptions.Clear();
            foreach (var category in _db.Categories.OrderBy(a => a.Name))
            {
                CategoryOptions.Add(new CategorySelectItem
                {
                    Name = category.Name,
                    IsSelected = true
                });
            }

            UpdateTotals();
            UpdateBalanceDiagrams();
        }

        private void UpdateBalanceDiagrams()
        {
            UpdateBalanceDiagram();
            UpdateAccountBalanceDiagram();
            UpdateAccountBalancePieDiagram();
            UpdateIncomeExpenseDiagram();
            UpdateIncomeExpenseCategoryDiagram();
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

        private void UpdateBalanceDiagram()
        {
            TotalBalanceSeries.Clear();
            TotalBalanceLabels.Clear();

            var filteredTransactions = FilterByPeriod(Transactions).ToList();
            if (!filteredTransactions.Any()) return;

            DateTime startDate = filteredTransactions.Min(t => t.Date).Date;
            DateTime endDate = DateTime.Now;
            decimal runningBalance = 0;

            var values = new ChartValues<double>();

            switch (SelectedPeriod)
            {
                case "По дням":
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

                case "По неделям":
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

                case "По месяцам":
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

                case "По годам":
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

        private void UpdateIncomeExpenseDiagram()
        {
            IncomeExpenseSeries.Clear();

            var filteredTransactions = FilterByPeriod(Transactions);
            filteredTransactions = FilterByAccounts(filteredTransactions);
            filteredTransactions = FilterByCategories(filteredTransactions);
            if (!filteredTransactions.Any()) return;

            decimal totalIncome = filteredTransactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            decimal totalExpense = filteredTransactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);

            IncomeExpenseSeries.Add(new PieSeries
            {
                Title = $"Доходы - {totalIncome:N0} ₽",
                Values = new ChartValues<decimal> { totalIncome },
                DataLabels = true,
                Fill = Brushes.Green,
                LabelPoint = cp => $"{cp.Participation:P0}"
            });

            IncomeExpenseSeries.Add(new PieSeries
            {
                Title = $"Расходы - {totalExpense:N0} ₽",
                Values = new ChartValues<decimal> { totalExpense },
                DataLabels = true,
                Fill = Brushes.Red,
                LabelPoint = cp => $"{cp.Participation:P0}"
            });

            OnPropertyChanged(nameof(IncomeExpenseSeries));
        }

        private void UpdateAccountBalancePieDiagram()
        {
            AccountBalancePieSeries.Clear();

            var filteredTransactions = FilterByPeriod(Transactions);
            if (!filteredTransactions.Any()) return;

            var accounts = _db.Accounts.ToList();

            foreach (var acc in accounts)
            {
                decimal balance = filteredTransactions
                    .Where(t => t.AccountId == acc.Id)
                    .Sum(t => t.Type == "Income" ? t.Amount : -t.Amount);

                AccountBalancePieSeries.Add(new PieSeries
                {
                    Title = $"{acc.Name} - {balance:N0} ₽",
                    Values = new ChartValues<double> { (double)balance },
                    LabelPoint = cp => $"{cp.Participation:P0}",
                    DataLabels = true
                });
            }

            OnPropertyChanged(nameof(AccountBalancePieSeries));
        }

        private void UpdateAccountBalanceDiagram()
        {
            AccountBalanceLineSeries.Clear();
            AccountLabels.Clear();

            var filteredTransactions = FilterByPeriod(Transactions);
            if (!filteredTransactions.Any()) return;

            var dates = filteredTransactions.Select(t => t.Date.Date).Distinct().OrderBy(d => d).ToList();
            foreach (var date in dates)
                AccountLabels.Add(date.ToString("dd.MM"));

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

        private void UpdateIncomeExpenseCategoryDiagram()
        {
            CategorySeries.Clear();

            var filteredTransactions = FilterByPeriod(Transactions);
            filteredTransactions = FilterByAccounts(filteredTransactions);
            filteredTransactions = FilterByCategories(filteredTransactions);
            if (!filteredTransactions.Any()) return;

            // Доходы
            var incomeGroups = filteredTransactions
                .Where(t => t.Type == "Income")
                .GroupBy(t => t.Category)
                .ToList();

            foreach (var group in incomeGroups)
            {
                var category = group.Key;
                CategorySeries.Add(new PieSeries
                {
                    Title = $"{category?.Name ?? "Без категории"} - {group.Sum(t => t.Amount):N0} ₽",
                    Values = new ChartValues<decimal> { group.Sum(t => t.Amount) },
                    DataLabels = true,
                    LabelPoint = cp => $"{cp.Participation:P0}",
                    Fill = category != null ? (SolidColorBrush)(new BrushConverter().ConvertFromString(category.Color)) : Brushes.Green
                });
            }

            // Расходы
            var expenseGroups = filteredTransactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .ToList();

            foreach (var group in expenseGroups)
            {
                var category = group.Key;
                CategorySeries.Add(new PieSeries
                {
                    Title = $"{category?.Name ?? "Без категории"} - {group.Sum(t => t.Amount):N0} ₽",
                    LabelPoint = cp => $"{cp.Participation:P0}",
                    Values = new ChartValues<decimal> { group.Sum(t => t.Amount) },
                    DataLabels = true,
                    Fill = category != null ? (SolidColorBrush)(new BrushConverter().ConvertFromString(category.Color)) : Brushes.Red
                });
            }

            OnPropertyChanged(nameof(CategorySeries));
        }

        private void UpdateSelectedCategoriesText()
        {
            var selected = CategoryOptions
                .Where(c => c.IsSelected)
                .Select(c => c.Name)
                .ToList();

            if (selected.Count == 0)
                SelectedCategoriesText = "Категории";
            else
                SelectedCategoriesText = string.Join(", ", selected);
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

        public decimal TotalIncomeCurrentMonth =>
            Transactions
                .Where(t => t.Type == "Income" && t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year)
                .Sum(t => t.Amount);

        public decimal TotalExpensesCurrentMonth =>
            Transactions
                .Where(t => t.Type == "Expense" && t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year)
                .Sum(t => t.Amount);
        public string CurrentMonthName => DateTime.Now.ToString("MMMM", new CultureInfo("ru-RU"));

        public IEnumerable<string> SelectedCategories =>
            CategoryOptions
                .Where(c => c.IsSelected)
                .Select(c => c.Name);

        private IEnumerable<Transaction> FilterByPeriod(IEnumerable<Transaction> source)
        {
            var now = DateTime.Now;

            return SelectedPeriodBalance switch
            {
                "Неделя" =>
                    source.Where(t => t.Date >= GetMonday(now)
                    ),

                "Месяц" =>
                    source.Where(t =>
                        t.Date.Year == now.Year &&
                        t.Date.Month == now.Month
                    ),

                "Год" =>
                    source.Where(t => t.Date.Year == now.Year),

                _ => source
            };
        }

        private IEnumerable<Transaction> FilterByAccounts(IEnumerable<Transaction> source)
        {
            return SelectedAccounts == "Все"
                ? source
                : source.Where(t => t.Account.Name == SelectedAccounts);
        }

        private IEnumerable<Transaction> FilterByCategories(IEnumerable<Transaction> source)
        {
            return source.Where(t => SelectedCategories.ToList().Contains(t.Category.Name));
        }

        private DateTime GetMonday(DateTime date)
        {
            int diff = (7 + (int)date.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return date.Date.AddDays(-diff);
        }

        partial void OnSelectedPeriodBalanceChanged(string oldValue, string newValue) => UpdateBalanceDiagrams();
        partial void OnSelectedPeriodChanged(string oldValue, string newValue) => UpdateBalanceDiagram();
        partial void OnSelectedAccountsChanged(string oldValue, string newValue)
        {
            UpdateIncomeExpenseDiagram();
            UpdateIncomeExpenseCategoryDiagram();
        }
    }
}