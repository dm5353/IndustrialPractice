using System.ComponentModel;

namespace MyFinance.ViewModels
{
    public class CategorySelectItem : INotifyPropertyChanged
    {
        public string Name { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));

                // вызываем обновление графиков
                CategoryChanged?.Invoke();
            }
        }

        // статический callback для ViewModel
        public static Action CategoryChanged;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}