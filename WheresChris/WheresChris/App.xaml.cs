﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using StayTogether;
using StayTogether.Helpers;
using StayTogether.Models;
using WheresChris.Helpers;
using WheresChris.ViewModels;
using WheresChris.Views;
using WheresChris.Views.Popup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xam.Plugin;
using Device = Xamarin.Forms.Device;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace WheresChris
{
    public partial class App : Application
    {
        public static PopupMenu Popup;
        private static TabbedPage _mainTabbedPage;
        private LocationSender _locationSender;

        public App()
        {
            InitializeComponent();

            SetMainPageAsync().ConfigureAwait(true);

            StartLocationSenderAsync().ConfigureAwait(true);

            MessagingCenter.Send(new MessagingCenterSender(), LocationSender.LeaveOrEndGroupMsg);

            InitializeMessagingCenter();
            
            Popup = new PopupMenu();
        }

        private void InitializeMessagingCenter()
        {
            MessagingCenter.Subscribe<LocationSender, ChatMessageSimpleVm>(this, LocationSender.ChatReceivedMsg,
                (sender, chatMessageVm) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var myPhoneNumber = SettingsHelper.GetPhoneNumber();
                        if(chatMessageVm.Member.PhoneNumber == myPhoneNumber) return;

                        var title = "New Chat Message";
                        var body = chatMessageVm.Message;

                        void SendNotificationsAction() => SetCurrentTab("Chat");
                        await ToastHelper.Display(title, body, null, true, SendNotificationsAction);
                    });
                });
        }

        private static readonly Interval PermissionRequest = new Interval();
        private static readonly Interval AddPagesInterval = new Interval();
        private static int _permisionRequestIntervalTime = 5000;
        private static int _addPagesIntervalTime = 5000;

        private async Task StartLocationSenderAsync()
        {
            _locationSender = await LocationSender.GetInstanceAsync();
        }

        public static void AttemptLoadPagesNeedingPermissions()
        {
            AttemptLoadPagesNeedingPermissionsAsync().ConfigureAwait(true);
        }

        public static async Task SetMainPageAsync()
        {
            try
            {
                _mainTabbedPage = new TabbedPage();               
                Current.MainPage = _mainTabbedPage;

                AddPage(new MainPage(), "Main");

                await AttemptLoadPagesNeedingPermissionsAsync();


                AddPage(new AboutPage(), "About");

                _mainTabbedPage.CurrentPageChanged += async (sender, args) =>
                {
                    try
                    {
                        var currentPage = GetMainTab().CurrentPage;
                        if(string.IsNullOrWhiteSpace(currentPage?.Title)) return;
                        switch(currentPage.Title)
                        {
                            case "Join":
                                var joinNavigationPage = GetPage("Join");
                                var joinNavigationStack = joinNavigationPage?.Navigation?.NavigationStack;
                                if(joinNavigationStack == null) return;
                                var index = joinNavigationStack.Count - 1;
                                if(joinNavigationStack[index] is JoinPage joinPage)
                                {
                                    await joinPage?.RefreshInvitations();
                                }
                                break;
                        }
                    }
                    catch(System.Exception exx)
                    {
                        Crashes.TrackError(exx);
                    }
                };

            }
            catch (System.Exception ex)
            {
                Analytics.TrackEvent("Permissions", new Dictionary<string, string>
                {
                    {"App.xaml.cs_SetMainPage_Error" , ex.Message}
                });
            }
        }

        private static async Task AttemptLoadPagesNeedingPermissionsAsync()
        {
            var alreadyHasPermissions = await PermissionHelper.HasNecessaryPermissions();
            if(alreadyHasPermissions)
            {
                _permisionRequestIntervalTime = 250;
                _addPagesIntervalTime = 250;
                PermissionRequest.SetInterval(InsertPagesNeedingPermissions, _permisionRequestIntervalTime);
            }
            else
            {
                var gpsEnabled = await PermissionHelper.HasGpsEnabled();
                if(!gpsEnabled)
                {
                    _permisionRequestIntervalTime = 15000;
                    _addPagesIntervalTime = 15000;
                    PermissionRequest.SetInterval(AttemptLoadPagesNeedingPermissions, _permisionRequestIntervalTime);
                    await PermissionHelper.RequestGpsEnable();
                }
                else
                {
                    PermissionRequest.SetInterval(InsertPagesNeedingPermissions, _permisionRequestIntervalTime);
                }
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
            InsertPageBeforeAbout(new MapPage(), "Map");
            InsertPageBeforeAbout(new ChatPage(), "Chat");
            InsertPageBeforeAbout(new JoinPage(), "Join");
        }

        private static void AddPage(Page page, string title)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (GetPage(title) is NavigationPage existingNavigationPage) return;

                var navigationPage = new NavigationPage(page)
                {
                    Title = title,
                    Icon = GetIcon()
                };
                _mainTabbedPage.Children.Add(navigationPage);
            });
        }

        private static FileImageSource GetIcon()
        {
            FileImageSource icon = null;
            switch(Device.RuntimePlatform)
            {
                case Device.iOS:
                    icon = "tab_feed.png";
                    break;
                default:
                    icon = null;
                    break;
            }
            return icon;
        }

        private static void InsertPageBeforeAbout(Page page, string title)
        {
            Device.BeginInvokeOnMainThread(() => {
                if (GetPage(title) is NavigationPage existingNavigationPage) return;

                var navigationPage = new NavigationPage(page)
                {
                    Title = title,
                    Icon = GetIcon()
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

        
        //https://github.com/SKLn-Rad/Xam.Plugin.PopupMenu
        //http://nugetmusthaves.com/Package/Xam.Plugin.PopupMenu
        //App.ShowPopup(items); //This is what would pop it.  Need to get access to a view 
        //((App)Application.Current).ShowPopup(items);
        /// <summary>
        /// This in an example of how to send in the items
        ///var items = new ObservableCollection &lt; PopupItem &gt;
        ///{
        ///    new PopupItem("Text1", async () => { await DisplayAlert("Item Tapped", "An item was tapped.", "OK"); }),
        ///    new PopupItem("Text2", () => { Debug.WriteLine("Text2"); }),
        ///    new PopupItem("Text3", () => { Debug.WriteLine("Text3"); }),
        ///};
        /// ((App)Xamarin.Forms.Application.Current).ShowPopup(items);
        /// </summary>
        /// <param name="items"></param>
        public void ShowPopup(ObservableCollection<PopupItem> items)
        {
            Current.NavigationProxy.PushModalAsync(new Popup(items));
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

    public class PopupViewModel
    {
        public IList<string> ListItems { get; set; } = new List<string>();

    }
}
