using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Wpf;
using MyFinance.Models;
using MyFinance.Services;
using System.Collections.ObjectModel;
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
            UpdatePieSeries();
        }

        private void LoadTransactions()
        {
            Transactions.Clear();
            foreach (var tx in _db.Transactions.OrderByDescending(t => t.Date))
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
                UpdateTotals();
                UpdatePieSeries();
            }
        }


        [RelayCommand]
        private void EditTransaction(Transaction transaction)
        {
            if (transaction == null) return;
            // Логика редактирования через окно диалога
        }

        [RelayCommand]
        private void DeleteTransaction(Transaction transaction)
        {
            if (transaction == null) return;
            _db.Transactions.Remove(transaction);
            _db.SaveChanges();
            Transactions.Remove(transaction);
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            TotalIncome = Transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            TotalExpenses = Transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            TotalBalance = TotalIncome - TotalExpenses;
        }

        private void UpdatePieSeries()
        {
            PieSeries = new SeriesCollection
    {
        new PieSeries
        {
            Title = "Доходы",
            Values = new ChartValues<double> { (double)TotalIncome },
            PushOut = 5,
            Fill = new RadialGradientBrush(
                Color.FromRgb(144,238,144), // светлый
                Color.FromRgb(0,128,0))     // тёмный
            { GradientOrigin = new System.Windows.Point(0.3,0.3), Center = new System.Windows.Point(0.5,0.5), RadiusX=0.7, RadiusY=0.7 },
            Stroke = Brushes.DarkGreen,
            StrokeThickness = 1,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Gray,
                BlurRadius = 8,
                Direction = 320,
                ShadowDepth = 3
            }
        },
        new PieSeries
        {
            Title = "Расходы",
            Values = new ChartValues<double> { (double)TotalExpenses },
            PushOut = 5,
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
    }
}