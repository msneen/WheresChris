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

namespace WheresChris.Views.GroupViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MemberPage : ContentPage
    {
        public MemberPage()
        {
			InitializeComponent ();
            BindingContext = new MemberPageViewModel();
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
            => ((ListView)sender).SelectedItem = null;

        async void Handle_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            await DisplayAlert("Selected", e.SelectedItem.ToString(), "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }



    class MemberPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Item> Items { get; }

        public MemberPageViewModel()
        {
            Items = new ObservableCollection<Item>(new[]
            {
                new Item { Text = "Michael Smith" },
                new Item { Text = "Bill Johnson" },
                new Item { Text = "Craig Jones"},
                new Item { Text = "Steven Howard"},
                new Item { Text = "Rachel Jefferson"},
            });

        }


        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public class Item
        {
            public string Text { get; set; }
            public string Detail { get; set; }

            public override string ToString() => Text;
        }

    }
}
