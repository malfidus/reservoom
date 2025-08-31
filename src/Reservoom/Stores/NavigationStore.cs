using CommunityToolkit.Mvvm.ComponentModel;
using Reservoom.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservoom.Stores
{
    public class NavigationStore
    {
        private ObservableRecipient _currentViewModel;  //We can use Recipient instead of Object because it is likely that all page ViewModels will inherit from ObservableRecipient
        public ObservableRecipient CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (_currentViewModel != null)
                {
                    _currentViewModel.IsActive = false; // Deactivate the previous ViewModel
                }

                _currentViewModel = value;

                if (_currentViewModel != null)
                {
                    _currentViewModel.IsActive = true; // Activate the new ViewModel
                }

                OnCurrentViewModelChanged();
            }
        }

        public event Action CurrentViewModelChanged;

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}
