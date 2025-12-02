using MyFinance.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MyFinance.Views
{
    public partial class AddTransactionWindow : Window
    {
        public Transaction NewTransaction { get; private set; }

        public AddTransactionWindow()
        {
            InitializeComponent();
            DatePicker.SelectedDate = DateTime.Now;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(AmountTextBox.Text, out var amount))
            {
                MessageBox.Show("Некорректная сумма", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewTransaction = new Transaction
            {
                Type = ((ComboBoxItem)TypeComboBox.SelectedItem).Content.ToString(),
                Amount = amount,
                CategoryId = int.Parse(CategoryTextBox.Text),
                AccountId = int.Parse(AccountTextBox.Text),
                Date = DatePicker.SelectedDate ?? DateTime.Now,
                Note = NoteTextBox.Text
            };

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
