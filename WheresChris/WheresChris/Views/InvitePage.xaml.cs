using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.Geolocator;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
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
            Title = "Invite Chris";
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

            var invitePageViewModel = BindingContext as InvitePageViewModel;
            if (invitePageViewModel == null) return;
            var selectedGroupMemberVms = GroupActionsHelper.GetSelectedGroupMembers(invitePageViewModel.Items);

            var selectedExpirationHours = ExpirationPicker.SelectedItem as ExpirationPickerViewModel;

            var expirationHours = selectedExpirationHours?.Hours ?? 4;

            await GroupActionsHelper.StartGroup(selectedGroupMemberVms, userPhoneNumber, expirationHours);


            SetFormEnabled(false);
        }

        private void SetFormEnabled(bool isSelected)
        {
            InviteButton.IsEnabled = isSelected;
            ExpirationPicker.IsEnabled = isSelected;
            ContactsListView.IsEnabled = isSelected;
        }


        protected override async void OnAppearing()
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Contacts);
            if (status != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] {Permission.Contacts});
                status = results[Permission.Contacts];
            }
            if (status == PermissionStatus.Granted)
            {
                await ((InvitePageViewModel) BindingContext).InitializeContacts();
                ContactsListView.ItemsSource = ((InvitePageViewModel) BindingContext).Items;
            }
            PhoneNumber.Text = SettingsHelper.GetPhoneNumber();
            Nickname.Text = CrossSettings.Current.GetValueOrDefault<string>("nickname");
            
        }


    }

    class ExpirationPickerViewModel
    {
        public string DisplayName { get; set; }
        public int Hours { get; set; }
    }

    class InvitePageViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public ObservableCollection<ContactDisplayItemVm> Items { get; set; }
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

    }
}