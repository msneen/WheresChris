using Android.App;
using Android.Content;
using WheresChris.Droid;

//using StayTogether.Droid.Activities;

namespace StayTogether.Droid.NotificationCenter
{
    public class InAnotherGroupNotification
    {
        public static readonly int NotificationId = 503;
        public static readonly string MemberInAnotherGroup = "member_in_another_group";

        public static void DisplayInAnotherGroupNotification(string phoneNumber, string name)
        {
            var displayNameNumber = ContactsHelper.NameOrPhone(phoneNumber, name);
            var notificationIntent = new Intent(Application.Context, typeof(MainActivity));
            notificationIntent.SetAction(MemberInAnotherGroup);
            notificationIntent.SetFlags(ActivityFlags.ReorderToFront);
            notificationIntent.PutExtra("phonenumber", phoneNumber);
            notificationIntent.PutExtra("name", name);

            var notification = new Notification.Builder(Application.Context)
                .SetSmallIcon(Resource.Drawable.ic_speaker_dark)
                .SetContentTitle("Invited Person in another Group")
                .SetContentText($"{displayNameNumber} is in another group and can't be added to your group")
                .SetContentIntent(PendingIntent.GetActivity(Application.Context, 0, notificationIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.OneShot))
                .Build();

            notification.Flags = NotificationFlags.AutoCancel;

            var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager?.Notify(NotificationId, notification);
        }
    }
}