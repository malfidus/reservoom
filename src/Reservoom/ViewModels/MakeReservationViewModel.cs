using CommunityToolkit.Mvvm.ComponentModel;
using Reservoom.Commands;
using Reservoom.Models;
using Reservoom.Services;
using Reservoom.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Reservoom.ViewModels
{
    public partial class MakeReservationViewModel : ObservableObject, INotifyDataErrorInfo
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCreateReservation))]
        private string _username;

        partial void OnUsernameChanging(string value)
        {
            ClearErrors(nameof(Username));

            if (string.IsNullOrEmpty(value))
            {
                AddError("Username cannot be empty.", nameof(Username));
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCreateReservation))]
        private int _floorNumber = 1;

        partial void OnFloorNumberChanging(int value)
        {
            ClearErrors(nameof(FloorNumber));

            if (value > 0)
            {
                AddError("Floor number must be greater than zero.", nameof(FloorNumber));
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCreateReservation))]
        private int _roomNumber;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCreateReservation))]
        private DateTime _startDate = new DateTime(2021, 1, 1);

        partial void OnStartDateChanging(DateTime value)
        {
            ClearErrors(nameof(StartDate));
            ClearErrors(nameof(EndDate));

            if (!(value < EndDate))
            {
                AddError("The start date cannot be after the end date.", nameof(StartDate));
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCreateReservation))]
        private DateTime _endDate = new DateTime(2021, 1, 8);
        partial void OnEndDateChanging(DateTime value)
        {
            ClearErrors(nameof(StartDate));
            ClearErrors(nameof(EndDate));

            if (!(StartDate < value))
            {
                AddError("The end date cannot be before the start date.", nameof(EndDate));
            }
        }

        public bool CanCreateReservation =>
            HasUsername &&
            HasFloorNumberGreaterThanZero &&
            HasStartDateBeforeEndDate &&
            !HasErrors;

        private bool HasUsername => !string.IsNullOrEmpty(Username);
        private bool HasFloorNumberGreaterThanZero => FloorNumber > 0;
        private bool HasStartDateBeforeEndDate => StartDate < EndDate;

        private string _submitErrorMessage;
        public string SubmitErrorMessage
        {
            get
            {
                return _submitErrorMessage;
            }
            set
            {
                _submitErrorMessage = value;
                OnPropertyChanged(nameof(SubmitErrorMessage));

                OnPropertyChanged(nameof(HasSubmitErrorMessage));
            }
        }

        public bool HasSubmitErrorMessage => !string.IsNullOrEmpty(SubmitErrorMessage);

        private bool _isSubmitting;
        public bool IsSubmitting
        {
            get
            {
                return _isSubmitting;
            }
            set
            {
                _isSubmitting = value;
                OnPropertyChanged(nameof(IsSubmitting));
            }
        }

        public ICommand SubmitCommand { get; }
        public ICommand CancelCommand { get; }

        private readonly Dictionary<string, List<string>> _propertyNameToErrorsDictionary;

        public bool HasErrors => _propertyNameToErrorsDictionary.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public MakeReservationViewModel(HotelStore hotelStore, NavigationService<ReservationListingViewModel> reservationViewNavigationService)
        {
            SubmitCommand = new MakeReservationCommand(this, hotelStore, reservationViewNavigationService);
            CancelCommand = new NavigateCommand<ReservationListingViewModel>(reservationViewNavigationService);

            _propertyNameToErrorsDictionary = new Dictionary<string, List<string>>();
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return _propertyNameToErrorsDictionary.GetValueOrDefault(propertyName, new List<string>());
        }

        private void AddError(string errorMessage, string propertyName)
        {
            if (!_propertyNameToErrorsDictionary.ContainsKey(propertyName))
            {
                _propertyNameToErrorsDictionary.Add(propertyName, new List<string>());
            }

            _propertyNameToErrorsDictionary[propertyName].Add(errorMessage);

            OnErrorsChanged(propertyName);
        }

        private void ClearErrors(string propertyName)
        {
            _propertyNameToErrorsDictionary.Remove(propertyName);

            OnErrorsChanged(propertyName);
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
