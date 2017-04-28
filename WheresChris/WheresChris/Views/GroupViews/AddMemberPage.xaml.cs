using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using StayTogether;
using StayTogether.Classes;
using WheresChris.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
#if __ANDROID__
using StayTogether.Droid.Services;
#endif
#if __IOS__
using WheresChris.iOS;
#endif

namespace WheresChris.Views.GroupViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddMemberPage : ContentPage
    {
        public AddMemberPage()
        {
			InitializeComponent ();
            BindingContext = new AddMemberPageViewModel();
        }

        protected override async void OnAppearing()
        {
            await ((AddMemberPageViewModel)BindingContext).InitializeContacts();
            ContactsListView.ItemsSource = ((AddMemberPageViewModel)BindingContext).Items;

        }

        private async void AddButton_OnClicked(object sender, EventArgs e)
        {
            var addMemberPageViewModel = BindingContext as AddMemberPageViewModel;
            if (addMemberPageViewModel == null) return;
            var selectedGroupMemberVms = GroupActionsHelper.GetSelectedGroupMembers(addMemberPageViewModel.Items);
            var userPhoneNumber = SettingsHelper.GetPhoneNumber();
            await GroupActionsHelper.StartOrAddToGroup(selectedGroupMemberVms, userPhoneNumber);
        }
    }



    class AddMemberPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ContactDisplayItemVm> Items { get; set; }

        public AddMemberPageViewModel()
        {
        }

        public async Task InitializeContacts()
        {
            Items = await LoadContacts();
        }

        private Task<ObservableCollection<ContactDisplayItemVm>> LoadContacts()
        {
            return Task.Run<ObservableCollection<ContactDisplayItemVm>>(async () =>
            {
                var contactsHelper = new ContactsHelper();
                var contacts = await contactsHelper.GetContacts();
                var itemList = new List<ContactDisplayItemVm>();
                foreach (var contact in contacts)
                {
                    var item = new ContactDisplayItemVm
                    {
                        Text = contact.Name,
                        Detail = contact.PhoneNumber
                    };
                    itemList.Add(item);
                }
                return new ObservableCollection<ContactDisplayItemVm>(itemList);
            });
        }




        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


    }
}
