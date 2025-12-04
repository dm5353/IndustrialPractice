using System.Windows;
using System.Windows.Controls;

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

        private void Balance_Click(object sender, RoutedEventArgs e) => ShowDetail(BalanceDetail);
        private void Expenses_Click(object sender, RoutedEventArgs e) => ShowDetail(ExpensesDetail);
        private void Income_Click(object sender, RoutedEventArgs e) => ShowDetail(IncomeDetail);
        private void BackButton_Click(object sender, RoutedEventArgs e) => ShowDetail(MainContent);

        private void ShowDetail(Grid detailGrid)
        {
            MainContent.Visibility = Visibility.Collapsed;
            BalanceDetail.Visibility = Visibility.Collapsed;
            ExpensesDetail.Visibility = Visibility.Collapsed;
            IncomeDetail.Visibility = Visibility.Collapsed;

            detailGrid.Visibility = Visibility.Visible;
        }
    }
}
