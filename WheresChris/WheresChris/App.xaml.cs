using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Authy.Net;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Stateless;
using StayTogether;
using StayTogether.Helpers;
using StayTogether.Models;
using WheresChris.Helpers;
using WheresChris.Views;
using WheresChris.Views.AuthenticatePhone;
using WheresChris.Views.Popup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xam.Plugin;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Application = Xamarin.Forms.Application;
using Device = Xamarin.Forms.Device;
using TabbedPage = Xamarin.Forms.TabbedPage;


[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace WheresChris
{
    public partial class App : Application
    {
        public static PopupMenu Popup;
        private static TabbedPage _mainTabbedPage;
        private static readonly Interval InitializeInterval = new Interval();
        private StateMachine<State,Trigger> _machine = new StateMachine<State, Trigger>(State.Uninitialized);

        public App()
        {
            InitializeComponent();

            Xamarin.Forms.Application.Current.On<Xamarin.Forms.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize | WindowSoftInputModeAdjust.Pan);

            SetMainPage();
            InitializeMessagingCenter();
            InitializeStateMachine(); 
        }

        private void InitializeMessagingCenter()
        {
            MessagingCenter.Subscribe<MessagingCenterSender, VerifyTokenResult>(this, LocationSender.AuthenticationSentMsg,
                (sender, result) =>
                {
                    Device.BeginInvokeOnMainThread(()=>
                    {
                        _machine.Fire(Trigger.AuthorizeAuthy);
                    });
                });
            MessagingCenter.Subscribe<MessagingCenterSender, VerifyTokenResult>(this, LocationSender.AuthenticationCompleteMsg,
                (sender, result) =>
                {
                    Device.BeginInvokeOnMainThread(()=>
                    {
                        _machine.FireAsync(Trigger.TriggerAuthyConfirmed);
                    });
                });
        }

        public enum State
        {
            Uninitialized,
            AuthySent,
            AuthyConfirmed,
            ContactsConfirmed,
            JoinPageAdded,
            LocationOn,
            LocationConfirmed,
            PhoneConfirmed,
            PhonePermissionUnknown,
            Initialized
        }

        public enum Trigger
        {
            TriggerUnInitialized,
            AuthorizeAuthy,
            TriggerAuthyConfirmed,
            ConfirmContactsPermission,
            TriggerRetryContactsPermission,
            TriggerJoinPageAdded,
            ConfirmLocationOn,
            ConfirmLocationPermission,
            TriggerConfirmPhonePermission,
            TriggerRetryPhonePermission,
            GpsPermissionsNeeded,
            TriggerPhonePermissionUnknown
        }

        private void InitializeStateMachine()
        {
            try
            {
                
                _machine.Configure(State.Uninitialized)
                    .OnEntryAsync(async ()=>
                    {
                        var phonePermissionGranted  = await PermissionHelper.HasOrRequestPhonePermission();
                        if(phonePermissionGranted)
                        {
                            _machine.Fire(Trigger.TriggerConfirmPhonePermission);
                        }
                        else
                        {
                            _machine.Fire(Trigger.TriggerPhonePermissionUnknown);
                        }
                    })
                    .Permit(Trigger.TriggerPhonePermissionUnknown, State.PhonePermissionUnknown)
                    .Permit(Trigger.TriggerConfirmPhonePermission, State.PhoneConfirmed);

                _machine.Configure(State.PhonePermissionUnknown)
                    .OnEntry(() =>
                    {
                        //wait a few seconds and try again
                        InitializeInterval.SetInterval(async() =>
                        {
                            await _machine.FireAsync(Trigger.TriggerRetryPhonePermission);
                        }, 10000);
                        
                    })
                    .Permit(Trigger.TriggerRetryPhonePermission, State.Uninitialized);
                    
                _machine.Configure(State.PhoneConfirmed)
                    .OnEntry(AuthyValidateUser)
                    .Permit(Trigger.AuthorizeAuthy, State.AuthySent)
                    .Permit(Trigger.TriggerAuthyConfirmed, State.AuthyConfirmed);

                _machine.Configure(State.AuthySent)
                    .OnEntry(AuthyValidateUser)
                    .Permit(Trigger.TriggerAuthyConfirmed, State.AuthyConfirmed);

                _machine.Configure(State.AuthyConfirmed)
                    .OnEntryAsync(async ()=>
                    {                    
                        MessagingCenter.Send(new MessagingCenterSender(), LocationSender.InitializeMainPageMsg);
                        var hasContactPermission = await PermissionHelper.HasOrRequestContactPermission();
                        if(hasContactPermission)
                        {
                            _machine.Fire(Trigger.ConfirmContactsPermission);
                        }
                        else
                        {
                            //wait a few seconds and try again
                            InitializeInterval.SetInterval(() =>
                            {
                                _machine.FireAsync(Trigger.TriggerRetryContactsPermission);
                            }, 10000);
                        }
                    })
                    .PermitReentry(Trigger.TriggerRetryContactsPermission)
                    .Permit(Trigger.TriggerRetryContactsPermission, State.AuthyConfirmed)
                    .Permit(Trigger.ConfirmContactsPermission, State.ContactsConfirmed);


                _machine.Configure(State.ContactsConfirmed)
                    .OnEntry(() =>
                    {
                        InsertPageBeforeAbout(new InvitePage(), "Invite");
                        _machine.Fire(Trigger.TriggerJoinPageAdded);
                    })
                    .Permit(Trigger.TriggerJoinPageAdded, State.JoinPageAdded);
                

                _machine.Configure(State.JoinPageAdded)
                    .OnEntryAsync(async () =>
                    {
                        var gpsEnabled = await PermissionHelper.HasGpsEnabled();
                        if (gpsEnabled)
                        {
                            _machine.Fire(Trigger.ConfirmLocationOn);
                        }
                        else
                        {
                            await PermissionHelper.RequestGpsEnable();
                            PermissionRequest.SetInterval(() =>
                            {
                                _machine.Fire(Trigger.GpsPermissionsNeeded);
                            }, 15000);

                        }
                    })
                    .PermitReentry(Trigger.GpsPermissionsNeeded)               
                    .Permit(Trigger.GpsPermissionsNeeded, State.JoinPageAdded)
                    .Permit(Trigger.ConfirmLocationOn, State.LocationOn);
                

                _machine.Configure(State.LocationOn)
                    .Permit(Trigger.ConfirmLocationOn, State.LocationConfirmed);

                _machine.Configure(State.LocationConfirmed)
                    .OnEntry(() =>
                    {
                        InsertPageBeforeAbout(new MapPage(), "Map");
                        InsertPageBeforeAbout(new ChatPage(), "Chat");
                        InsertPageBeforeAbout(new JoinPage(), "Join");
                    })
                    .Permit(Trigger.ConfirmLocationPermission, State.Initialized);

                _machine.FireAsync(Trigger.AuthorizeAuthy);
            }
            catch(System.Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    {"Source", ex.Source},
                    {"stackTrace", ex.StackTrace},
                    {"State", _machine.State.ToString()},
                    {"PermittedTriggers", _machine.PermittedTriggers.ToString() }
                });
            }
        }

        public void AuthyValidateUser()
        {
            if(PermissionHelper.IsAuthyAuthenticated())
            {
                _machine.Fire(Trigger.TriggerAuthyConfirmed);
            }
            else
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var authenticatePhonePage = new AuthenticatePhonePage();
                    await Current.NavigationProxy.PushModalAsync(authenticatePhonePage);
                });
            }
        }


        private static readonly Interval PermissionRequest = new Interval();
        private static readonly Interval AddPagesInterval = new Interval();
        private static int _permisionRequestIntervalTime = 5000;
        private static int _addPagesIntervalTime = 5000;

        private static async Task StartLocationSenderAsync()
        {
            await LocationSender.GetInstanceAsync()
                .ContinueWith((t) =>
                {
                    MessagingCenter.Send(new MessagingCenterSender(), LocationSender.LeaveOrEndGroupMsg);
                });
        }

        public static void AttemptLoadPagesNeedingPermissions()
        {
            AsyncHelper.RunSync(AttemptLoadPagesNeedingPermissionsAsync);
        }

        public static void SetMainPage()
        {
            try
            {
                _mainTabbedPage = new TabbedPage();
                Current.MainPage = _mainTabbedPage;

                AddPage(new MainPage(), "Main");

                AddPage(new AboutPage(), "About");

                //var authyAuthenticated = PermissionHelper.IsAuthyAuthenticated();
                //if(authyAuthenticated)
                //{
                //    PermissionRequest.SetInterval(AttemptLoadPagesNeedingPermissions, 5000);
                //}
            }
            catch (System.Exception ex)
            {
                Analytics.TrackEvent("Permissions", new Dictionary<string, string>
                {
                    {"App.xaml.cs_SetMainPage_Error" , ex.Message}
                });
            }
        }

        private static void InitializeTabbedPageLoads()
        {
            _mainTabbedPage.CurrentPageChanged += async (sender, args) =>
            {
                try
                {
                    var currentPage = GetMainTab().CurrentPage;
                    if(string.IsNullOrWhiteSpace(currentPage?.Title)) return;

                    var currentNavigationPage = GetPage(currentPage?.Title);
                    var currentNavigationStack = currentNavigationPage?.Navigation?.NavigationStack;
                    if(currentNavigationStack == null) return;
                    var index = currentNavigationStack.Count - 1;

                    switch(currentNavigationStack[index])
                    {
                        case JoinPage joinPage:
                            await joinPage?.RefreshInvitations();
                            break;
                        case ChatPage chatPage:
                            await chatPage?.InitializeChat();
                            break;
                    }
                }
                catch(System.Exception exx)
                {
                    Crashes.TrackError(exx, new Dictionary<string, string>
                    {
                        {"Source", exx.Source},
                        {"stackTrace", exx.StackTrace}
                    });
                }
            };
        }

        public static void FinishInitializing()
        {
            InitializeTabbedPageLoads();

            AsyncHelper.RunSync(StartLocationSenderAsync);
          
            Popup = new PopupMenu();
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

            PermissionRequest.SetInterval(FinishInitializing, 5000);
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
            Device.BeginInvokeOnMainThread(() =>
            {
                GetMainTab().CurrentPage = GetPage(title);
            });
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
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Current.NavigationProxy.PushModalAsync(new Popup(items)); 
            });         
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
