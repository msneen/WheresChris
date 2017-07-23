using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using StayTogether.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WheresChris.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        public ObservableCollection<string> Items { get; set; }

        public ChatPage()
        {
            InitializeComponent();
            Title = "Where's Chris - Chat";

            Items = new ObservableCollection<string>
            {
                "Item 1",
                "Item 2",
                "Item 3",
                "Item 4",
                "Item 5"
            };

            BindingContext = this;
        }

        async void Handle_ItemTapped(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        private async void SendButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            var message = ChatMessage.Text.Trim();
            if (string.IsNullOrWhiteSpace(message)) return;

            await ChatHelper.SendChatMessage(message);
            ChatMessage.Text = string.Empty;
        }
    }
}