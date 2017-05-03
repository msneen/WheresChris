﻿using WheresChris.Views;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace WheresChris
{
	public partial class App : Application
	{
        public App()
		{
			InitializeComponent();

			SetMainPage();
		}

		public static void SetMainPage()
		{
            Current.MainPage = new TabbedPage
            {
                Children =
                {
                    new NavigationPage(new MainPage())
                    {
                        Title="Main",
                        Icon = Device.OnPlatform<string>("tab_feed.png",null,null)
                    },
                    //needs phone and contacts permission
                    //new NavigationPage(new InvitePage())
                    //{
                    //    Title = "Invite",
                    //    Icon = Device.OnPlatform<string>("tab_feed.png",null,null)
                    //},
                    //new NavigationPage(new JoinPage())
                    //{
                    //    Title = "Join",
                    //    Icon = Device.OnPlatform<string>("tab_feed.png",null,null)
                    //},
                    //needs location permission
                    //new NavigationPage(new MapPage())
                    //{
                    //    Title = "Map",
                    //    Icon = Device.OnPlatform<string>("tab_feed.png",null,null)
                    //},
                    new NavigationPage(new AboutPage())
                    {
                        Title = "About",
                        Icon = Device.OnPlatform<string>("tab_about.png",null,null)
                    },
                }
            };
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
