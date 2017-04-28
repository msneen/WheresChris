﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.Geolocator;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using StayTogether;
using StayTogether.Classes;
using WheresChris.Helpers;
using Plugin.Settings;
using StayTogether.Group;
#if __ANDROID__
using StayTogether.Droid.Services;
#endif
#if __IOS__
using WheresChris.iOS;
#endif
namespace WheresChris.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InvitePage : ContentPage
    {
        public InvitePage()
        {
			InitializeComponent ();
            BindingContext = new InvitePageViewModel();
            InitializeExpirationPicker();
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
            SettingsHelper.SaveNickname(Nickname.Text);
            
            var userPhoneNumber = SettingsHelper.GetPhoneNumber();
            if (string.IsNullOrWhiteSpace(userPhoneNumber))
            {
                if (!string.IsNullOrWhiteSpace(PhoneNumber.Text))
                {
                    userPhoneNumber = SettingsHelper.SavePhoneNumber(PhoneNumber.Text);
                }
            }

            var selectedGroupMemberVms = GetSelectedGroupMembers();

            var selectedExpirationHours = ExpirationPicker.SelectedItem as ExpirationPickerViewModel;

            var expirationHours = selectedExpirationHours?.Hours ?? 4;

            await StartGroup(selectedGroupMemberVms, userPhoneNumber, expirationHours);
        }

        private List<GroupMemberVm> GetSelectedGroupMembers()
        {
            var invitePageViewModel = BindingContext as InvitePageViewModel;
            if (invitePageViewModel == null) return null;
            List<GroupMemberVm> selectedGroupMemberVms = new List<GroupMemberVm>();
            foreach (var item in invitePageViewModel.Items)
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
            return selectedGroupMemberVms;
        }

        private static async Task StartGroup(List<GroupMemberVm> selectedGroupMemberVms, string userPhoneNumber, int expirationHours)
        {
            if (!selectedGroupMemberVms.Any()) return;
            if (string.IsNullOrWhiteSpace(userPhoneNumber)) return;
            if (expirationHours < 2) return;

            var locationSender = LocationSenderFactory.GetLocationSender();
            var userPosition = await CrossGeolocator.Current.GetLastKnownLocationAsync();
            var groupVm = GroupHelper.InitializeGroupVm(selectedGroupMemberVms, userPosition, userPhoneNumber, expirationHours);
            await locationSender.StartGroup(groupVm);
        }

        protected override async void OnAppearing()
        {
            await ((InvitePageViewModel)BindingContext).InitializeContacts();
            ContactsListView.ItemsSource = ((InvitePageViewModel)BindingContext).Items;

            PhoneNumber.Text = SettingsHelper.GetPhoneNumber();
            Nickname.Text = CrossSettings.Current.GetValueOrDefault<string>("nickname");
            
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
            => ((ListView)sender).SelectedItem = null;

        async void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }


    }

    class ExpirationPickerViewModel
    {
        public string DisplayName { get; set; }
        public int Hours { get; set; }
    }

    class InvitePageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Item> Items { get; set; }
        //public ObservableCollection<Grouping<string, Item>> ItemsGrouped { get; }

        public object ExpirationInHoursIndex { get; set; }

        public InvitePageViewModel()
        {

            RefreshDataCommand = new Command(
                async () => await RefreshData());
        }

        public async Task InitializeContacts()
        {
            Items = await LoadContacts();
        }

        private Task<ObservableCollection<Item>> LoadContacts()
        {
            return Task.Run<ObservableCollection<Item>>(async () =>
            {
                var contactsHelper = new ContactsHelper();
                var contacts = await contactsHelper.GetContacts();
                var itemList = new List<InvitePageViewModel.Item>();
                foreach (var contact in contacts)
                {
                    var item = new InvitePageViewModel.Item
                    {
                        Text = contact.Name,
                        Detail = contact.PhoneNumber
                    };
                    itemList.Add(item);
                }
                return new ObservableCollection<InvitePageViewModel.Item>(itemList);
            });
        }


        public ICommand RefreshDataCommand { get; }

        async Task RefreshData()
        {
            IsBusy = true;
            //Load Data Here
            await Task.Delay(2000);

            IsBusy = false;
        }

        bool busy;
        public bool IsBusy
        {
            get { return busy; }
            set
            {
                busy = value;
                OnPropertyChanged();
                ((Command)RefreshDataCommand).ChangeCanExecute();
            }
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

        public class Grouping<K, T> : ObservableCollection<T>
        {
            public K Key { get; private set; }

            public Grouping(K key, IEnumerable<T> items)
            {
                Key = key;
                foreach (var item in items)
                    this.Items.Add(item);
            }
        }


    }
}
//#if __ANDROID__
//                LocationSenderService.Instance.StartGroup(selectedGroupMemberVms, expirationHours);
//#endif
//#if __IOS__
//                AppDelegate.LocationManager.StartGroup(selectedGroupMemberVms, expirationHours);
//#endif