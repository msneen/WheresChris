using Android.App;
using Android.Content;
using StayTogether.Helpers;
using WheresChris.Droid;
using WheresChris.NotificationCenter;
using Application = Android.App.Application;

namespace StayTogether.Droid.NotificationCenter
{
    public class InAnotherGroupNotification : INotificationStrategy
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

            var title = "Invited Person in another Group";
            var body = $"{displayNameNumber} is in another group";

            var notification = new Notification.Builder(Application.Context)
                .SetSmallIcon(Resource.Drawable.ic_vol_type_speaker_dark)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetContentIntent(PendingIntent.GetActivity(Application.Context, 0, notificationIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.OneShot))
                .Build();

            notification.Flags = NotificationFlags.AutoCancel;

            var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager?.Notify(NotificationId, notification);

            //Display a toast as well as the local notification
            void QuitMyGroupAndJoinAnotherAction() => InAnotherGroupNotificationResponse.HandlePersonInAnotherGroup(phoneNumber, name);
            ToastHelper.Display(title, body, null, true, QuitMyGroupAndJoinAnotherAction).ConfigureAwait(true);
        }

        public void OnNotify(Intent intent)
        {
            if (!MemberInAnotherGroup.Equals(intent.Action)) return;

            var name = intent.GetStringExtra("name");
            var phoneNumber = intent.GetStringExtra("phonenumber");

            InAnotherGroupNotificationResponse.HandlePersonInAnotherGroup(phoneNumber, name);
        }
    }
}