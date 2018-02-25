using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Mobile.Analytics;
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
            App.SetCurrentTab("Map");

            var userPhoneNumber = SettingsHelper.GetPhoneNumber();

            var invitePageViewModel = BindingContext as InvitePageViewModel;
            if (invitePageViewModel == null) return;

            if (invitePageViewModel.Items == null) return;
            if (invitePageViewModel.Items.Count <= 0) return;

            var selectedGroupMemberVms = GroupActionsHelper.GetSelectedGroupMembers(invitePageViewModel.Items);

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

            lock (selectedGroupMemberVms)
            {
                foreach (var groupMember in selectedGroupMemberVms)
                {
                    var contactDisplayItemVm = invitePageViewModel
                        .Items
                        .FirstOrDefault(x => x.PhoneNumber == groupMember.PhoneNumber);
                    if (contactDisplayItemVm != null)
                    {
                        contactDisplayItemVm.Selected = false;
                        contactDisplayItemVm.BackgroundColor = ContactsListView.BackgroundColor;
                    }
                }
            }

            SetFormEnabled(false);

        }

        private void SetFormEnabled(bool isSelected)
        {
            InviteButton.IsEnabled = isSelected;
            ExpirationPicker.IsEnabled = isSelected;
            ContactsListView.IsEnabled = isSelected;
        }

        public async Task InitializeContactsAsync()
        {
            var hasPermissions = await PermissionHelper.HasOrRequestContactPermission();

            if(hasPermissions)
            {
                try
                {
                    await ((InvitePageViewModel)BindingContext).InitializeContactsAsync();
                    //ContactsListView.ItemsSource = ((InvitePageViewModel)BindingContext).Items; 
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
            }
        }

        private void SearchButton_OnClicked(object sender, EventArgs e)
        {
            var searchText = SearchEntry.Text;
            if (string.IsNullOrWhiteSpace(searchText)) return;

            var foundItem = ((InvitePageViewModel)BindingContext).Items.FirstOrDefault(i=>i.Name.StartsWith(searchText,StringComparison.CurrentCultureIgnoreCase));
            if (foundItem == null) return;

            ContactsListView.ScrollTo(foundItem, ScrollToPosition.Center, false);
            SearchEntry.Text = string.Empty;
        }
    }
}