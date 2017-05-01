using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XLabs;

namespace WheresChris.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JoinPage : ContentPage
    {
        public JoinPage()
        {
            InitializeComponent();
            BindingContext = new JoinPageViewModel();
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
            => ((ListView)sender).SelectedItem = null;

        async void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            //await DisplayAlert("Selected", e.SelectedItem.ToString(), "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }

    class JoinPageViewModel
    {
        public ObservableCollection<ContactDisplayItemVm> Items { get; }

        public JoinPageViewModel()
        {
            Items = new ObservableCollection<ContactDisplayItemVm>();
            //LoadInvitations();
        }

        private void LoadInvitations()
        {
            var locationSender = LocationSenderFactory.GetLocationSender();
            var invitationList = locationSender.GetInvitations();
            invitationList
                .OrderBy(i => i.ReceivedTime)
                .ToList()
                .ForEach(invitation => Items.Add(new ContactDisplayItemVm
                {
                    Text = invitation.DisplayName()
                }));
        }
    }
}
