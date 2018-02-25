using Android.App;
using Android.Content;
using StayTogether.Droid.Services;
using StayTogether.Models;
using WheresChris.Droid;
using WheresChris.Droid.Services;
using WheresChris.Views;
using Xamarin.Forms;
using Application = Android.App.Application;

namespace StayTogether.Droid.NotificationCenter
{
    public class GroupInvitationNotification: INotificationStrategy
    {
        public static string GroupInvitation = "groupInvitation";
        public static readonly int NotificationId = 501;


        public static void DisplayGroupInvitationNotification(string phoneNumber, string name)
        {

            var notificationIntent = new Intent(Application.Context, typeof(MainActivity));
            notificationIntent.SetAction(GroupInvitation);
            
            notificationIntent.SetFlags(ActivityFlags.ReorderToFront);
            notificationIntent.PutExtra("phonenumber", phoneNumber);
            notificationIntent.PutExtra("name", name);

            var notification = new Notification.Builder(Application.Context)
                .SetSmallIcon(Resource.Drawable.ic_vol_type_speaker_dark)
                .SetContentTitle($"Group Invitation from {ContactsHelper.NameOrPhone(phoneNumber, name)}")
                .SetContentText($"{ContactsHelper.NameOrPhone(phoneNumber, name)} invited to you join a group.  Click here to join!")
                .SetContentIntent(PendingIntent.GetActivity(Application.Context, 0, notificationIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.OneShot))
                .Build();

            notification.Flags = NotificationFlags.AutoCancel;

            var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager?.Notify(NotificationId, notification);
        }


        public void OnNotify(Intent intent)
        {
            if (!GroupInvitation.Equals(intent.Action)) return;

            var name = intent.GetStringExtra("name");
            var phoneNumber = intent.GetStringExtra("phonenumber");

            var groupMemberSimpleVm = new GroupMemberSimpleVm
            {
                Name = name,
                PhoneNumber = phoneNumber
            };
            MessagingCenter.Send<MessagingCenterSender, GroupMemberSimpleVm>(new MessagingCenterSender(), LocationSender.ConfirmGroupInvitationMsg, groupMemberSimpleVm);
        }
    }
}