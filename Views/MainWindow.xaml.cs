using System.Windows;

namespace MyFinance.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Balance_Click(object sender, RoutedEventArgs e) => BalanceDetail.Visibility = Visibility.Visible;
        private void Expenses_Click(object sender, RoutedEventArgs e) => BalanceDetail.Visibility = Visibility.Visible;
        private void Income_Click(object sender, RoutedEventArgs e) => BalanceDetail.Visibility = Visibility.Visible;

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BalanceDetail.Visibility = Visibility.Collapsed;
            ExpensesDetail.Visibility = Visibility.Collapsed;
            IncomeDetail.Visibility = Visibility.Collapsed;
        }
    }
}
