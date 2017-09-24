using System;
using System.Collections.ObjectModel;
using StayTogether;
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
            BindingContext = this;
            InitializeMessagingCenter();
        }

        private void InitializeMessagingCenter()
        {
            MessagingCenter.Subscribe<LocationSender, ChatMessageSimpleVm>(this, LocationSender.ChatReceivedMsg,
                (sender, chatMessageVm) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Items.Add(new ChatMessageVm
                        {
                            Message = chatMessageVm.Message,
                            Name = ContactsHelper.NameOrPhone(chatMessageVm?.Member?.PhoneNumber, chatMessageVm?.Member?.Name),
                            Member = chatMessageVm?.Member
                        });
                    });
                });
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