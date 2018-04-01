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
            this.BackgroundColor = new Color(0,0,0, 0.1);
            
            InitializeComponent();

            Items = items;

            Device.BeginInvokeOnMainThread(async () =>
            {
                var choices = Items.Select(i => i.Text).ToArray();
                var action = await DisplayActionSheet("Pick One", "Cancel", null, choices);
                if(string.IsNullOrWhiteSpace(action)) return;
                
                var choice = Items.FirstOrDefault(i => i.Text == action);
                if(choice == null) return;
                
                PerformeSelectedAction(choice);
            });
            
        }

        private static void PerformeSelectedAction(PopupItem clickedItem)
        {
            clickedItem?.ClickAction?.Invoke();
            Application.Current.MainPage.Navigation.PopModalAsync(true);
        }
    }
}
