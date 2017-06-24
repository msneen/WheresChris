using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using StayTogether;
using Xamarin.Forms;

namespace WheresChris.Views
{
    public class InvitePageViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public ObservableCollection<ContactDisplayItemVm> Items { get; set; }

        public object ExpirationInHoursIndex { get; set; }

        public InvitePageViewModel()
        {
            Title = "Where's Chris - Invite";
            Name = "Invite";
            RefreshDataCommand = new Command(
                async () => await RefreshData());
        }

        public async Task InitializeContactsAsync()
        {
            var contacts = await LoadContactsAsync();
            if (contacts != null)
            {
                Items = contacts;                
            }
        }

        private static async Task<ObservableCollection<ContactDisplayItemVm>> LoadContactsAsync()
        {
            var contactsHelper = new ContactsHelper();
            return await contactsHelper.GetContactsAsync();
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