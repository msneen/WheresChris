using System.Collections.Generic;
using UIKit;
using WheresChris.NotificationCenter;

namespace StayTogether.iOS.NotificationCenter
{
    public class GroupInvitationNotification : NotificationBase
    {
        public static void DisplayGroupInvitationNotification(string phoneNumber, string name)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return;

            var notification = CreateNotification($"{ContactsHelper.NameOrPhone(phoneNumber, name)} invited to you join a group.  Click here to join!", "Group Invitation", 10102);

            var dictionary = GetDictionary(notification);

            AddValue("Name", name, ref dictionary);
            AddValue("PhoneNumber", phoneNumber, ref dictionary);

            notification.UserInfo = dictionary;

            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
        }

        public static List<UIAlertAction> OnNotify(UILocalNotification notification)
        {
            var actions = new List<UIAlertAction>();
            var dictionary = notification.UserInfo;
            var name = GetValue("Name", ref dictionary);
            var phoneNumber = GetValue("PhoneNumber", ref dictionary);


            var declineAction = UIAlertAction.Create("Decline", UIAlertActionStyle.Default, null);
            var joinAction = UIAlertAction.Create("Confirm Joining Group", UIAlertActionStyle.Default, alertAction =>
            {
                GroupInvitationNotificationResponse.ConfirmGroupInvitation(name, phoneNumber);
            });

            actions.Add(declineAction);
            actions.Add(joinAction);
            return actions;
        }
    }
}
