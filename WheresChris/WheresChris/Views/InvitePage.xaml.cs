using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Mobile.Analytics;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using WheresChris.Helpers;
using WheresChris.Messaging;

namespace WheresChris.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    // ReSharper disable once RedundantExtendsListEntry
    public partial class InvitePage : ContentPage
    {
        public GroupLeftEvent GroupLeftEvent;
        public GroupJoinedEvent GroupJoinedEvent;

        public InvitePage()
        {
            Title = "Invite Chris";
            InitializeComponent ();
            InitializeMessagingCenterSubscriptions();
            BindingContext = new InvitePageViewModel
            {
                Title = "Invite Chris"
            };
            InitializeExpirationPicker();                      
        }

        private bool _hasAppeared;
        protected override async void OnAppearing()
        {
            if (_hasAppeared) return;

            await InitializeContactsAsync();
        
            _hasAppeared = true;
        }

        private void InitializeMessagingCenterSubscriptions()
        {
            GroupJoinedEvent = new GroupJoinedEvent();
            GroupJoinedEvent.OnGroupJoinedMsg += (sender, args) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SetFormEnabled(false);
                });
                App.SetCurrentTab("Map");
            };

            GroupLeftEvent = new GroupLeftEvent();
            GroupLeftEvent.OnGroupLeftMsg += (sender, args) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SetFormEnabled(true);
                });
            };
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

        public async void StartGroup(object sender, EventArgs e)
        {
            var userPhoneNumber = SettingsHelper.GetPhoneNumber();

            var invitePageViewModel = BindingContext as InvitePageViewModel;
            if (invitePageViewModel == null) return;
            var selectedGroupMemberVms = GroupActionsHelper.GetSelectedGroupMembers(invitePageViewModel.Items);

            var selectedExpirationHours = ExpirationPicker.SelectedItem as ExpirationPickerViewModel;

            var expirationHours = selectedExpirationHours?.Hours ?? 4;
            if (!selectedGroupMemberVms.Any()) return;

            await GroupActionsHelper.StartGroup(selectedGroupMemberVms, userPhoneNumber, expirationHours);
            foreach (var groupMember in selectedGroupMemberVms)
            {
                var contactDisplayItemVm = invitePageViewModel
                    .Items
                    .FirstOrDefault(x => x.PhoneNumber == groupMember.PhoneNumber);
                if (contactDisplayItemVm != null)
                    contactDisplayItemVm
                        .Selected = false;
            }
            SetFormEnabled(false);
            App.SetCurrentTab("Map");

        }

        private void SetFormEnabled(bool isSelected)
        {
            InviteButton.IsEnabled = isSelected;
            ExpirationPicker.IsEnabled = isSelected;
            ContactsListView.IsEnabled = isSelected;
        }

        public async Task InitializeContactsAsync()
        { 
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Contacts);
            if (status != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] {Permission.Contacts});
                status = results[Permission.Contacts];
            }
            if (status == PermissionStatus.Granted)
            {
                try
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Task.Run(async () =>
                        {
                            await ((InvitePageViewModel)BindingContext).InitializeContactsAsync();
                            ContactsListView.ItemsSource = ((InvitePageViewModel)BindingContext).Items;
                        });

                    });
                }
                catch (Exception ex)
                {
                    Analytics.TrackEvent("InvitePage_InitializeContacts", new Dictionary<string, string>
                    {
                        {"ErrorMessage", ex.Message.Substring(0, Math.Min(60, ex.Message.Length))}
                    });
                }
            }
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {

            var selectedColor = Color.LightSkyBlue;
            object themeColor;
            if (Application.Current.Resources.TryGetValue("ListViewSelectedColor", out themeColor))
            {
                selectedColor = (Color)themeColor;
            }
            var contactDisplayItemVm = (ContactDisplayItemVm)((RelativeLayout) sender).BindingContext;
            contactDisplayItemVm.Selected = !contactDisplayItemVm.Selected;
            var color = contactDisplayItemVm.Selected ? selectedColor : ContactsListView.BackgroundColor;
            ((RelativeLayout) sender).BackgroundColor = color;
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