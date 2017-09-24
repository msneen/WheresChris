using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using StayTogether;
using StayTogether.Models;
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
                    await((JoinPageViewModel)BindingContext).LoadInvitations();
                });
            });
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
            => ((ListView)sender).SelectedItem = null;

        async void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            //await DisplayAlert("Selected", e.SelectedItem.ToString(), "OK");
            var selectedItem = e.SelectedItem as ContactDisplayItemVm;
            if (selectedItem != null)
            {
                var groupMemberSimpleVm = new GroupMemberSimpleVm
                {
                    Name = selectedItem.Invitation.Name,
                    PhoneNumber = selectedItem.Invitation.PhoneNumber
                };
                MessagingCenter.Send<JoinPage, GroupMemberSimpleVm>(this, LocationSender.ConfirmGroupInvitationMsg, groupMemberSimpleVm);
            }
            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        private async void RefreshButton_OnClicked(object sender, EventArgs e)
        {
            await ((JoinPageViewModel)BindingContext).LoadInvitations();
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

        public async Task LoadInvitations()
        {
            Items.Clear();
            var locationSender = await LocationSenderFactory.GetLocationSender();
            var invitationList = locationSender.GetInvitations();
            invitationList
                .OrderBy(i => i.ReceivedTime)
                .ToList()
                .ForEach(invitation => Items.Add(new ContactDisplayItemVm
                {
                    Name = invitation.DisplayName(),
                    Invitation = invitation
                }));
        }
    }
}
