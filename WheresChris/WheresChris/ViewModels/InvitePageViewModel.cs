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

namespace WheresChris.Views
{
    public class InvitePageViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Name { get; set; }

        public ObservableCollection<ContactDisplayItemVm> Items { get; set; }

        private readonly List<ContactDisplayItemVm> _selectedContacts = new List<ContactDisplayItemVm>();

        public object ExpirationInHoursIndex { get; set; }

        public enum InviteeAction
        {
            None,
            Added,
            Removed
        }

        public InvitePageViewModel()
        {
            Title = "Where's Chris - Invite";
            Name = "Invite";
            RefreshDataCommand = new Command(
                async () => await RefreshData());
        }

        public bool IsEnabled { get; set; } = true;

        public Task LoadGroupContacts(IList<ContactDisplayItemVm> groupContacts)
        {
            IsBusy = true;
            if (groupContacts == null) return Task.CompletedTask;
            var contactCollection = new ObservableCollection<ContactDisplayItemVm>(groupContacts);
            Items = contactCollection;
            IsBusy = false;
            return Task.CompletedTask;
        }

        public async Task InitializeContactsAsync(string characters = "")
        {
            if(!IsEnabled) return;
            IsBusy = true;
            var contacts = await LoadContactsAsync(characters);
            if (contacts != null)
            {
                Items = contacts; 
            }
            IsBusy = false;
        }

        private static async Task<ObservableCollection<ContactDisplayItemVm>> LoadContactsAsync(string characters = "")
        {
            var contactsHelper = new ContactsHelper();
            return await contactsHelper.GetContactsAsync(characters);
        }

        public void AddPhoneNumbeContact(string phoneNumber)
        {
            if(!IsEnabled) return;
            if(string.IsNullOrWhiteSpace(phoneNumber)) return;
            if(!ContactsHelper.IsValidPhoneNumber(phoneNumber)) return;
            _selectedContacts.Add(new ContactDisplayItemVm
            {
                PhoneNumber = phoneNumber
            });
        }

        public async Task<List<GroupMemberVm>> AddToGroup()
        {
            if(!IsEnabled) return null; 
            if(_selectedContacts == null) return null;

            await LoadGroupContacts(_selectedContacts);

            var selectedGroupMemberVms = GroupActionsHelper.GetSelectedGroupMembers(_selectedContacts);
            return selectedGroupMemberVms;
        }

        public InviteeAction AddSelectedContact(ContactDisplayItemVm contactDisplayItemVm)
        {
            if(!IsEnabled) return InviteeAction.None;
            if(contactDisplayItemVm.Selected)
            {
                _selectedContacts.Add(contactDisplayItemVm);
                return InviteeAction.Added;
            }
            else
            {
                var contact = _selectedContacts.FirstOrDefault(x => x.PhoneNumber == contactDisplayItemVm.PhoneNumber);
                if(contact == null) return InviteeAction.None;
                _selectedContacts.Remove(contact);
                return InviteeAction.Removed;
            }           
        }


        public ICommand RefreshDataCommand { get; }

        async Task RefreshData()
        {
            IsBusy = true;
            //Load Data Here
            await LoadContactsAsync();

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
        void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}