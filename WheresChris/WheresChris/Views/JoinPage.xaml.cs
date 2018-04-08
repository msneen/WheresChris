using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using StayTogether;
using StayTogether.Models;
using WheresChris.Helpers;
using WheresChris.NotificationCenter;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WheresChris.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JoinPage : ContentPage
    {
        public JoinPage()
        {
            Title = "Where's Chris - Join Group";
            InitializeComponent();
            BindingContext = new JoinPageViewModel
            {
                Title = "Where's Chris - Join Group"
            };
            InitializeMessagingCenterSubscriptions();
        }

        private void InitializeMessagingCenterSubscriptions()
        {
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupInvitationReceivedMsg,
            (sender) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await ((JoinPageViewModel)BindingContext).RequestInvitations();
                });
            });
            MessagingCenter.Subscribe<LocationSender, InvitationList>(this, LocationSender.GroupInvitationsMsg,
            (sender, invitationList) =>
            {
                Device.BeginInvokeOnMainThread( () =>
                {
                    ((JoinPageViewModel)BindingContext).LoadInvitations(invitationList);
                });
            });
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
            => ((ListView)sender).SelectedItem = null;

        void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            //await DisplayAlert("Selected", e.SelectedItem.ToString(), "OK");
            if (e.SelectedItem is ContactDisplayItemVm selectedItem)
            {
                var groupMemberSimpleVm = new GroupMemberSimpleVm
                {
                    Name = selectedItem.Invitation.Name,
                    PhoneNumber = selectedItem.Invitation.PhoneNumber
                };
                MessagingCenter.Send<MessagingCenterSender, GroupMemberSimpleVm>(new MessagingCenterSender(), LocationSender.ConfirmGroupInvitationMsg, groupMemberSimpleVm);
            }
            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        private async void RefreshButton_OnClicked(object sender, EventArgs e)
        {
            await RefreshInvitations();
        }

        public async Task RefreshInvitations()
        {
            await ((JoinPageViewModel) BindingContext).RequestInvitations();
        }

        private async Task SearchEntry_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if(e.NewTextValue.Length == 10)
            {
                RequestJoinButton.IsEnabled = e.NewTextValue.IsNumeric();
            }
            else if(e.NewTextValue.Length == 0 || e.NewTextValue.Length > 2)
            {
                RequestJoinButton.IsEnabled = false;
                await InitializeContactsAsync(e.NewTextValue);
            }
        }

        private void SearchButton_OnClicked(object sender, EventArgs e)
        {
            var phoneNumber = SearchEntry.Text;
            var contact = _invitePageViewModel.GetContact(phoneNumber);
            if(contact != null)
            {
                _selectedContactDisplayItemVm = contact;
                RequestJoinButton.IsEnabled = true;
            }
            SearchButton.IsEnabled = false;
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {

            //deselect the previous item
            if(_selectedContactDisplayItemVm != null && _selectedContactLayout != null)
            {
                SetSelection(_selectedContactDisplayItemVm, _selectedContactLayout, false);
                //_selectedContactDisplayItemVm.Selected = false;
            }
            //find the new item and select
            _selectedContactLayout = (RelativeLayout) sender;
            _selectedContactDisplayItemVm = (ContactDisplayItemVm) ((RelativeLayout) sender).BindingContext;
            SetSelection(_selectedContactDisplayItemVm, _selectedContactLayout, true);
            RequestJoinButton.IsEnabled = true;
        }

        private void RequestJoinButton_OnClicked(object sender, EventArgs e)
        {
            if(_selectedContactDisplayItemVm == null) return;

            //send request.  Using this because it conveniently creates the correct transport model
            InAnotherGroupNotificationResponse.ConfirmEndMyGroupAndJoinAnother(_selectedContactDisplayItemVm.PhoneNumber);
        }

        private void SetSelection(ContactDisplayItemVm contactDisplayItemVm, RelativeLayout contactLayout, bool selected)
        {
            var selectedColor = Color.LightSkyBlue;
            if (Application.Current.Resources.TryGetValue("ListViewSelectedColor", out var themeColor))
            {
                selectedColor = (Color) themeColor;
            }
            contactDisplayItemVm.Selected = selected;
            var color = contactDisplayItemVm.Selected ? selectedColor : ContactsListView.BackgroundColor;
            contactLayout.BackgroundColor = color;
        }

        private InvitePageViewModel _invitePageViewModel = new InvitePageViewModel();
        private ContactDisplayItemVm _selectedContactDisplayItemVm;
        private RelativeLayout _selectedContactLayout;

        public async Task InitializeContactsAsync(string characters = "")
        {
            var hasPermissions = await PermissionHelper.HasOrRequestContactPermission();

            if(hasPermissions)
            {
                try
                {
                    ContactsListView.ItemsSource = null;
                    
                    await _invitePageViewModel.InitializeContactsAsync(characters);
                    ContactsListView.ItemsSource = _invitePageViewModel.Items;
                    //ContactsListView.SetBinding(ListView.ItemsSourceProperty, "Items", BindingMode.TwoWay);                                  
                }
                catch (Exception ex)
                {
                    Analytics.TrackEvent("JoinPage_InitializeContacts_error", new Dictionary<string, string>
                    {
                        {"ErrorMessage", ex.Message.Substring(0, Math.Min(60, ex.Message.Length))}
                    });
                }
            }
        }


    }

    class JoinPageViewModel
    {
        public string Title { get; set; }
        public ObservableCollection<ContactDisplayItemVm> Items { get; }

        public JoinPageViewModel()
        {
            Items = new ObservableCollection<ContactDisplayItemVm>();
        }

        public Task RequestInvitations()
        {
            Items.Clear();
            MessagingCenter.Send<MessagingCenterSender>(new MessagingCenterSender(), LocationSender.GetInvitationsMsg);

            return Task.CompletedTask;
        }

        public Task LoadInvitations(InvitationList invitationList)
        {
            invitationList
                .GroupBy(g=>g.DisplayName())
                .Select(i=>i.First())
                .OrderBy(i => i.ReceivedTime)
                .ToList()
                .ForEach(invitation => Items.Add(new ContactDisplayItemVm
                {
                    Name = invitation.DisplayName(),
                    Invitation = invitation
                }));
            return Task.CompletedTask;
        }
    }
}
