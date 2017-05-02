using System;
using System.Linq;
using StayTogether;
using Xamarin.Forms;
#if __ANDROID__
using StayTogether.Droid.Services;
#endif
#if __IOS__
using WheresChris.iOS;
#endif

namespace WheresChris.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private LocationSender _locationSender;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            MessagingCenter.Subscribe<LocationSender, string>(this, LocationSender.LocationSentMsg, (sender, arg) =>
            {
                TitleLabel.TextColor = TitleLabel.TextColor == Color.Blue ? Color.Black : Color.Blue;
            });
        }

        public void StartGroup(object sender, EventArgs e)
        {
            NavigateToPage("Invite");
        }

        public void JoinGroup(object sender, EventArgs e)
        {
            NavigateToPage("Join");
        }

        private void NavigateToPage(string title)
        {
            var masterPage = Parent.Parent as TabbedPage;
            var invitePage = masterPage?.Children.FirstOrDefault(x => x.Title == title);
            if (invitePage == null) return;

            var index = masterPage.Children.IndexOf(invitePage);
            if (index > -1)
            {
                masterPage.CurrentPage = masterPage.Children[index];
            }
        }
    }
}
