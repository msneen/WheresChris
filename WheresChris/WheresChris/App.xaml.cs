using System.Collections.Generic;
using WheresChris.Helpers;
using WheresChris.Views;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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

        //protected override void OnStart()
        //{
        //    // Handle when your app starts
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
