using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using StayTogether;
using StayTogether.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using WheresChris.Helpers;
using WheresChris.Models;

namespace WheresChris.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    // ReSharper disable once RedundantExtendsListEntry
    public partial class InvitePage : ContentPage
    {
        private readonly Interval _contactInterval = new Interval();


        public InvitePage()
        {
            Title = "Where's Chris - Invite";
            InitializeComponent ();
            InitializeMessagingCenterSubscriptions();
            BindingContext = new InvitePageViewModel
            {
                Title = "Where's Chris - Invite"
            };
            InitializeExpirationPicker();
            _contactInterval.SetInterval(LoadContacts, 5000);                      
        }

        private void LoadContacts()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var permissionStatus = await PermissionHelper.HasOrRequestContactPermission();
                if (permissionStatus)
                {
                    await InitializeContactsAsync();
                }
            });
        }

        private void InitializeMessagingCenterSubscriptions()
        {
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupJoinedMsg,
            (sender) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SetFormEnabled(false);
                    App.SetCurrentTab("Map");
                });
            });
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupCreatedMsg,
            (sender) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SetFormEnabled(false);
                    App.SetCurrentTab("Map");
                });
            });

            //If the group is disbanded, it means this user also left the group with everyone else
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupDisbandedMsg,
            (sender) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SetFormEnabled(true);
                });
            });
            //This user left the group
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.ThisUserLeftGroupMsg,
            (sender) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SetFormEnabled(true);
                });
            });

        }

        private void InitializeExpirationPicker()
        {
            ExpirationPicker.ItemsSource = new List<ExpirationPickerViewModel>
            {
                new ExpirationPickerViewModel {DisplayName = "4 Hours", Hours = 4},
                new ExpirationPickerViewModel {DisplayName = "6 Hours", Hours = 6},
                new ExpirationPickerViewModel {DisplayName = "8 Hours", Hours = 8},
                new ExpirationPickerViewModel {DisplayName = "12 Hours", Hours = 12},
                new ExpirationPickerViewModel {DisplayName = "24 Hours", Hours = 24},
                new ExpirationPickerViewModel {DisplayName = "48 Hours", Hours = 48},
            };
            ExpirationPicker.ItemDisplayBinding = new Binding("DisplayName");
            ExpirationPicker.SelectedIndex = 0;
        }

        public async Task StartGroup(object sender, EventArgs e)
        {            
            if(!((InvitePageViewModel) BindingContext).IsEnabled) return;
            
            App.SetCurrentTab("Map");

            var userPhoneNumber = SettingsHelper.GetPhoneNumber();

            if (!(BindingContext is InvitePageViewModel invitePageViewModel)) return;

            ContactsListView.ItemsSource = null;
            var selectedGroupMemberVms = await ((InvitePageViewModel)BindingContext).AddToGroup();
            ContactsListView.SetBinding(ListView.ItemsSourceProperty, "Items", BindingMode.TwoWay);

            if(selectedGroupMemberVms == null || selectedGroupMemberVms.Count < 1) return;

            var selectedExpirationHours = ExpirationPicker.SelectedItem as ExpirationPickerViewModel;

            var expirationHours = selectedExpirationHours?.Hours ?? 4;
            if (!selectedGroupMemberVms.Any()) return;

            await GroupActionsHelper.StartGroup(selectedGroupMemberVms, userPhoneNumber, expirationHours);

            var savedInvitation = new Invitation
            {
                Members = selectedGroupMemberVms,
                UserPhoneNumber = userPhoneNumber,
                ExpirationHours = expirationHours
            };
            InvitationHelper.SaveInvitation(savedInvitation);

            SetFormEnabled(false);
        }



        private void SetFormEnabled(bool isEnabled)
        {
            ((InvitePageViewModel) BindingContext).IsEnabled = isEnabled;
            AddButton.IsEnabled = isEnabled;
            InviteButton.IsEnabled = isEnabled;
            ExpirationPicker.IsEnabled = isEnabled;
            ContactsListView.IsEnabled = isEnabled;
            if(isEnabled == false)
            {
                SearchEntry.Text = "";
            }
            SearchEntry.IsEnabled = isEnabled;
        }

        public async Task InitializeContactsAsync(string characters = "")
        {
            var hasPermissions = await PermissionHelper.HasOrRequestContactPermission();

            if(hasPermissions)
            {
                try
                {
                    ContactsListView.ItemsSource = null;
                    await ((InvitePageViewModel)BindingContext).InitializeContactsAsync(characters);
                    ContactsListView.SetBinding(ListView.ItemsSourceProperty, "Items", BindingMode.TwoWay);                                  
                }
                catch (Exception ex)
                {
                    Analytics.TrackEvent("InvitePage_InitializeContacts_error", new Dictionary<string, string>
                    {
                        {"ErrorMessage", ex.Message.Substring(0, Math.Min(60, ex.Message.Length))}
                    });
                }
            }
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            if(!((InvitePageViewModel) BindingContext).IsEnabled) return;

            if (ContactsListView.IsEnabled)
            {
                var selectedColor = Color.LightSkyBlue;
                object themeColor;
                if (Application.Current.Resources.TryGetValue("ListViewSelectedColor", out themeColor))
                {
                    selectedColor = (Color) themeColor;
                }
                var contactDisplayItemVm = (ContactDisplayItemVm) ((RelativeLayout) sender).BindingContext;
                contactDisplayItemVm.Selected = !contactDisplayItemVm.Selected;
                var color = contactDisplayItemVm.Selected ? selectedColor : ContactsListView.BackgroundColor;
                ((RelativeLayout) sender).BackgroundColor = color;
                var action = ((InvitePageViewModel)BindingContext).AddSelectedContact(contactDisplayItemVm);
                if(action == InvitePageViewModel.InviteeAction.Removed)
                {
                    ViewInviteeList().ConfigureAwait(true);
                }
            }
        }




        private async Task SearchEntry_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if(!((InvitePageViewModel) BindingContext).IsEnabled) return;
            if(e.NewTextValue.Length == 10)
            {
                HandlePhoneNumber(e.NewTextValue);
            }
            else if(e.NewTextValue.Length == 0 || e.NewTextValue.Length > 2)
            {
                AddButton.IsEnabled = false;
                await InitializeContactsAsync(e.NewTextValue);
            }

        }

        private void HandlePhoneNumber(string text)
        {
            AddButton.IsEnabled = text.IsNumeric();
        }

        private void AddButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            if(!((InvitePageViewModel) BindingContext).IsEnabled) return;
            var phoneNumber = SearchEntry.Text;
            ((InvitePageViewModel)BindingContext).AddPhoneNumbeContact(phoneNumber);
            AddButton.IsEnabled = false;
            SearchEntry.Text = "";
        }


        private async Task ContactsListButton_OnClicked(object sender, EventArgs e)
        {
            if(!((InvitePageViewModel) BindingContext).IsEnabled) return;

            SearchEntry.Text = "";
            ContactsListView.ItemsSource = null;
            await InitializeContactsAsync();
            ContactsListView.SetBinding(ListView.ItemsSourceProperty, "Items", BindingMode.TwoWay);
        }

        private async Task InviteListButton_OnClicked(object sender, EventArgs e)
        {
            if(!((InvitePageViewModel) BindingContext).IsEnabled) return;

            await ViewInviteeList();
        }

        private async Task ViewInviteeList()
        {
            SearchEntry.Text = "";
            ContactsListView.ItemsSource = null;
            await ((InvitePageViewModel) BindingContext).AddToGroup();
            ContactsListView.SetBinding(ListView.ItemsSourceProperty, "Items", BindingMode.TwoWay);
        }
    }
}