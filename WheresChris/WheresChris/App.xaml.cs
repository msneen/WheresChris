using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using Microsoft.Azure.Mobile.Distribute;
using Plugin.Permissions.Abstractions;
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

        public App()
        {
            InitializeComponent();

            SetMainPage().Wait();     
        }

        private static readonly Interval InitialContactInterval = new Interval();
        private static readonly Interval PermissionRequest = new Interval();
        private static readonly Interval AddPagesInterval = new Interval();
        private static int _permisionRequestIntervalTime = 5000;
        private static int _addPagesIntervalTime = 15000;
        private static int _initializeContactsIntervalTime = 5000;

        public static async Task SetMainPage()
        {
            try
            {
                _mainTabbedPage = new TabbedPage();

                AddPage(new MainPage(), "Main");

                var alreadyHasPermissions = await PermissionHelper.HasNecessaryPermissions();
                if (alreadyHasPermissions)
                {
                    _permisionRequestIntervalTime = 500;
                    _addPagesIntervalTime = 500;
                }

                PermissionRequest.SetInterval(InsertPagesNeedingPermissions().Wait, _permisionRequestIntervalTime);
                //await InsertPagesNeedingPermissions();

                AddPage(new AboutPage(), "About");
                Current.MainPage = _mainTabbedPage;
            }
            catch (System.Exception ex)
            {
                
            }
        }

        private static async Task InsertPagesNeedingPermissions()
        {
            var hasPermissions = await PermissionHelper.HasNecessaryPermissionsWithRequest();
            if (hasPermissions)
            {
                AddPagesInterval.SetInterval(InsertPages, _addPagesIntervalTime);

            }
            else
            {
                PermissionRequest.SetInterval(InsertPagesNeedingPermissions().Wait, _permisionRequestIntervalTime);
            }
        }

        private static void InsertPages()
        {
            InsertPageBeforeAbout(new InvitePage(), "Invite");
            InsertPageBeforeAbout(new JoinPage(), "Join");
            InsertPageBeforeAbout(new MapPage(), "Map");

            InitialContactInterval.SetInterval(InitializeContacts, _initializeContactsIntervalTime);
        }

        private static void AddPage(Page page, string title)
        {
            var existingNavigationPage = GetPage(title) as NavigationPage;
            if (existingNavigationPage != null) return;

            var navigationPage = new NavigationPage(page)
            {
                Title = title,
                Icon = Device.OnPlatform("tab_feed.png", null, null),
            };
            _mainTabbedPage.Children.Add(navigationPage);
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

        //Call this from AppDelegate or android service
        public static void InitializeContacts()
        {
            Device.BeginInvokeOnMainThread(async ()=>{
                var permissionStatus = await PermissionHelper.RequestContactPermission();
                if (permissionStatus == PermissionStatus.Granted)
                {
                    var inviteNavigationPage = (NavigationPage)GetPage("Invite");
                    if (inviteNavigationPage == null) return;
                    var invitePage = (InvitePage)inviteNavigationPage.CurrentPage;
                    if (invitePage == null) return;
                    await invitePage.InitializeContactsAsync();
                }
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
            var requestedPage = GetMainTab()?.Children.FirstOrDefault(x => x.Title == title);
            if (requestedPage == null) return null;

            var index = GetMainTab().Children.IndexOf(requestedPage);
            return index <= -1 ? null : GetMainTab().Children[index];
        }

        protected override void OnStart()
        {
            //Delete me
            //MobileCenter.Start("ios=2cd11ff1-c5b1-47d8-ac96-9fa5b74a47bd;android=14162ca6-0c56-4822-9d95-f265b524bd98;", typeof(Analytics), typeof(Crashes), typeof(Distribute));
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
