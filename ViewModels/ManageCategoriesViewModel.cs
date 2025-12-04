using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyFinance.Models;
using MyFinance.Services;
using MyFinance.Views;
using System.Collections.ObjectModel;

namespace MyFinance.ViewModels
{
    public partial class ManageCategoriesViewModel : ObservableObject
    {
        private readonly AppDbContext _db = new AppDbContext();

        public ObservableCollection<Category> Categories { get; set; }

        [ObservableProperty]
        private Category selectedCategory;

        public ManageCategoriesViewModel()
        {
            Categories = new ObservableCollection<Category>(_db.Categories.ToList());
        }

        [RelayCommand]
        private void AddCategory()
        {
            var win = new AddCategoryWindow();

            if (win.ShowDialog() == true)
            {
                var newCat = win.NewCategory;

                _db.Categories.Add(newCat);
                _db.SaveChanges();

                Categories.Add(newCat);
            }
        }


        [RelayCommand]
        private void DeleteCategory()
        {
            if (SelectedCategory == null) return;

            // Получаем все транзакции с этой категорией
            var relatedTransactions = _db.Transactions
                .Where(t => t.CategoryId == SelectedCategory.Id)
                .ToList();

            foreach (var tx in relatedTransactions)
            {
                // Корректируем баланс счёта
                var account = _db.Accounts.FirstOrDefault(a => a.Id == tx.AccountId);
                if (account != null)
                {
                    if (tx.Type == "Income")
                        account.Balance -= tx.Amount;
                    else
                        account.Balance += tx.Amount;
                }

                _db.Transactions.Remove(tx);
            }

            _db.Categories.Remove(SelectedCategory);
            _db.SaveChanges();

            Categories.Remove(SelectedCategory);

            // Обновляем главную страницу
            (App.Current.MainWindow.DataContext as MainViewModel)?.LoadTransactions();
        }
    }
}