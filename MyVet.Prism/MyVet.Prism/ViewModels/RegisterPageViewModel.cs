using MyVet.Common.Helpers;
using MyVet.Common.Models;
using MyVet.Common.Services;
using MyVet.Prism.Helpers;
using Prism.Commands;
using Prism.Navigation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace MyVet.Prism.ViewModels
{
    public class RegisterPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IApiService _apiService;
        private readonly IGeolocatorService _geolocatorService;
        private bool _isRunning;
        private bool _isEnabled;
        private DelegateCommand _registerCommand;
        private Position _position;

        public RegisterPageViewModel(
            INavigationService navigationService,
            IApiService apiService,
            IGeolocatorService geolocatorService) : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _geolocatorService = geolocatorService;
            Title = "Register new user";
            IsEnabled = true;
        }

        public DelegateCommand RegisterCommand => _registerCommand ?? (_registerCommand = new DelegateCommand(Register));

        public string Document { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Password { get; set; }

        public string PasswordConfirm { get; set; }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private async void Register()
        {
            var isValid = await ValidateData();
            if (!isValid)
            {
                return;
            }

            IsRunning = true;
            IsEnabled = false;

            var request = new UserRequest
            {
                Address = Address,
                Document = Document,
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                Password = Password,
                Phone = Phone,
                Latitude = _position.Latitude,
                Longitude = _position.Longitude
            };

            var url = App.Current.Resources["UrlAPI"].ToString();
            var response = await _apiService.RegisterUserAsync(
                url,
                "api",
                "/Account",
                request);

            IsRunning = false;
            IsEnabled = true;

            if (!response.IsSuccess)
            {
                await App.Current.MainPage.DisplayAlert(
                    "Error",
                    response.Message,
                    "Accept");
                return;
            }

            await App.Current.MainPage.DisplayAlert(
                "Ok",
                response.Message,
                "Accept");
            await _navigationService.GoBackAsync();
        }

        private async Task<bool> ValidateData()
        {
            if (string.IsNullOrEmpty(Document))
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter a document.", "Accept");
                return false;
            }

            if (string.IsNullOrEmpty(FirstName))
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter a firstname.", "Accept");
                return false;
            }

            if (string.IsNullOrEmpty(LastName))
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter a lastname.", "Accept");
                return false;
            }

            var isValidAddress = await ValidateAddressAsync();
            if (!isValidAddress)
            {
                return false;
            }

            if (string.IsNullOrEmpty(Email) || !RegexHelper.IsValidEmail(Email))
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter a valid email.", "Accept");
                return false;
            }

            if (string.IsNullOrEmpty(Phone))
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter a phone.", "Accept");
                return false;
            }

            if (string.IsNullOrEmpty(Password) || Password.Length < 6)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter a password at least 6 character.", "Accept");
                return false;
            }

            if (string.IsNullOrEmpty(PasswordConfirm))
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter a password confirm.", "Accept");
                return false;
            }

            if (!Password.Equals(PasswordConfirm))
            {
                await App.Current.MainPage.DisplayAlert("Error", "The password and confirm does not match.", "Accept");
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateAddressAsync()
        {
            if (string.IsNullOrEmpty(Address))
            {
                await App.Current.MainPage.DisplayAlert(
                    "Error", "You must enter a valid address.", "Accept");
                return false;
            }

            var geoCoder = new Geocoder();
            var locations = await geoCoder.GetPositionsForAddressAsync(Address);
            var locationList = locations.ToList();
            if (locationList.Count == 0)
            {
                var response = await App.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    "Address not found, do you want to use your current location like your address.",
                    Languages.Yes,
                    Languages.No);
                if (response)
                {
                    await _geolocatorService.GetLocationAsync();
                    if (_geolocatorService.Latitude != 0 && _geolocatorService.Longitude != 0)
                    {
                        _position = new Position(
                            _geolocatorService.Latitude,
                            _geolocatorService.Longitude);

                        var list = await geoCoder.GetAddressesForPositionAsync(_position);
                        Address = list.FirstOrDefault();
                        return true;
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert(
                            Languages.Error,
                            "Not Location Available",
                            Languages.Accept);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (locationList.Count == 1)
            {
                _position = locationList.FirstOrDefault();
                return true;
            }

            if (locationList.Count > 1)
            {
                var addresses = new List<Address>();
                var names = new List<string>();
                foreach (var location in locationList)
                {
                    var list = await geoCoder.GetAddressesForPositionAsync(location);
                    names.AddRange(list);
                    foreach (var item in list)
                    {
                        addresses.Add(new Address
                        {
                            Name = item,
                            Latitude = location.Latitude,
                            Longitude = location.Longitude
                        });
                    }
                }

                var source = await App.Current.MainPage.DisplayActionSheet(
                    "Select An Adrress...",
                    "Cancel",
                    null,
                    names.ToArray());
                if (source == "Cancel")
                {
                    return false;
                }

                Address = source;
                var address = addresses.FirstOrDefault(a => a.Name == source);
                _position = new Position(address.Latitude, address.Longitude);
            }

            return true;
        }
    }
}
