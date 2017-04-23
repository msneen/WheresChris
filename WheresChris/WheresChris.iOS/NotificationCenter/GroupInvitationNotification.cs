using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using WheresChris.iOS;

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
            var joinAction = UIAlertAction.Create("Confirm Joining Group", UIAlertActionStyle.Default, async alertAction =>
            {
                var nameOrPhone = ContactsHelper.NameOrPhone(phoneNumber, name);
                var locationSender = AppDelegate.LocationManager?.LocationSender;
                if (locationSender != null)
                {
                    await locationSender.ConfirmGroupInvitation(phoneNumber, name);
                }
            });

            actions.Add(declineAction);
            actions.Add(joinAction);
            return actions;
        }
    }
}
