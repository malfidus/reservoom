using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Reservoom.Exceptions;
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
using System.Windows;
using System.Windows.Input;

namespace Reservoom.ViewModels
{
    public partial class MakeReservationViewModel : ObservableRecipient, INotifyDataErrorInfo
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCreateReservation))]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
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
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
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
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
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
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSubmitErrorMessage))]
        private string _submitErrorMessage;

        public bool HasSubmitErrorMessage => !string.IsNullOrEmpty(SubmitErrorMessage);

        [ObservableProperty]
        private bool _isSubmitting;

        [RelayCommand(CanExecute = nameof(CanCreateReservation))]
        private async Task Submit()
        {
            SubmitErrorMessage = string.Empty;
            IsSubmitting = true;

            Reservation reservation = new Reservation(
                new RoomID(FloorNumber, RoomNumber),
                Username,
                StartDate,
                EndDate);

            try
            {
                await _hotelStore.MakeReservation(reservation);

                MessageBox.Show("Successfully reserved room.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                _reservationViewNavigationService.Navigate();
            }
            catch (ReservationConflictException)
            {
                SubmitErrorMessage = "This room is already taken on those dates.";
            }
            catch (InvalidReservationTimeRangeException)
            {
                SubmitErrorMessage = "Start date must be before end date.";
            }
            catch (Exception)
            {
                SubmitErrorMessage = "Failed to make reservation.";
            }

            IsSubmitting = false;
        }

        [RelayCommand]
        private void Cancel()
        {
            _reservationViewNavigationService.Navigate();
        }

        private readonly Dictionary<string, List<string>> _propertyNameToErrorsDictionary;
        private readonly HotelStore _hotelStore;
        private readonly NavigationService<ReservationListingViewModel> _reservationViewNavigationService;

        public bool HasErrors => _propertyNameToErrorsDictionary.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public MakeReservationViewModel(HotelStore hotelStore, NavigationService<ReservationListingViewModel> reservationViewNavigationService)
        {
            _propertyNameToErrorsDictionary = new Dictionary<string, List<string>>();
            _hotelStore = hotelStore;
            _reservationViewNavigationService = reservationViewNavigationService;
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
