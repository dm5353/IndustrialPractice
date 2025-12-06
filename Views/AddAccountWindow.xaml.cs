using MyFinance.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyFinance.Views
{
    public partial class AddAccountWindow : Window
    {
        public Account NewAccount { get; private set; }

        public AddAccountWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите название счёта");
                return;
            }

            NewAccount = new Account
            {
                Name = NameBox.Text.Trim(),
                Balance = 0
            };

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}