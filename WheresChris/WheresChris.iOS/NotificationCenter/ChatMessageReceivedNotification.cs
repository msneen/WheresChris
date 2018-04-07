using System.Collections.Generic;
using CloudKit;
using Foundation;
using UIKit;
using UserNotifications;
using WheresChris.Helpers;
using WheresChris.NotificationCenter;
using WheresChris.ViewModels;

namespace StayTogether.iOS.NotificationCenter
{
    public class ChatMessageReceivedNotification : NotificationBase
    {
        public static void DisplayChatMessageReceivedNotification(ChatMessageSimpleVm chatMessageSimpleVm)
        {
            if(chatMessageSimpleVm.Member.PhoneNumber == SettingsHelper.GetPhoneNumber()) return;

            var notification = CreateNotification($"{ContactsHelper.NameOrPhone(chatMessageSimpleVm.Member.PhoneNumber, chatMessageSimpleVm.Name)} sent a chat message.  {chatMessageSimpleVm.Message}", "Chat Message Received", 10107);
            
            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
        }

        public static List<UIAlertAction> OnNotify(UILocalNotification notification)
        {
            var actions = new List<UIAlertAction>();

            var cancelAction = UIAlertAction.Create("Ignore", UIAlertActionStyle.Default, null);
            var viewAction = UIAlertAction.Create("View Chat Message", UIAlertActionStyle.Default, alertAction =>
            {
                ChatMessageReceivedResponse.HandleChatMessageReceived();
            });

            actions.Add(cancelAction);
            actions.Add(viewAction);
            return actions;
        }

        public static void CancelNotification()
        {
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        }
    }
}
