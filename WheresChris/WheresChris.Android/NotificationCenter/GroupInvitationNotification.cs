using Android.App;
using Android.Content;
using Android.Support.V4.App;
using StayTogether.Droid.Services;
using StayTogether.Helpers;
using StayTogether.Models;
using WheresChris.Droid;
using WheresChris.Droid.Services;
using WheresChris.NotificationCenter;
using WheresChris.Views;
using Xamarin.Forms;
using Application = Android.App.Application;

namespace StayTogether.Droid.NotificationCenter
{
    public class GroupInvitationNotification: INotificationStrategy
    {
        public static string GroupInvitation = "groupInvitation";
        public static readonly int NotificationId = 501;

        private static readonly Interval _debounceInterval = new Interval();
        private static string _lastTitle = "";
        private static string _lastBody = "";

        public static void DisplayGroupInvitationNotification(string phoneNumber, string name)
        {

            var notificationIntent = new Intent(Application.Context, typeof(MainActivity));
            notificationIntent.SetAction(GroupInvitation);
            
            notificationIntent.SetFlags(ActivityFlags.ReorderToFront);
            notificationIntent.PutExtra("phonenumber", phoneNumber);
            notificationIntent.PutExtra("name", name);

            var title = $"Group Invitation from {ContactsHelper.NameOrPhone(phoneNumber, name)}";
            var body = $"{ContactsHelper.NameOrPhone(phoneNumber, name)} invited to you join a group.  Click here to join!";

            if(DebounceNotification(title, body)) return;

            NotificationStrategyController.Notify(title, body, NotificationId, notificationIntent);

            void SendNotificationsAction() => ConfirmInvitation(name, phoneNumber);
            ToastHelper.Display(title, body, null, true, SendNotificationsAction);          
        }


        public void OnNotify(Intent intent)
        {
            if (!GroupInvitation.Equals(intent.Action)) return;

            var name = intent.GetStringExtra("name");
            var phoneNumber = intent.GetStringExtra("phonenumber");

            ConfirmInvitation(name, phoneNumber);
        }

        private static void ConfirmInvitation(string name, string phoneNumber)
        {
            NotificationStrategyController.Cancel(GroupInvitationNotification.NotificationId);
            GroupInvitationNotificationResponse.HandleGroupInvitation(name, phoneNumber);
        }

        private static bool DebounceNotification(string title, string body)
        {
            if(_lastTitle == title && body == _lastBody) return true;
            _lastTitle = title;
            _lastBody = body;
            _debounceInterval.SetInterval(() =>
            {
                _lastTitle = "";
                _lastBody = "";
            }, 60000);
            
            return false;
        }
    }
}