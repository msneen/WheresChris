using Android.App;
using Android.Content;
using StayTogether.Helpers;
using WheresChris.Droid;

namespace StayTogether.Droid.NotificationCenter
{
    public class GroupDisbandedNotification
    {
        public static readonly int NotificationId = 506;
        public static readonly string MemberLeft = "group_disbanded";

        public static void DisplayGroupDisbandedNotification()
        {           
            var notificationIntent = new Intent(Application.Context, typeof(MainActivity));
            notificationIntent.SetAction(MemberLeft);
            notificationIntent.SetFlags(ActivityFlags.ReorderToFront);

            var title = "Group Ended";
            var body = $"Your group has ended.";

            NotificationStrategyController.Notify(title, body, NotificationId, notificationIntent);

            ToastHelper.Display(title, body);
        }
    }
}