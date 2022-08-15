using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfProblem
{
    public class MainWindowViewModel : ObservableRecipient
    {
        public ObservableCollection<ChildTabViewModel> ChildTabs { get; } = new();

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }
        private int _selectedIndex;

        public ICommand TabChangedCommand
                => new RelayCommand<object>(TabChanged);

        private void TabChanged(object commandParameter)
        {
            TabControl tabItemViewModel = commandParameter as TabControl;
            if ((tabItemViewModel.SelectedItem as ChildTabViewModel).Header == "+")
            {
                ChildTabs.Insert(ChildTabs.Count - 1, new ChildTabViewModel());

                SelectedIndex = ChildTabs.Count - 2;
            }
        }

        public MainWindowViewModel()
        {
            ChildTabs.Add(new ChildTabViewModel());

            ChildTabs.Add(new ChildTabViewModel(true));
        }
    }
}