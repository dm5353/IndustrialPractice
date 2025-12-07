using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MyFinance.ViewModels;

namespace MyFinance.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var cvsExpense = (CollectionViewSource)FindResource("GroupedExpenseTransactions");
            cvsExpense.Filter += (s, e) => e.Accepted = ((MainViewModel)DataContext).ExpenseTransactionsFilter(e.Item);

            var cvsIncome = (CollectionViewSource)FindResource("GroupedIncomeTransactions");
            cvsIncome.Filter += (s, e) => e.Accepted = ((MainViewModel)DataContext).IncomeTransactionsFilter(e.Item);
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