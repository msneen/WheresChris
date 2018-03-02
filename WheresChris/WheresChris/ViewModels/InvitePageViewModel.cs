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

        public async Task InitializeContactsAsync(string characters = "")
        {
            var contacts = await LoadContactsAsync(characters);
            if (contacts != null)
            {
                Items = contacts; 
                await RefreshData();
            }
        }

        private static async Task<ObservableCollection<ContactDisplayItemVm>> LoadContactsAsync(string characters = "")
        {
            var contactsHelper = new ContactsHelper();
            return await contactsHelper.GetContactsAsync(characters);
        }


        public ICommand RefreshDataCommand { get; }

        async Task RefreshData()
        {
            IsBusy = true;
            //Load Data Here
            await Task.Delay(100);

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