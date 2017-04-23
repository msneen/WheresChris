using Android.App;
using Android.Content;
//using StayTogether.Droid.Activities;
using WheresChris.Droid;

namespace StayTogether.Droid.NotificationCenter
{
    public class LeftGroupNotification
    {
        public static readonly int NotificationId = 502;
        public static readonly string MemberLeft = "member_left";

        public static void DisplayLostNotification(string phoneNumber, string name)
        {
            var displayNameNumber = ContactsHelper.NameOrPhone(phoneNumber, name);
            var notificationIntent = new Intent(Application.Context, typeof(MainActivity));
            notificationIntent.SetAction(MemberLeft);
            notificationIntent.SetFlags(ActivityFlags.ReorderToFront);
            notificationIntent.PutExtra("phonenumber", phoneNumber);
            notificationIntent.PutExtra("name", name);

            var notification = new Notification.Builder(Application.Context)
                .SetSmallIcon(Resource.Drawable.ic_speaker_dark)
                .SetContentTitle("Someone Left Group")
                .SetContentText($"{displayNameNumber} left your group")
                .SetContentIntent(PendingIntent.GetActivity(Application.Context, 0, notificationIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.OneShot))
                .Build();

            notification.Flags = NotificationFlags.AutoCancel;

            var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager?.Notify(NotificationId, notification);
        }
    }
}