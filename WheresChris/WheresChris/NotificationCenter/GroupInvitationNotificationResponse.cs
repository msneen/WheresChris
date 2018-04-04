using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using StayTogether;
using StayTogether.Droid.NotificationCenter;
using StayTogether.Models;
using WheresChris.Views.Popup;
using Xamarin.Forms;

namespace WheresChris.NotificationCenter
{
    public class GroupInvitationNotificationResponse
    {
        public static void HandleGroupInvitation(string name, string phoneNumber)
        {
            NotificationStrategyController.Cancel(GroupInvitationNotification.NotificationId);

            var displayName = ContactsHelper.NameOrPhone(phoneNumber, name);

            var items = new ObservableCollection < PopupItem >
            {
                new PopupItem($"Join {displayName} Group", () =>
                {
                    var groupMemberSimpleVm = new GroupMemberSimpleVm
                    {
                        Name = name,
                        PhoneNumber = phoneNumber
                    };
                    MessagingCenter.Send<MessagingCenterSender, GroupMemberSimpleVm>(new MessagingCenterSender(),
                        LocationSender.ConfirmGroupInvitationMsg, groupMemberSimpleVm);
                }),
                new PopupItem("Ignore The Invitation", null),
            };

            ((App)Xamarin.Forms.Application.Current).ShowPopup(items);
        }
    }
}
