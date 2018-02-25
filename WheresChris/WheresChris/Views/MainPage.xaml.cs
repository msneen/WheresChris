using System;
using System.Threading.Tasks;
using Plugin.Toasts;
using StayTogether;
using WheresChris.Helpers;
using WheresChris.Models;
using Xamarin.Forms;


namespace WheresChris.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private Invitation Invitation;
        public MainPage()
        {
            InitializeComponent();
            SaveButton.Clicked += SaveButton_OnClicked;
            InitializeMessagingCenterSubscriptions();
            InitializePhoneAndNickname();
            Invitation = InvitationHelper.LoadInvitation();
            if (Invitation?.Members != null && Invitation.Members.Count > 0)
            {
                LastInviteButton.IsVisible = true;
            }
            else
            {
                LastInviteButton.IsVisible = false;
            }
        }

        private void InitializePhoneAndNickname()
        {
            PhoneNumber.Text = SettingsHelper.GetPhoneNumber();
            if (string.IsNullOrWhiteSpace(PhoneNumber.Text))
            {
                Device.BeginInvokeOnMainThread(async ()=>
                {
                    var phoneNumber = await SettingsHelper.GetPhoneNumberFromService();
                    PhoneNumber.Text = phoneNumber;
                });
            }
            

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
            //MessagingCenter.Subscribe<LocationSender>(this, LocationSender.LocationSentMsg, (sender) =>
            //{
            //    Device.BeginInvokeOnMainThread(() =>
            //    {
            //        TitleLabel.TextColor = TitleLabel.TextColor == Color.White ? Color.Yellow : Color.White;
            //    });
            //});
        }

        public void StartGroup(object sender, EventArgs e)
        {
            App.SetCurrentTab("Invite");
        }

        public void JoinGroup(object sender, EventArgs e)
        {
            App.SetCurrentTab("Join");

        }

        private void SaveButton_OnClicked(object sender, EventArgs e)
        {
            TrySavePhoneNumber();
            if (AskForPhoneNumber().ConfigureAwait(true).GetAwaiter().GetResult()) return;
            TrySaveNickname();
            if ( AskForNickname().ConfigureAwait(true).GetAwaiter().GetResult()) return;
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

        private async Task SendLastInvitmtion(object sender, EventArgs e)
        {
            await GroupActionsHelper.StartGroup(Invitation.Members, Invitation.UserPhoneNumber, Invitation.ExpirationHours);
        }
    }
}
