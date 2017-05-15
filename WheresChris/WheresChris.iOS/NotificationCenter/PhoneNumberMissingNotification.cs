using System;
using System.Collections.Generic;
using System.Text;
using StayTogether.iOS.NotificationCenter;
using UIKit;

namespace WheresChris.iOS.NotificationCenter
{
    public class PhoneNumberMissingNotification : NotificationBase
    {
        public static void DisplayGroupInvitationNotification()
        {
            var notification = CreateNotification($"Please enter your phone number on the main pmge", "Your Phone Number is Invalid", 10105);

            var dictionary = GetDictionary(notification);

            notification.UserInfo = dictionary;

            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
        }

        public static List<UIAlertAction> OnNotify(UILocalNotification notification)
        {
            var actions = new List<UIAlertAction>();

            var okAction = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null);

            actions.Add(okAction);
            return actions;
        }
    }
}
