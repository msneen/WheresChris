using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WheresChris.Views.Popup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Popup : ContentPage
    {
        public ObservableCollection<PopupItem> Items { get; set; }

        public Popup(ObservableCollection<PopupItem> items)
        {
            InitializeComponent();

            Items = items;

            //Remove this example after debugging
            if(items == null)
            {
                Items = new ObservableCollection<PopupItem>
                {
                    new PopupItem("Text1", async () => { await DisplayAlert("Item Tapped", "An item was tapped.", "OK"); }), //Remove this example after debugging
                    new PopupItem("Text2", () => { Debug.WriteLine("Text2"); }),
                    new PopupItem("Text3", () => { Debug.WriteLine("Text3"); }),
                };
            }

            MyListView.ItemsSource = Items;
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            //await DisplayAlert("Item Tapped", "An item was tapped.", "OK");//remove me

            var clickedItem = (PopupItem) e.Item;
            clickedItem?.ClickAction?.Invoke();

            //Deselect Item
            ((ListView)sender).SelectedItem = null;

            Application.Current.MainPage.Navigation.PopModalAsync(true);
        }
    }
}
