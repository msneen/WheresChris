using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
//using StayTogether.Droid.Activities;
using StayTogether.Droid.Services;
using WheresChris.Droid;

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
                .SetSmallIcon(Resource.Drawable.ic_speaker_dark)
                .SetContentTitle("Group Invitation")
                .SetContentText($"{ContactsHelper.NameOrPhone(phoneNumber, name)} invited to you join a group.  Click here to join!")
                .SetContentIntent(PendingIntent.GetActivity(Application.Context, 0, notificationIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.OneShot))
                .Build();

            notification.Flags = NotificationFlags.AutoCancel;

            var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager?.Notify(NotificationId, notification);
        }


        public async void OnNotify(Intent intent)
        {
            if (!GroupInvitation.Equals(intent.Action)) return;

            var name = intent.GetStringExtra("name");
            var phoneNumber = intent.GetStringExtra("phonenumber");
            await LocationSenderService.Instance.LocationSender.ConfirmGroupInvitation(phoneNumber, name);
        }
    }
}