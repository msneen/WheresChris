using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Plugin.Toasts;
using StayTogether;
using StayTogether.Helpers;
using StayTogether.Models;
using WheresChris.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ChatMessageVm = WheresChris.ViewModels.ChatMessageVm;

namespace WheresChris.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {        

        public ObservableCollection<ChatMessageVm> Items { get; set; }

        public ChatPage()
        {
            InitializeComponent();
            Title = "Where's Chris - Chat";
            Items = new ObservableCollection<ChatMessageVm>();
            BindingContext = this;
            InitializeMessagingCenter();
            ChatMessage.Completed += async (sender, args) => { await ProcessChatMessage(); };
            ChatMessage.TextChanged += async (sender, args) =>
            {
                if(args.NewTextValue.EndsWith(Environment.NewLine))
                {
                    await ProcessChatMessage();
                }
            };
        }

        private void InitializeMessagingCenter()
        {
            MessagingCenter.Subscribe<LocationSender, ChatMessageSimpleVm>(this, LocationSender.ChatReceivedMsg,
                (sender, chatMessageVm) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        Items.Add(new ChatMessageVm
                        {
                            Message = chatMessageVm.Message,
                            Name = ContactsHelper.NameOrPhone(chatMessageVm?.Member?.PhoneNumber, chatMessageVm?.Member?.Name),
                            Member = chatMessageVm?.Member
                        });
                    });
                });


            MessagingCenter.Subscribe<MessagingCenterSender>(this, LocationSender.LeaveGroupMsg, async (sender) =>
            {
               Items.Clear();
            });
            MessagingCenter.Subscribe<MessagingCenterSender>(this, LocationSender.EndGroupMsg, async (sender) =>
            {
                Items.Clear();
            });
        }

        private async Task SendButton_OnClickedButton_OnClicked(object sender, EventArgs e)
        {
            await ProcessChatMessage();
        }

        private async Task ProcessChatMessage()
        {
            var message = ChatMessage.Text.Trim();
            if(string.IsNullOrWhiteSpace(message)) return;

            await ChatHelper.SendChatMessage(message);
            ChatMessage.Text = string.Empty;
        }
    }
}