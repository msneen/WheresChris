using System;
using System.Linq;
using System.Timers;
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
        private Timer _timer;

        public MainPage()
        {
            InitializeComponent();
        }

        private void TrackLocationSent()
        {
            _locationSender = LocationSenderFactory.GetLocationSender();
            _locationSender.OnLocationSent += (sender, args) =>
            {
                //MWS:  Change the text color for 2 seconds each time a message is sent
                //This is for debugging
                TitleLabel.TextColor = Color.Blue;
                _timer = new Timer();
                _timer.Elapsed += (o, eventArgs) =>
                {
                    _timer.Stop();
                    TitleLabel.TextColor = Color.Black;
                };
                _timer.Start();
            };
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

        private void DebugSent_OnClicked(object sender, EventArgs e)
        {
            TrackLocationSent();
        }
    }
}
