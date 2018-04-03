using Android.App;
using Android.Content;
using Android.Support.V4.App;
using StayTogether.Helpers;
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

            var title = "Someone Left Group";
            var body = $"{displayNameNumber} left your group";

            NotificationStrategyController.Notify(title, body, NotificationId, notificationIntent);

            ToastHelper.Display(title, body);
        }
    }
}