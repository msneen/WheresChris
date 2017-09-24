﻿using System;
using System.Collections.Generic;
using System.Text;
using StayTogether.Models;
using UIKit;
using WheresChris.iOS;
using Xamarin.Forms;

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
                var nameOrPhone = ContactsHelper.NameOrPhone(phoneNumber, name);

                var groupMemberSimpleVm = new GroupMemberSimpleVm
                {
                    Name = name,
                    PhoneNumber = phoneNumber
                };
                MessagingCenter.Send<MessagingCenterSender, GroupMemberSimpleVm>(new MessagingCenterSender(), LocationSender.ConfirmGroupInvitationMsg, groupMemberSimpleVm);
            });

            actions.Add(declineAction);
            actions.Add(joinAction);
            return actions;
        }
    }
}
