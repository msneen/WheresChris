using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using StayTogether;
using StayTogether.Group;
using StayTogether.Helpers;
using WheresChris.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WheresChris.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {        

        private LocationSender _locationSender;
        public ObservableCollection<ChatMessageVm> Items { get; set; }

        public ChatPage()
        {
            InitializeComponent();
            Title = "Where's Chris - Chat";
            Items = new ObservableCollection<ChatMessageVm>();

            _locationSender = LocationSender.GetInstance();
            _locationSender.OnChatMessageReceived += (sender, args) =>
            {
                //add chat message to list
                Device.BeginInvokeOnMainThread(()=>{
                    Items.Add(new ChatMessageVm
                    {
                        Message = args.Message,
                        Name = ContactsHelper.NameOrPhone( args?.GroupMember?.PhoneNumber, args?.GroupMember?.Name),
                        Member = args.GroupMember
                    });
                });
            };
            BindingContext = this;

        }

        //                ItemTapped="Handle_ItemTapped"
        //async void Handle_ItemTapped(object sender, SelectedItemChangedEventArgs e)
        //{
        //    if (e.SelectedItem == null)
        //        return;

        //    await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

        //    //Deselect Item
        //    ((ListView)sender).SelectedItem = null;
        //}

        private async void SendButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            var message = ChatMessage.Text.Trim();
            if (string.IsNullOrWhiteSpace(message)) return;

            await ChatHelper.SendChatMessage(message);
            ChatMessage.Text = string.Empty;
        }
    }
}