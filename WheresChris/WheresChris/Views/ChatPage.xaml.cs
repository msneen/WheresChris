using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private Interval _resizeInterval = new Interval();

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

        public Task InitializeChat()
        {
            ChatMessage.Focus();
            return Task.CompletedTask;
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
            ExpandListbox();
        }

        private async Task ProcessChatMessage()
        {
            var message = ChatMessage.Text.Trim();
            if(string.IsNullOrWhiteSpace(message)) return;

            await ChatHelper.SendChatMessage(message);
            ChatMessage.Text = string.Empty;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ChatMessage.Focused += InputFocused;
            //ChatMessage.Unfocused += InputUnfocused;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ChatMessage.Focused -= InputFocused;
            //ChatMessage.Unfocused -= InputUnfocused;
        }
        void InputFocused(object sender, EventArgs args)
        {
            ShrinkListbox();
            var last = ChatListView.ItemsSource.Cast<object>().LastOrDefault();
            ChatListView.ScrollTo(last, ScrollToPosition.MakeVisible, true);
        }

        private bool _listboxShrunk;
        private void ShrinkListbox()
        {
            if(_listboxShrunk) return;
            ChatListView.HeightRequest = ChatListView.Height - 360;
            _listboxShrunk = true;
        }

        private void ExpandListbox()
        {
            if(!_listboxShrunk) return;
            ChatListView.HeightRequest = -1;
            _listboxShrunk = false;
        }
    }
}