using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using WheresChris.Helpers;
using Plugin.Settings;
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
            Task.Run(() => InitializeContacts()).Wait();            
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
                invitePageViewModel
                    .Items
                    .FirstOrDefault(x => x.PhoneNumber == groupMember.PhoneNumber)
                    .Selected = false;
            }
            SetFormEnabled(false);
            NavigateToPage("Map");
        }

        private void SetFormEnabled(bool isSelected)
        {
            InviteButton.IsEnabled = isSelected;
            ExpirationPicker.IsEnabled = isSelected;
            ContactsListView.IsEnabled = isSelected;
        }

        protected async void InitializeContacts()
        { 
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Contacts);
            if (status != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] {Permission.Contacts});
                status = results[Permission.Contacts];
            }
            if (status == PermissionStatus.Granted)
            {
                //Todo:  Turn me back on.  For debugging iPhone Crashes
                //await ((InvitePageViewModel) BindingContext).InitializeContacts();
                //ContactsListView.ItemsSource = ((InvitePageViewModel) BindingContext).Items;
            }

        }
        private void NavigateToPage(string title)
        {
            var masterPage = Parent.Parent as TabbedPage;
            var invitePage = masterPage?.Children.FirstOrDefault(x => x.Title == title);
            if (invitePage == null) return;

            var index = masterPage.Children.IndexOf(invitePage);
            if (index > -1)
            {
                masterPage.CurrentPage = masterPage.Children[index];
            }
        }
    }
}