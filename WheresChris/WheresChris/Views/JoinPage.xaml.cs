using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WheresChris.Messaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XLabs;

namespace WheresChris.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JoinPage : ContentPage
    {
        public InvitationReceivedEvent InvitationReceivedEvent;

        public JoinPage()
        {
            Title = "Join Group";
            InitializeComponent();
            BindingContext = new JoinPageViewModel
            {
                Title = "Join Group"
            };
            InitializeMessagingCenterSubscriptions();
        }

        private void InitializeMessagingCenterSubscriptions()
        {
            InvitationReceivedEvent = new InvitationReceivedEvent();
            InvitationReceivedEvent.OnInvitationReceivedMsg += (sender, args) =>
            {
                ((JoinPageViewModel)BindingContext).LoadInvitations();
            };
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
                var locationSender = LocationSenderFactory.GetLocationSender();
                await locationSender.ConfirmGroupInvitation(selectedItem.Invitation.PhoneNumber, selectedItem.Invitation.Name);
            }
            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        private void RefreshButton_OnClicked(object sender, EventArgs e)
        {
            ((JoinPageViewModel)BindingContext).LoadInvitations();
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

        public void LoadInvitations()
        {
            Items.Clear();
            var locationSender = LocationSenderFactory.GetLocationSender();
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
