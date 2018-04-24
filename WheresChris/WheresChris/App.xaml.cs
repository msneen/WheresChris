using System;
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
        private readonly StateMachine<State,Trigger> _workflowStateMachine = new StateMachine<State, Trigger>(State.Uninitialized);
        private static readonly Interval PermissionRequest = new Interval();
        private static readonly Interval AddPagesInterval = new Interval();
        private static int _permisionRequestIntervalTime = 5000;
        private static int _addPagesIntervalTime = 5000;

        public App()
        {
            InitializeComponent();

            Xamarin.Forms.Application.Current.On<Xamarin.Forms.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize | WindowSoftInputModeAdjust.Pan);

            SetMainPage();

            InitializeInterval.SetInterval(InitializeStateMachine, 1000);
             
        }

        public enum State
        {
            Uninitialized,

            InitializingPhone,
            PhoneConfirmed,
            PhonePermissionUnknown,

            InitializingAuthy,
            AuthyConfirmed,
            AuthyUnknown,

            InitializingContacts,
            ContactsConfirmed,
            ContactsPermissionUnknown,

            JoinPageAdded,

            InitializingLocation,
            LocationConfirmed,
            LocationPermissionUnknown,
                        
            InitializingGPS,
            ConfirmGpsOn,
            RetryEnableGPS,

            InitializingJoinPage,

            InsertingPages,
            InsertingPagesFinished
        }

        public enum Trigger
        {
            TriggerInitializingPhone,
            TriggerPhoneConfirmed,           
            TriggerPhonePermissionUnknown,

            TriggerInitializingAuthy,
            TriggerAuthyConfirmed,
            TriggerAuthyUnknown,

            TriggerInitializingContacts,
            TriggerContactsConfirmed,
            TriggerContactsPermissionUnknown,

            TriggerInitializingJoinPage,
            TriggerJoinPageAdded,

            TriggerInitializingLocation,
            TriggerLocationConfirmed,
            TriggerLocationPermissionUnknown,

            TriggerInitializingGPS,
            TriggerRetryEnabaGPS,
            TriggerConfirmGpsOn,

            TriggerInsertingPages,
            TriggerInsertingPagesFinished

        }

        private void InitializeStateMachine()
        {
            try
            {

                _workflowStateMachine.Configure(State.Uninitialized)
                    .Permit(Trigger.TriggerInitializingPhone, State.InitializingPhone);


                _workflowStateMachine.Configure(State.InitializingPhone)
                    .OnEntryAsync(async ()=> { await RequestPhonePermissions(); })
                    .Permit(Trigger.TriggerPhonePermissionUnknown, State.PhonePermissionUnknown)
                    .Permit(Trigger.TriggerPhoneConfirmed, State.PhoneConfirmed);


                _workflowStateMachine.Configure(State.PhonePermissionUnknown)
                    .OnEntry(() =>
                    {
                        //wait a few seconds and try again
                        InitializeInterval.SetInterval(async() =>
                        {
                            await _workflowStateMachine.FireAsync(Trigger.TriggerInitializingPhone);
                        }, 10000);
                        
                    })
                    .Permit(Trigger.TriggerInitializingPhone, State.InitializingPhone);
                    

                _workflowStateMachine.Configure(State.PhoneConfirmed)
                    .OnEntry(()=>{_workflowStateMachine.Fire(Trigger.TriggerInitializingAuthy);})
                    .Permit(Trigger.TriggerInitializingAuthy, State.InitializingAuthy);

                _workflowStateMachine.Configure(State.InitializingAuthy)
                    .OnEntry(AuthyValidateUser)
                    .OnExit(()=>{MessagingCenter.Send(new MessagingCenterSender(), LocationSender.InitializeMainPageMsg);})
                    .Permit(Trigger.TriggerAuthyUnknown, State.AuthyUnknown)
                    .Permit(Trigger.TriggerAuthyConfirmed, State.AuthyConfirmed);

                _workflowStateMachine.Configure(State.AuthyUnknown)
                    .OnEntry(() =>
                    {
                        //wait a few seconds and try again
                        InitializeInterval.SetInterval(async () =>
                        {
                            await _workflowStateMachine.FireAsync(Trigger.TriggerInitializingAuthy);
                        }, 10000);
                    })
                    .Permit(Trigger.TriggerInitializingAuthy, State.InitializingAuthy);

                _workflowStateMachine.Configure(State.AuthyConfirmed)
                    .OnEntry(() => { _workflowStateMachine.Fire(Trigger.TriggerInitializingContacts); })
                    .Permit(Trigger.TriggerInitializingContacts, State.InitializingContacts);
                

                _workflowStateMachine.Configure(State.InitializingContacts)
                    .OnEntry(async() => { await RequestContactsPermissions(); })
                    .Permit(Trigger.TriggerContactsPermissionUnknown, State.ContactsPermissionUnknown)
                    .Permit(Trigger.TriggerContactsConfirmed, State.ContactsConfirmed);


                _workflowStateMachine.Configure(State.ContactsConfirmed)
                    .OnEntry(()=>{_workflowStateMachine.Fire(Trigger.TriggerInitializingJoinPage);})
                    .Permit(Trigger.TriggerInitializingJoinPage, State.InitializingJoinPage);

                _workflowStateMachine.Configure(State.ContactsPermissionUnknown)
                    .OnEntry(() =>
                    {
                        _workflowStateMachine.Fire(Trigger.TriggerInitializingContacts);
                    })
                    .Permit(Trigger.TriggerInitializingContacts, State.InitializingContacts);


                _workflowStateMachine.Configure(State.InitializingJoinPage)
                    .OnEntry(() =>
                    {
                        InsertPageBeforeAbout(new InvitePage(), "Invite");
                        _workflowStateMachine.Fire(Trigger.TriggerJoinPageAdded);
                    })
                    .Permit(Trigger.TriggerJoinPageAdded, State.JoinPageAdded);

                
                _workflowStateMachine.Configure(State.JoinPageAdded)
                    .OnEntry(()=>{_workflowStateMachine.Fire(Trigger.TriggerInitializingLocation);})
                    .Permit(Trigger.TriggerInitializingLocation, State.InitializingLocation);


                _workflowStateMachine.Configure(State.InitializingLocation)
                    .OnEntry(async () => { await RequestLocationPermission(); })
                    .Permit(Trigger.TriggerLocationPermissionUnknown, State.LocationPermissionUnknown)
                    .Permit(Trigger.TriggerLocationConfirmed, State.LocationConfirmed);

                _workflowStateMachine.Configure(State.LocationPermissionUnknown)
                    .OnEntry(() => { _workflowStateMachine.Fire(Trigger.TriggerInitializingLocation); })
                    .Permit(Trigger.TriggerInitializingLocation, State.InitializingLocation);

                _workflowStateMachine.Configure(State.LocationConfirmed)
                    .OnEntry(()=>{_workflowStateMachine.Fire(Trigger.TriggerInitializingGPS);})
                    .Permit(Trigger.TriggerInitializingGPS, State.InitializingGPS);

                _workflowStateMachine.Configure(State.InitializingGPS)
                    .OnEntry(async () => { await RequestEnableGps(); })
                    .Permit(Trigger.TriggerRetryEnabaGPS, State.RetryEnableGPS)
                    .Permit(Trigger.TriggerConfirmGpsOn, State.ConfirmGpsOn);

                _workflowStateMachine.Configure(State.RetryEnableGPS)
                    .OnEntry(() => { _workflowStateMachine.Fire(Trigger.TriggerInitializingGPS); })
                    .Permit(Trigger.TriggerInitializingGPS, State.InitializingGPS);

                _workflowStateMachine.Configure(State.ConfirmGpsOn)
                    .OnEntry(() => { _workflowStateMachine.Fire(Trigger.TriggerInsertingPages); })
                    .Permit(Trigger.TriggerInsertingPages, State.InsertingPages);

                _workflowStateMachine.Configure(State.InsertingPages)
                    .OnEntry(() =>
                    {
                        InsertPagesNeedingLocation();
                        _workflowStateMachine.Fire(Trigger.TriggerInsertingPagesFinished);
                    })
                    .Permit(Trigger.TriggerInsertingPagesFinished, State.InsertingPagesFinished);


                _workflowStateMachine.Configure(State.InsertingPagesFinished);


                //Configuration Finished, start Initializing
                _workflowStateMachine.FireAsync(Trigger.TriggerInitializingPhone);
            }
            catch(System.Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    {"Source", ex.Source},
                    {"stackTrace", ex.StackTrace},
                    {"State", _workflowStateMachine.State.ToString()},
                    {"PermittedTriggers", _workflowStateMachine.PermittedTriggers.ToString() }
                });
            }
        }

        private static void InsertPagesNeedingLocation()
        {
            InsertPageBeforeAbout(new MapPage(), "Map");
            InsertPageBeforeAbout(new ChatPage(), "Chat");
            InsertPageBeforeAbout(new JoinPage(), "Join");
        }

        private async Task RequestEnableGps()
        {
            var gpsEnabled = await PermissionHelper.HasGpsEnabled();
            if(gpsEnabled)
            {
                _workflowStateMachine.Fire(Trigger.TriggerConfirmGpsOn);
            }
            else
            {
                PermissionRequest.SetInterval(() => { _workflowStateMachine.Fire(Trigger.TriggerRetryEnabaGPS); }, 15000);
            }
        }

        private async Task RequestLocationPermission()
        {
            var locationPermission = await PermissionHelper.HasOrRequestLocationPermission();
            if(locationPermission)
            {
                _workflowStateMachine.Fire(Trigger.TriggerLocationConfirmed);
            }
            else
            {
                PermissionRequest.SetInterval(() => { _workflowStateMachine.Fire(Trigger.TriggerLocationPermissionUnknown); }, 10000);
            }
        }

        private async Task RequestContactsPermissions()
        {
            var hasContactPermission = await PermissionHelper.HasOrRequestContactPermission();
            if(hasContactPermission)
            {
                _workflowStateMachine.Fire(Trigger.TriggerContactsConfirmed);
            }
            else
            {
                //wait a few seconds and try again
                InitializeInterval.SetInterval(() => { _workflowStateMachine.FireAsync(Trigger.TriggerContactsPermissionUnknown); }, 10000);
            }
        }

        private async Task RequestPhonePermissions()
        {
            try
            {
                var phonePermissionGranted = await PermissionHelper.HasOrRequestPhonePermission();
                if(phonePermissionGranted)
                {
                    _workflowStateMachine.Fire(Trigger.TriggerPhoneConfirmed);
                }
                else
                {
                    _workflowStateMachine.Fire(Trigger.TriggerPhonePermissionUnknown);
                }
            }
            catch(Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    {"Source", ex.Source},
                    {"stackTrace", ex.StackTrace},
                    {"State", _workflowStateMachine.State.ToString()}
                });
            }
        }

        public void AuthyValidateUser()
        {
            if(PermissionHelper.IsAuthyAuthenticated())
            {
                _workflowStateMachine.Fire(Trigger.TriggerAuthyConfirmed);
            }
            else
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var authenticatePhonePage = new AuthenticatePhonePage();
                    authenticatePhonePage.Disappearing += (sender, args) =>
                    {
                        if(PermissionHelper.IsAuthyAuthenticated())
                        {
                            _workflowStateMachine.Fire(Trigger.TriggerAuthyConfirmed);
                        }
                        else
                        {
                            _workflowStateMachine.Fire(Trigger.TriggerAuthyUnknown);
                        }
                    };
                    await Current.NavigationProxy.PushModalAsync(authenticatePhonePage);
                });
            }
        }

        private static async Task StartLocationSenderAsync()
        {
            await LocationSender.GetInstanceAsync()
                .ContinueWith((t) =>
                {
                    MessagingCenter.Send(new MessagingCenterSender(), LocationSender.LeaveOrEndGroupMsg);
                });
        }

        public static void SetMainPage()
        {
            try
            {
                _mainTabbedPage = new TabbedPage();
                Current.MainPage = _mainTabbedPage;

                AddPage(new MainPage(), "Main");

                AddPage(new AboutPage(), "About");
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
