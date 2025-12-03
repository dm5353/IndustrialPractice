using MyFinance.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyFinance.Views
{
    public partial class AddCategoryWindow : Window
    {
        public Category NewCategory { get; private set; }

        public AddCategoryWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите название категории");
                return;
            }

            var color = ColorPicker.SelectedColor ?? Colors.Black;

            NewCategory = new Category
            {
                Name = NameBox.Text.Trim(),
                Type = ((ComboBoxItem)TypeBox.SelectedItem).Content.ToString() == "Доход" ? "Income" : "Expense",
                Color = color.ToString()
            };

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}