using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using StayTogether;
using StayTogether.Classes;
using WheresChris.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


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
            await InitializeContactsAsync();
        }

        public async Task InitializeContactsAsync(string characters = "")
        {
            ContactsListView.ItemsSource = null;
            await ((AddMemberPageViewModel) BindingContext).InitializeContacts(characters);
            ContactsListView.ItemsSource = ((AddMemberPageViewModel) BindingContext).Items;
        }

        private async Task ViewInviteeList()
        {
            SearchEntry.Text = "";
            ContactsListView.ItemsSource = null;
            await ((AddMemberPageViewModel) BindingContext).AddToGroup();
            ContactsListView.SetBinding(ListView.ItemsSourceProperty, "Items", BindingMode.TwoWay);
        }

        private async void AddButton_OnClicked(object sender, EventArgs e)
        {
            if (!(BindingContext is AddMemberPageViewModel addMemberPageViewModel)) return;

            var selectedGroupMemberVms = await ((AddMemberPageViewModel)BindingContext).AddToGroup(); //GroupActionsHelper.GetSelectedGroupMembers(addMemberPageViewModel.Items);
            var userPhoneNumber = SettingsHelper.GetPhoneNumber();
            await GroupActionsHelper.StartOrAddToGroup(selectedGroupMemberVms, userPhoneNumber);
        }
        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            if (ContactsListView.IsEnabled)
            {
                var selectedColor = Color.LightSkyBlue;
                object themeColor;
                if (Application.Current.Resources.TryGetValue("ListViewSelectedColor", out themeColor))
                {
                    selectedColor = (Color)themeColor;
                }
                var contactDisplayItemVm = (ContactDisplayItemVm)((RelativeLayout)sender).BindingContext;
                contactDisplayItemVm.Selected = !contactDisplayItemVm.Selected;
                var color = contactDisplayItemVm.Selected ? selectedColor : ContactsListView.BackgroundColor;
                ((RelativeLayout)sender).BackgroundColor = color;

                var action = ((AddMemberPageViewModel)BindingContext).AddSelectedContact(contactDisplayItemVm);
                if(action == InvitePageViewModel.InviteeAction.Removed)
                {
                    ViewInviteeList().ConfigureAwait(true);
                }
            }
        }

        private async Task SearchEntry_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if(e.NewTextValue.Length == 10)
            {
                HandlePhoneNumber(e.NewTextValue);
            }
            else if(e.NewTextValue.Length == 0 || e.NewTextValue.Length > 2)
            {
                await InitializeContactsAsync(e.NewTextValue);
            }
        }

        private void AddButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            var phoneNumber = SearchEntry.Text;
            ((AddMemberPageViewModel)BindingContext).AddPhoneNumbeContact(phoneNumber);
            AddButton.IsEnabled = false;
            SearchEntry.Text = "";
        }

        private void HandlePhoneNumber(string text)
        {
            AddPhoneButton.IsEnabled = text.IsNumeric();
        }

        private async Task ContactsListButton_OnClicked(object sender, EventArgs e)
        {
            SearchEntry.Text = "";
            ContactsListView.ItemsSource = null;
            await InitializeContactsAsync();
            ContactsListView.SetBinding(ListView.ItemsSourceProperty, "Items", BindingMode.TwoWay);
        }

        private async Task InviteListButton_OnClicked(object sender, EventArgs e)
        {
            await ViewInviteeList();
        }
    }


    public class AddMemberPageViewModel : INotifyPropertyChanged
    {
        public AddMemberPageViewModel()
        {
            RefreshDataCommand = new Command(
                async () => await RefreshData());
        }

        private readonly List<ContactDisplayItemVm> _selectedContacts = new List<ContactDisplayItemVm>();
        public string Title { get; set; } = "Add Members";
        public ObservableCollection<ContactDisplayItemVm> Items { get; set; }

        public async Task InitializeContacts(string characters = "")
        {
            var contactsHelper = new ContactsHelper();
            Items = await contactsHelper.GetContactsAsync(characters);

        }

        public InvitePageViewModel.InviteeAction AddSelectedContact(ContactDisplayItemVm contactDisplayItemVm)
        {
            if(contactDisplayItemVm.Selected)
            {
                _selectedContacts.Add(contactDisplayItemVm);
                return InvitePageViewModel.InviteeAction.Added;
            }
            else
            {
                var contact = _selectedContacts.FirstOrDefault(x => x.PhoneNumber == contactDisplayItemVm.PhoneNumber);
                if(contact == null) return InvitePageViewModel.InviteeAction.None;
                _selectedContacts.Remove(contact);
                return InvitePageViewModel.InviteeAction.Removed;
            }           
        }

        public async Task<List<GroupMemberVm>> AddToGroup()
        {
            if(_selectedContacts == null) return null;

            await LoadGroupContacts(_selectedContacts);

            var selectedGroupMemberVms = GroupActionsHelper.GetSelectedGroupMembers(_selectedContacts);
            return selectedGroupMemberVms;
        }

        public Task LoadGroupContacts(IList<ContactDisplayItemVm> groupContacts)
        {
            IsBusy = true;
            if (groupContacts == null) return Task.CompletedTask;
            var contactCollection = new ObservableCollection<ContactDisplayItemVm>(groupContacts);
            Items = contactCollection;
            IsBusy = false;
            return Task.CompletedTask;
        }

        public void AddPhoneNumbeContact(string phoneNumber)
        {
            if(string.IsNullOrWhiteSpace(phoneNumber)) return;
            if(!ContactsHelper.IsValidPhoneNumber(phoneNumber)) return;
            _selectedContacts.Add(new ContactDisplayItemVm
            {
                PhoneNumber = phoneNumber
            });
        }

        public ICommand RefreshDataCommand { get; }

        async Task RefreshData()
        {
            IsBusy = true;
            //Load Data Here
            await InitializeContacts();

            IsBusy = false;
        }

        private bool _busy;
        public bool IsBusy
        {
            get { return _busy; }
            set
            {
                _busy = value;
                OnPropertyChanged();
                ((Command)RefreshDataCommand).ChangeCanExecute();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
