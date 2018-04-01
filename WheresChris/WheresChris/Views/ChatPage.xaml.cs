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
        private LocationSender _locationSender;

        public ChatPage()
        {
            InitializeComponent();
            Title = "Where's Chris - Chat";
            Items = new ObservableCollection<ChatMessageVm>();
            BindingContext = this;
            StartLocationSenderAsync().ConfigureAwait(true);
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

        private async Task StartLocationSenderAsync()
        {
            _locationSender = await LocationSender.GetInstanceAsync();
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


            MessagingCenter.Subscribe<MessagingCenterSender>(this, LocationSender.LeaveGroupMsg, (sender) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Items.Clear();
                });
            });
            MessagingCenter.Subscribe<MessagingCenterSender>(this, LocationSender.EndGroupMsg, (sender) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Items.Clear();
                });
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