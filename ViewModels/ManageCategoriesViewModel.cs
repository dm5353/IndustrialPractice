using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using MyFinance.Models;
using MyFinance.Services;
using MyFinance.Views;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MyFinance.ViewModels
{
    public partial class ManageCategoriesViewModel : ObservableObject
    {
        private readonly AppDbContext _db = new AppDbContext();

        public ObservableCollection<Category> Categories { get; set; }

        public ManageCategoriesViewModel()
        {
            Categories = new ObservableCollection<Category>(_db.Categories.ToList());
        }
    }
}