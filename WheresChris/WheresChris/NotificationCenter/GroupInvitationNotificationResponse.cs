using System.Collections.ObjectModel;
using StayTogether;
using StayTogether.Models;
using WheresChris.Views.Popup;
using Xamarin.Forms;

namespace WheresChris.NotificationCenter
{
    public class GroupInvitationNotificationResponse
    {
        public static void HandleGroupInvitation(string name, string phoneNumber)
        {

            var displayName = ContactsHelper.NameOrPhone(phoneNumber, name);

            var items = new ObservableCollection < PopupItem >
            {
                new PopupItem($"Join {displayName} Group", () => { ConfirmGroupInvitation(name, phoneNumber); }),
                new PopupItem("Ignore The Invitation", null),
            };

            ((App)Xamarin.Forms.Application.Current).ShowPopup(items);
        }

        public static void ConfirmGroupInvitation(string name, string phoneNumber)
        {
            var groupMemberSimpleVm = new GroupMemberSimpleVm
            {
                Name = name,
                PhoneNumber = phoneNumber
            };
            MessagingCenter.Send<MessagingCenterSender, GroupMemberSimpleVm>(new MessagingCenterSender(),
                LocationSender.ConfirmGroupInvitationMsg, groupMemberSimpleVm);
        }
    }
}
