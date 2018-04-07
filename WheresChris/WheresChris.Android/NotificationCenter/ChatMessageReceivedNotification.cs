using Android.App;
using Android.Content;
using StayTogether.Helpers;
using WheresChris.Droid;
using WheresChris.Helpers;
using WheresChris.NotificationCenter;
using WheresChris.ViewModels;

namespace StayTogether.Droid.NotificationCenter
{
    public class ChatMessageReceivedNotification: INotificationStrategy
    {
        public static string ChatMessage = "chatmessage";
        public static readonly int NotificationId = 505;

        public static void DisplayChatReceivedNotification(ChatMessageSimpleVm chatMessageSimpleVm)
        {
            if(chatMessageSimpleVm.Member.PhoneNumber == SettingsHelper.GetPhoneNumber()) return;

            var notificationIntent = new Intent(Application.Context, typeof(MainActivity));
            notificationIntent.SetAction(ChatMessage);
            
            notificationIntent.SetFlags(ActivityFlags.ReorderToFront);
            notificationIntent.PutExtra("phonenumber", chatMessageSimpleVm.Member.PhoneNumber);
            notificationIntent.PutExtra("name", chatMessageSimpleVm.Name);

            var title = $"Chat Message from {ContactsHelper.NameOrPhone(chatMessageSimpleVm.Member.PhoneNumber, chatMessageSimpleVm.Name)}";
            var body = $"{ContactsHelper.NameOrPhone(chatMessageSimpleVm.Member.PhoneNumber, chatMessageSimpleVm.Name)} sent a chat message. {chatMessageSimpleVm.Message}";

            NotificationStrategyController.Notify(title, body, NotificationId, notificationIntent);

            void ViewChatMessageAction() => ViewChatMessage();
            ToastHelper.Display(title, body, null, true, ViewChatMessageAction);          
        }

        public void OnNotify(Intent intent)
        {
            if (!ChatMessage.Equals(intent.Action)) return;

            ChatMessageReceivedResponse.HandleChatMessageReceived();
        }

        private static void ViewChatMessage()
        {
            ChatMessageReceivedResponse.HandleChatMessageReceived();
        }

        public static void CancelNotification()
        {
            var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager?.Cancel(NotificationId);
        }
    }
}