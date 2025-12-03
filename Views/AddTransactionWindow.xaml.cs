using MyFinance.Models;
using MyFinance.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MyFinance.Views
{
    public partial class AddTransactionWindow : Window
    {
        private readonly AppDbContext _db;

        public Transaction NewTransaction { get; private set; }

        public AddTransactionWindow(Transaction transaction)
        {
            InitializeComponent();

            _db = new AppDbContext();

            // Заполняем ComboBox
            CategoryComboBox.ItemsSource = _db.Categories.ToList();
            CategoryComboBox.DisplayMemberPath = "Name";
            CategoryComboBox.SelectedValuePath = "Id";

            AccountComboBox.ItemsSource = _db.Accounts.ToList();
            AccountComboBox.DisplayMemberPath = "Name";
            AccountComboBox.SelectedValuePath = "Id";

            // Устанавливаем значения из переданной транзакции
            NameTextBox.Text = transaction.Name;
            AmountTextBox.Text = transaction.Amount.ToString();
            NoteTextBox.Text = transaction.Note;

            TypeComboBox.SelectedItem = transaction.Type == "Income" ?
                TypeComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (string)i.Content == "Доход") :
                TypeComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (string)i.Content == "Расход");

            CategoryComboBox.SelectedValue = transaction.CategoryId;
            AccountComboBox.SelectedValue = transaction.AccountId;

            DatePicker.Text = transaction.Date.ToString("dd.MM.yyyy");
            TimeTextBox.Text = transaction.Date.ToString("HH:mm");
        }

        public AddTransactionWindow()
        {
            InitializeComponent();

            _db = new AppDbContext();
            DatePicker.SelectedDate = DateTime.Now.Date;
            TimeTextBox.Text = DateTime.Now.ToString("HH:mm");

            if (!_db.Accounts.Any())
            {
                _db.Accounts.Add(new Account { Name = "Карта", Balance = 0 });
                _db.Accounts.Add(new Account { Name = "Наличные", Balance = 0 });
            }

            if (!_db.Categories.Any())
            {
                // Доходы
                _db.Categories.Add(new Category { Name = "Зарплата", Type = "Income" });
                _db.Categories.Add(new Category { Name = "Подарок", Type = "Income" });
                _db.Categories.Add(new Category { Name = "Проценты по вкладу", Type = "Income" });
                _db.Categories.Add(new Category { Name = "Кешбек", Type = "Income" });

                // Расходы
                _db.Categories.Add(new Category { Name = "Еда", Type = "Expense" });
                _db.Categories.Add(new Category { Name = "Транспорт", Type = "Expense" });
                _db.Categories.Add(new Category { Name = "Развлечения", Type = "Expense" });
                _db.Categories.Add(new Category { Name = "Одежда", Type = "Expense" });
                _db.Categories.Add(new Category { Name = "Медицина", Type = "Expense" });
                _db.Categories.Add(new Category { Name = "Коммунальные платежи", Type = "Expense" });
                _db.Categories.Add(new Category { Name = "Образование", Type = "Expense" });
            }

            _db.SaveChanges();

            // Заполняем ComboBox категории и счета из БД
            CategoryComboBox.ItemsSource = _db.Categories.ToList();
            AccountComboBox.ItemsSource = _db.Accounts.ToList();
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedType = (TypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(selectedType))
            {
                // Тип не выбран — показываем все категории
                CategoryComboBox.ItemsSource = _db.Categories.ToList();
            }
            else
            {
                selectedType = selectedType == "Доход" ? "Income" : "Expense";
                // Фильтруем категории по выбранному типу (Income/Expense)
                CategoryComboBox.ItemsSource = _db.Categories
                    .Where(c => c.Type == selectedType)
                    .ToList();
            }

            CategoryComboBox.DisplayMemberPath = "Name";
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is Category selectedCategory)
            {
                string typeName = selectedCategory.Type == "Income" ? "Доход" : "Расход";

                foreach (ComboBoxItem comboItem in TypeComboBox.Items)
                {
                    if ((string)comboItem.Content == typeName)
                    {
                        TypeComboBox.SelectedItem = comboItem;
                        break;
                    }
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (TypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип транзакции", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (AccountComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите счёт", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(AmountTextBox.Text, out var amount))
            {
                MessageBox.Show("Некорректная сумма", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var date = DatePicker.SelectedDate ?? DateTime.Now;

            int hours = 0, minutes = 0;
            if (!string.IsNullOrEmpty(TimeTextBox.Text))
            {
                var timeParts = TimeTextBox.Text.Split(':');
                if (timeParts.Length == 2)
                {
                    int.TryParse(timeParts[0], out hours);
                    int.TryParse(timeParts[1], out minutes);
                }
            }

            NewTransaction = new Transaction
            {
                Type = ((ComboBoxItem)TypeComboBox.SelectedItem).Content.ToString() == "Доход" ? "Income" : "Expense",
                Name = string.IsNullOrWhiteSpace(NameTextBox.Text) ? "Операция" : NameTextBox.Text,
                CategoryId = CategoryComboBox.SelectedValue != null ? (int)CategoryComboBox.SelectedValue : 0,
                AccountId = AccountComboBox.SelectedValue != null ? (int)AccountComboBox.SelectedValue : 0,
                Date = new DateTime(date.Year, date.Month, date.Day, hours, minutes, 0),
                Amount = amount,
                Note = NoteTextBox.Text
            };

            // обновление баланса
            var account = _db.Accounts.FirstOrDefault(a => a.Id == NewTransaction.AccountId);
            if (account != null)
            {
                if (NewTransaction.Type == "Expense" && account.Balance < NewTransaction.Amount)
                {
                    MessageBox.Show("Недостаточно средств на счёте", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (NewTransaction.Type == "Income")
                    account.Balance += NewTransaction.Amount;
                else
                    account.Balance -= NewTransaction.Amount;

                _db.SaveChanges();
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
