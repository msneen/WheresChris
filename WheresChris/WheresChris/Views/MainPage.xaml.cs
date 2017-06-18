using System;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Toasts;
using WheresChris.Helpers;
using WheresChris.Messaging;
using Xamarin.Forms;


namespace WheresChris.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        public LocationSentEvent LocationSentEvent;

        public MainPage()
        {
            InitializeComponent();
            SaveButton.Clicked += SaveButton_OnClicked;
            InitializeMessagingCenterSubscriptions();
            CheckLocationServicesEnabled();
            InitializePhoneAndNickname();
        }

        private void InitializePhoneAndNickname()
        {
            PhoneNumber.Text = SettingsHelper.GetPhoneNumber();
            Nickname.Text = SettingsHelper.GetNickname();

            DisableIfValid(PhoneNumber);
            DisableIfValid(Nickname);
            if (PhoneNumber.IsEnabled == false && Nickname.IsEnabled == false)
            {
                SaveButton.IsVisible = false;
                InviteButton.IsEnabled = true;
                JoinButton.IsEnabled = true;
            }
            else
            {
                InviteButton.IsEnabled = false;
                JoinButton.IsEnabled = false;
            }
        }

        private static void DisableIfValid(Entry textbox)
        {
            if (string.IsNullOrWhiteSpace(textbox?.Text)) return;
            textbox.IsEnabled = false;
        }

        private void InitializeMessagingCenterSubscriptions()
        {
            LocationSentEvent = new LocationSentEvent();
            LocationSentEvent.OnLocationSentMsg +=
            (sender, args) =>
            {
                TitleLabel.TextColor = TitleLabel.TextColor == Color.Blue ? Color.Black : Color.Blue;
            };
        }


        protected override void OnAppearing()
        {
        }

        private void CheckLocationServicesEnabled()
        {
            if (!(CrossGeolocator.Current.IsGeolocationAvailable && CrossGeolocator.Current.IsGeolocationEnabled))
            {
                MessageLabel.Text = "Please enable your Location in phone settings!";
            }
        }

        public void StartGroup(object sender, EventArgs e)
        {
            App.SetCurrentTab("Invite");
        }

        public void JoinGroup(object sender, EventArgs e)
        {
            App.SetCurrentTab("Join");

        }

        private async void SaveButton_OnClicked(object sender, EventArgs e)
        {
            TrySavePhoneNumber();
            if (await AskForPhoneNumber()) return;
            TrySaveNickname();
            if (await AskForNickname()) return;
            SaveButton.IsVisible = false;
            InviteButton.IsEnabled = true;
            JoinButton.IsEnabled = true;
        }

        private void TrySavePhoneNumber()
        {
            if (PhoneNumber.IsEnabled && !string.IsNullOrWhiteSpace(PhoneNumber.Text))
            {
                SettingsHelper.SavePhoneNumber(PhoneNumber.Text);
                PhoneNumber.IsEnabled = false;
            }
        }

        private void TrySaveNickname()
        {
            if (Nickname.IsEnabled && !string.IsNullOrWhiteSpace(Nickname.Text))
            {
                SettingsHelper.SaveNickname(Nickname.Text);
                Nickname.IsEnabled = false;
            }
        }

        private async Task<bool> AskForNickname()
        {
            if (string.IsNullOrWhiteSpace(Nickname.Text))
            {
                await (new ToastNotification()).Notify(new NotificationOptions
                {
                    Title = "Nickname",
                    Description = "Please enter your Nickname"
                });
                Nickname.Focus();
                return true;
            }
            return false;
        }

        private async Task<bool> AskForPhoneNumber()
        {
            if (string.IsNullOrWhiteSpace(PhoneNumber.Text))
            {
                await (new ToastNotification()).Notify(new NotificationOptions
                {
                    Title = "Phone Number",
                    Description = "Please enter your PhoneNumber"
                });
                PhoneNumber.Focus();
                return true;
            }
            return false;
        }
    }
}
