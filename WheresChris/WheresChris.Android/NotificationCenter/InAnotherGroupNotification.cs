using Android.App;
using Android.Content;
using Android.Support.V4.App;
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

            NotificationStrategyController.Notify(title, body, NotificationId, notificationIntent);

            //Display a toast as well as the local notification
            void QuitMyGroupAndJoinAnotherAction() => InAnotherGroupNotificationResponse.HandlePersonInAnotherGroup(phoneNumber, name);
            ToastHelper.Display(title, body, null, true, QuitMyGroupAndJoinAnotherAction);
            //AsyncHelper.RunSync(() => ToastHelper.Display(title, body, null, true, QuitMyGroupAndJoinAnotherAction));            
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