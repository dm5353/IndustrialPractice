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

            _db.Categories.Remove(SelectedCategory);
            _db.SaveChanges();

            Categories.Remove(SelectedCategory);
        }
    }
}