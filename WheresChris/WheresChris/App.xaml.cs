using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Mobile.Analytics;
using StayTogether;
using StayTogether.Helpers;
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
        private LocationSender _locationSender;

        public App()
        {
            InitializeComponent();

            SetMainPage().Wait();

            StartLocationSenderAsync();
        }

        private static readonly Interval PermissionRequest = new Interval();
        private static readonly Interval AddPagesInterval = new Interval();
        private static int _permisionRequestIntervalTime = 5000;
        private static int _addPagesIntervalTime = 5000;

        private async void StartLocationSenderAsync()
        {
            _locationSender = await LocationSender.GetInstanceAsync();
        }

        public static async Task SetMainPage()
        {
            try
            {
                _mainTabbedPage = new TabbedPage();

                AddPage(new MainPage(), "Main");

                var alreadyHasPermissions = await PermissionHelper.HasNecessaryPermissions();
                if (alreadyHasPermissions)
                {
                    _permisionRequestIntervalTime = 250;
                    _addPagesIntervalTime = 250;
                }

                PermissionRequest.SetInterval(InsertPagesNeedingPermissions, _permisionRequestIntervalTime);
               
                AddPage(new AboutPage(), "About");
                Current.MainPage = _mainTabbedPage;
            }
            catch (System.Exception ex)
            {
                Analytics.TrackEvent("Permissions", new Dictionary<string, string>
                {
                    {"App.xaml.cs_SetMainPage_Error" , ex.Message}
                });
            }
        }

        private static void InsertPagesNeedingPermissions()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                AddPagesInterval.SetInterval(InsertPages, _addPagesIntervalTime);
            });
        }

        private static void InsertPages()
        {
            InsertPageBeforeAbout(new InvitePage(), "Invite");
            InsertPageBeforeAbout(new JoinPage(), "Join");
            InsertPageBeforeAbout(new MapPage(), "Map");
            InsertPageBeforeAbout(new ChatPage(), "Chat");
        }

        private static void AddPage(Page page, string title)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var existingNavigationPage = GetPage(title) as NavigationPage;
                if (existingNavigationPage != null) return;

                var navigationPage = new NavigationPage(page)
                {
                    Title = title,
                    Icon = Device.OnPlatform("tab_feed.png", null, null),
                };
                _mainTabbedPage.Children.Add(navigationPage);
            });
        }

        private static void InsertPageBeforeAbout(Page page, string title)
        {
            Device.BeginInvokeOnMainThread(() => {
                var existingNavigationPage = GetPage(title) as NavigationPage;
                if (existingNavigationPage != null) return;

                var navigationPage = new NavigationPage(page)
                {
                    Title = title,
                    Icon = Device.OnPlatform("tab_feed.png", null, null),
                };
                var lastIndex = _mainTabbedPage.Children.Count - 1;

                _mainTabbedPage.Children.Insert(lastIndex, navigationPage);
            });
        }

        public static Page GetCurrentTab()
        {
            var tabbedPage = Current.MainPage as TabbedPage;
            return tabbedPage?.CurrentPage;
        }

        public static TabbedPage GetMainTab()
        {
            return Current.MainPage as TabbedPage;
        }

        public static void SetCurrentTab(string title)
        {
            GetMainTab().CurrentPage = GetPage(title);
        }

        public static Page GetPage(string title)
        {
            if (GetMainTab()?.Children.Count <= 0) return null;

            var requestedPage = GetMainTab()?.Children.FirstOrDefault(x => x.Title.EndsWith(title));
            if (requestedPage == null) return null;

            var index = GetMainTab().Children.IndexOf(requestedPage);
            return index <= -1 ? null : GetMainTab().Children[index];
        }

        //protected override void OnStart()
        //{
         
        //}

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
