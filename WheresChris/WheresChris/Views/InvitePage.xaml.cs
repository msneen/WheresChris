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

namespace WheresChris.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InvitePage : ContentPage
    {
        public InvitePage()
        {
			InitializeComponent ();
            BindingContext = new InvitePageViewModel();
        }

        public void StartGroup(object sender, EventArgs e)
        {
            var invitePageViewModel = BindingContext as InvitePageViewModel;
            if (invitePageViewModel == null) return;
            foreach (var item in invitePageViewModel.Items)
            {
                if (item.Selected)
                {
                    
                }
            }
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



    class InvitePageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Item> Items { get; }
        public ObservableCollection<Grouping<string, Item>> ItemsGrouped { get; }

        public InvitePageViewModel()
        {
            Items = new ObservableCollection<Item>(new[]
            {
                new Item { Text = "Mike Sneen", Detail = "619-928-4340" },
                new Item { Text = "Sonny Garcia", Detail = "619-222-4341" },
                new Item { Text = "Dave Maynard", Detail = "760-338-2842" },
                new Item { Text = "Joe Smith", Detail = "380-282-3732" },
                new Item { Text = "Wayne Wilson", Detail= "760-322-2800" },
                new Item { Text = "Don Walker", Detail = "619-233-7247" },
                new Item { Text = "Tyler Smith", Detail = "619-322-4832" },
            });

            var sorted = from item in Items
                         orderby item.Text
                         group item by item.Text[0].ToString() into itemGroup
                         select new Grouping<string, Item>(itemGroup.Key, itemGroup);

            ItemsGrouped = new ObservableCollection<Grouping<string, Item>>(sorted);

            RefreshDataCommand = new Command(
                async () => await RefreshData());
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
