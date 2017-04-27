using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using StayTogether;
using StayTogether.Classes;
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

        private void AddButton_OnClicked(object sender, EventArgs e)
        {
            var addMemberPageViewModel = BindingContext as AddMemberPageViewModel;
            if (addMemberPageViewModel == null) return;
            List<GroupMemberVm> selectedGroupMemberVms = new List<GroupMemberVm>();
            foreach (var item in addMemberPageViewModel.Items)
            {
                if (item.Selected)
                {
                    selectedGroupMemberVms.Add(new GroupMemberVm
                    {
                        Name = item.Text,
                        PhoneNumber = item.Detail
                    });
                }
            }

            //Todo: Create Methods to addToGroup in LocationSender and AppDelegate  
#if __ANDROID__
           // LocationSenderService.Instance.StartGroup(selectedGroupMemberVms, expirationHours);
#endif
#if __IOS__
                //AppDelegate.LocationManager.StartGroup(selectedGroupMemberVms, expirationHours);
#endif
        }
    }



    class AddMemberPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Item> Items { get; set; }

        public AddMemberPageViewModel()
        {
        }

        public async Task InitializeContacts()
        {
            Items = await LoadContacts();
        }

        private Task<ObservableCollection<AddMemberPageViewModel.Item>> LoadContacts()
        {
            return Task.Run<ObservableCollection<AddMemberPageViewModel.Item>>(async () =>
            {
                var contactsHelper = new ContactsHelper();
                var contacts = await contactsHelper.GetContacts();
                var itemList = new List<AddMemberPageViewModel.Item>();
                foreach (var contact in contacts)
                {
                    var item = new AddMemberPageViewModel.Item
                    {
                        Text = contact.Name,
                        Detail = contact.PhoneNumber
                    };
                    itemList.Add(item);
                }
                return new ObservableCollection<AddMemberPageViewModel.Item>(itemList);
            });
        }




        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public class Item
        {
            public string Text { get; set; }
            public string Detail { get; set; }
            public bool Selected { get; set; }

            public override string ToString() => Text;
        }
    }
}
