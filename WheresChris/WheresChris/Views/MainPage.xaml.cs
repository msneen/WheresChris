﻿using System;
using System.Linq;
using Plugin.Geolocator;
using WheresChris.Messaging;
using Xamarin.Forms;
using XLabs.Platform.Services.Geolocation;
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
        public LocationSentEvent LocationSentEvent;

        public MainPage()
        {
            InitializeComponent();
            InitializeMessagingCenterSubscriptions();
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
            CheckLocationServicesEnabled();
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
