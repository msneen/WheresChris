using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StayTogether.Helpers;
using StayTogether.Models;
using UIKit;
using WheresChris.NotificationCenter;
using Xamarin.Forms;

namespace StayTogether.iOS.NotificationCenter
{
    public class InAnotherGroupNotification : NotificationBase
    {
        public static void DisplayInAnotherGroupNotification(string phoneNumber, string name)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return;
            var displayNameNumber = ContactsHelper.NameOrPhone(phoneNumber, name);

            var body = $"{displayNameNumber} is in another group and can't be added to your group";
            var title = "Invited Person in another Group";
            var notification = CreateNotification(body, title, 10104);

            var dictionary = GetDictionary(notification);

            AddValue("Name", name, ref dictionary);
            AddValue("PhoneNumber", phoneNumber, ref dictionary);

            notification.UserInfo = dictionary;

            UIApplication.SharedApplication.ScheduleLocalNotification(notification);

            //Display a toast as well as the local notification
            void QuitMyGroupAndJoinAnotherAction() => InAnotherGroupNotificationResponse.HandlePersonInAnotherGroup(phoneNumber, name);
            ToastHelper.Display(title, body, null, true, QuitMyGroupAndJoinAnotherAction);
        }

        public static List<UIAlertAction> OnNotify(UILocalNotification notification)
        {
            var actions = new List<UIAlertAction>();
            var dictionary = notification.UserInfo;
            var name = GetValue("Name", ref dictionary);
            var phoneNumber = GetValue("PhoneNumber", ref dictionary);
           
            var okAction = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, alertAction =>
            {
                InAnotherGroupNotificationResponse.ConfirmEndMyGroupAndJoinAnother(phoneNumber);
            });

            actions.Add(okAction);
            return actions;
        }
    }
}