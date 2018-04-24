using System;
using System.Threading.Tasks;
using Authy.Net;
using StayTogether;
using StayTogether.Helpers;
using StayTogether.Models;
using WheresChris.Helpers;
using WheresChris.Models;
using WheresChris.Views.AuthenticatePhone;
using Xamarin.Forms;


namespace WheresChris.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private Invitation _invitation;
        private static readonly Interval InitializeInterval = new Interval();

        public MainPage()
        {
            InitializeComponent();
            InitializeMessagingCenter();
            InitializeInterval.SetInterval(InitializePage, 1000);
        }

        private void InitializeMessagingCenter()
        {
            MessagingCenter.Subscribe<MessagingCenterSender>(this, LocationSender.InitializeMainPageMsg,
                (sender) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        InitializeInterval.SetInterval(InitializePhoneAndNickname,1000);
                    });                    
                });
        }

        private void InitializePage()
        {
            InitializePhoneAndNicknameWithValidation();
            LoadLastInvitation();
        }

        private void LoadLastInvitation()
        {
            _invitation = InvitationHelper.LoadInvitation();
            if(_invitation?.Members != null && _invitation.Members.Count > 0)
            {
                LastInviteButton.IsVisible = true;
            }
            else
            {
                LastInviteButton.IsVisible = false;
            }
        }

        private void InitializePhoneAndNicknameWithValidation()
        {
            InitializePhoneAndNickname();

            //Device.BeginInvokeOnMainThread(async () =>
            //{
            //    await AuthyValidateUser();
            //});
        }

        private void InitializePhoneAndNickname()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                InviteButton.IsEnabled = false;
                JoinButton.IsEnabled = false;

                var phoneNumber = SettingsHelper.GetPhoneNumber();

                PhoneNumber.Text = phoneNumber;

                Nickname.Text = SettingsHelper.GetNickname();

                if(PermissionHelper.IsAuthyAuthenticated() && phoneNumber.IsValidPhoneNumber())
                {
                    InviteButton.IsEnabled = true;
                    JoinButton.IsEnabled = true;
                }  
            });
        
        }


        public void StartGroup(object sender, EventArgs e)
        {
            App.SetCurrentTab("Invite");
        }

        public void JoinGroup(object sender, EventArgs e)
        {
            App.SetCurrentTab("Join");
        }

        //public async Task AuthyValidateUser()
        //{
        //    var authenticatePhonePage = new AuthenticatePhonePage();
        //    await Navigation.PushModalAsync(authenticatePhonePage);
        //}

        private async Task SendLastInvitation(object sender, EventArgs e)
        {
            await GroupActionsHelper.StartGroup(_invitation.Members, _invitation.UserPhoneNumber, _invitation.ExpirationHours);
        }
    }
}
