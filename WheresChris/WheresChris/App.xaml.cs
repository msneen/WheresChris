﻿using System.Linq;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using Microsoft.Azure.Mobile.Distribute;
using WheresChris.Helpers;
using WheresChris.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Device = Xamarin.Forms.Device;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace WheresChris
{
    public partial class App : Application
    {
        private static TabbedPage _mainTabbedPage;

        public App()
        {
            InitializeComponent();

            SetMainPage();     
        }

        public static Page GetCurrentTab()
        {
            var tabbedPage = Current.MainPage as TabbedPage;
            return tabbedPage?.CurrentPage;
        }

        public static void SetCurrentTab(string title)
        {
            var tabbedPage = Current.MainPage as TabbedPage;
            var invitePage = tabbedPage?.Children.FirstOrDefault(x => x.Title == title);
            if (invitePage == null) return;

            var index = tabbedPage.Children.IndexOf(invitePage);
            if (index > -1)
            {
                tabbedPage.CurrentPage = tabbedPage.Children[index];
            }
        }
       

        public static void SetMainPage()
        {
            _mainTabbedPage = new TabbedPage();

            AddPage(new MainPage(), "Main");

            if (PermissionHelper.HasNecessaryPermissions())
            {
                AddPage(new InvitePage(), "Invite");
                AddPage(new JoinPage(), "Join");
                AddPage(new MapPage(), "Map");
            }
            AddPage(new AboutPage(), "About");           
            Current.MainPage = _mainTabbedPage;
        }

        private static void AddPage(Page page, string title)
        {
            _mainTabbedPage.Children.Add(new NavigationPage(page)
            {
                Title = title,
                Icon = Device.OnPlatform<string>("tab_feed.png", null, null)
            });
        }

        protected override void OnStart()
        {
            MobileCenter.Start("ios=2cd11ff1-c5b1-47d8-ac96-9fa5b74a47bd;android=14162ca6-0c56-4822-9d95-f265b524bd98;", typeof(Analytics), typeof(Crashes), typeof(Distribute));
        }

        //protected override void OnSleep()
        //{
        //    // Handle when your app sleeps
        //}

        //protected override void OnResume()
        //{
        //    // Handle when your app resumes
        //}
    }
}
