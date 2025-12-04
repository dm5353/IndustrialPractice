using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyFinance.Models;
using MyFinance.Services;
using MyFinance.Views;
using System.Collections.ObjectModel;

namespace MyFinance.ViewModels
{
    public partial class ManageAccountsViewModel : ObservableObject
    {
        private readonly AppDbContext _db = new AppDbContext();

        public ObservableCollection<Account> Accounts { get; set; }

        [ObservableProperty]
        private Account selectedAccount;

        public ManageAccountsViewModel()
        {
            Accounts = new ObservableCollection<Account>(_db.Accounts.ToList());
        }

        [RelayCommand]
        private void AddAccount()
        {
            var win = new AddAccountWindow();

            if (win.ShowDialog() == true)
            {
                var newAcc = win.NewAccount;

                _db.Accounts.Add(newAcc);
                _db.SaveChanges();

                Accounts.Add(newAcc);
                (App.Current.MainWindow.DataContext as MainViewModel)?.LoadTransactions();
            }
        }


        [RelayCommand]
        private void DeleteAccount()
        {
            if (SelectedAccount == null) return;

            _db.Accounts.Remove(SelectedAccount);
            _db.SaveChanges();

            Accounts.Remove(SelectedAccount);
            (App.Current.MainWindow.DataContext as MainViewModel)?.LoadTransactions();
        }
    }
}