using System;
using System.Collections.ObjectModel;
using StayTogether;
using StayTogether.Models;
using WheresChris.Views.Popup;
using Xamarin.Forms;

namespace WheresChris.NotificationCenter
{
    public class InAnotherGroupNotificationResponse
    {
        public static void HandlePersonInAnotherGroup(string phoneNumber, string name)
        {
            var displayName = ContactsHelper.NameOrPhone(phoneNumber, name);
            var items = new ObservableCollection < PopupItem >
            {
                new PopupItem($"End my group and request to join {displayName}", () =>
                {
                    ConfirmEndMyGroupAndJoinAnother(phoneNumber);
                }),
                new PopupItem("Ignore and try to invite them later", null),
            };

            ((App)Xamarin.Forms.Application.Current).ShowPopup(items);
        }

        public static void ConfirmEndMyGroupAndJoinAnother(string phoneNumber)
        {
            //quit my group and join another
            var additionalMemberInvitationVm = new AdditionalMemberInvitationVm
            {
                Group = new GroupVm
                {
                    GroupCreatedDateTime = DateTime.Now,
                    PhoneNumber = phoneNumber
                },
                GroupLeaderPhoneNumber = phoneNumber
            };
            MessagingCenter.Send<MessagingCenterSender, AdditionalMemberInvitationVm>(new MessagingCenterSender(),
                LocationSender.RequestAdditionalMembersJoinGroup, additionalMemberInvitationVm);
        }
    }
}
