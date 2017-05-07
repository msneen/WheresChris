using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using StayTogether;
using Xamarin.Forms;

namespace WheresChris.Views
{
    class InvitePageViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public ObservableCollection<ContactDisplayItemVm> Items { get; set; }

        public object ExpirationInHoursIndex { get; set; }

        public InvitePageViewModel()
        {

            RefreshDataCommand = new Command(
                async () => await RefreshData());
        }

        public async Task InitializeContacts()
        {
            var contacts = await LoadContacts();
            if (contacts != null)
            {
                Items = contacts;
            }
        }

        private Task<ObservableCollection<ContactDisplayItemVm>> LoadContacts()
        {
            return Task.Run(async () =>
            {
                var contactsHelper = new ContactsHelper();
                var contacts = await contactsHelper.GetContacts();
                if (contacts == null) return null;

                var itemList = contacts.Select(contact => new ContactDisplayItemVm
                {
                    Text = contact.Name,
                    Detail = contact.PhoneNumber
                }).ToList();
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