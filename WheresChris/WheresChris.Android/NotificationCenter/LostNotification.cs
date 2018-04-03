using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Java.Lang;
using Plugin.ExternalMaps;
using Plugin.ExternalMaps.Abstractions;
using StayTogether.Classes;
using StayTogether.Helpers;
using WheresChris.Droid;

namespace StayTogether.Droid.NotificationCenter
{

    public class LostNotification : INotificationStrategy
    {
        public static readonly int NotificationId = 500;
        public static readonly string ShowLostMemberOnMap = "show_member_on_map";

        public static void DisplayLostNotification(GroupMemberVm groupMemberVm)
        {

            var notificationIntent = new Intent(Application.Context, typeof(MainActivity));
            notificationIntent.SetAction(ShowLostMemberOnMap);
            notificationIntent.SetFlags(ActivityFlags.ReorderToFront);
            notificationIntent.PutExtra("phonenumber", groupMemberVm.PhoneNumber);
            notificationIntent.PutExtra("latitude", groupMemberVm.Latitude);
            notificationIntent.PutExtra("longitude", groupMemberVm.Longitude);
            notificationIntent.PutExtra("name", groupMemberVm.Name);

            var lostDistance = Math.Round(groupMemberVm.LostDistance);

            var title = $"{ContactsHelper.NameOrPhone(groupMemberVm.PhoneNumber, groupMemberVm.Name)} is lost by {lostDistance} feet";
            var body = "View On Map";

            NotificationStrategyController.Notify(title, body, NotificationId, notificationIntent);

            void SendNotificationsAction() => ShowLostPersonOnMap(groupMemberVm.PhoneNumber, groupMemberVm.Name, groupMemberVm.Latitude, groupMemberVm.Longitude).ConfigureAwait(true);
            ToastHelper.Display(title, body, null, true, SendNotificationsAction);
            //AsyncHelper.RunSync(() => ToastHelper.Display(title, body, null, true, SendNotificationsAction));
        }

        public async void OnNotify(Intent intent)
        {
            if (!LostNotification.ShowLostMemberOnMap.Equals(intent.Action)) return;

            //Launch map here.
            var name = intent.GetStringExtra("name");
            var phoneNumber = intent.GetStringExtra("phonenumber");
            var latitude = intent.GetDoubleExtra("latitude", 0);
            var longitude = intent.GetDoubleExtra("longitude", 0);
            await ShowLostPersonOnMap(phoneNumber, name, latitude, longitude);
        }

        private static async Task ShowLostPersonOnMap(string phoneNumber, string name, double latitude, double longitude)
        {
            NotificationStrategyController.Cancel(NotificationId);

            var nameOrPhone = ContactsHelper.NameOrPhone(phoneNumber, name);
            await CrossExternalMaps.Current.NavigateTo(nameOrPhone, latitude, longitude, NavigationType.Default);
        }
    }
}