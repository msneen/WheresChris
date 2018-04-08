using System;
using System.Threading.Tasks;
using StayTogether.Helpers;
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
        private static readonly Interval InitializeInterval = new Interval();

        public MainPage()
        {
            InitializeComponent();
            InitializeInterval.SetInterval(InitializePage, 1000);
        }

        private void InitializePage()
        {
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
            Device.BeginInvokeOnMainThread(async () =>
            {
                PhoneNumber.Text = SettingsHelper.GetPhoneNumber();
                if(string.IsNullOrWhiteSpace(PhoneNumber.Text))
                {

                    var phoneNumber = await SettingsHelper.GetPhoneNumberFromService();

                    PhoneNumber.Text = phoneNumber;
                }
                Nickname.Text = SettingsHelper.GetNickname();

                DisableIfValid(PhoneNumber);
                DisableIfValid(Nickname);
                if(PhoneNumber.IsEnabled == false && Nickname.IsEnabled == false)
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

            });
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
            Device.BeginInvokeOnMainThread(() =>
            {
                App.SetCurrentTab("Invite");
            });
        }

        public void JoinGroup(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                App.SetCurrentTab("Join");
            });
        }

        private void SaveButton_OnClicked(object sender, EventArgs e)
        {
            TrySavePhoneNumber();
            if (AskForPhoneNumber()) return;
            TrySaveNickname();
            if ( AskForNickname()) return;
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
            if(!Nickname.IsEnabled || string.IsNullOrWhiteSpace(Nickname.Text)) return;

            SettingsHelper.SaveNickname(Nickname.Text);
            Nickname.IsEnabled = false;
        }

        private bool AskForNickname()
        {
            if(!string.IsNullOrWhiteSpace(Nickname.Text)) return false;

            var title = "Nickname";
            var description = "Please enter your Nickname";
            ToastHelper.Display(title, description);

            Nickname.Focus();
            return true;
        }

        private bool AskForPhoneNumber()
        {
            if(!string.IsNullOrWhiteSpace(PhoneNumber.Text)) return false;

            var title = "Phone Number";
            var description = "Please enter your PhoneNumber";
            ToastHelper.Display(title,description);

            PhoneNumber.Focus();
            return true;
        }

        private async Task SendLastInvitmtion(object sender, EventArgs e)
        {
            await GroupActionsHelper.StartGroup(Invitation.Members, Invitation.UserPhoneNumber, Invitation.ExpirationHours);
        }
    }
}
