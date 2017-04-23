using System.Collections.Generic;
using UIKit;

namespace StayTogether.iOS.NotificationCenter
{
    public class LeftGroupNotification : NotificationBase
    {
        public static void DisplayGroupInvitationNotification(string phoneNumber, string name)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return;

            var displayNameNumber = ContactsHelper.NameOrPhone(phoneNumber, name);
            var notification = CreateNotification($"{displayNameNumber} left your group", "Someone Left Group", 10103);

            var dictionary = GetDictionary(notification);

            AddValue("Name", name, ref dictionary);
            AddValue("PhoneNumber", phoneNumber, ref dictionary);

            notification.UserInfo = dictionary;

            UIApplication.SharedApplication.ScheduleLocalNotification(notification);

        }

        public static List<UIAlertAction> OnNotify(UILocalNotification notification)
        {
            var actions = new List<UIAlertAction>();
            //var dictionary = notification.UserInfo;
            //var name = GetValue("Name", ref dictionary);
            //var phoneNumber = GetValue("PhoneNumber", ref dictionary);

            var okAction = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null);

            actions.Add(okAction);
            return actions;
        }
    }
}
