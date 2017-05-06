using Android.App;
using Android.Content;
using Java.Lang;
using Plugin.ExternalMaps;
using Plugin.ExternalMaps.Abstractions;
using StayTogether.Classes;
using WheresChris.Droid;

//using StayTogether.Droid.Activities;

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

            var notification = new Notification.Builder(Application.Context)
                .SetSmallIcon(Resource.Drawable.ic_speaker_dark)
                .SetContentTitle($"{ContactsHelper.NameOrPhone(groupMemberVm.PhoneNumber, groupMemberVm.Name)} is lost by {lostDistance} feet")
                .SetContentText("View On Map")
                .SetContentIntent(PendingIntent.GetActivity(Application.Context, 0, notificationIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.OneShot))
                .Build();

            notification.Flags = NotificationFlags.AutoCancel;

            var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager?.Notify(NotificationId, notification);
        }

        public async void OnNotify(Intent intent)
        {
            if (!LostNotification.ShowLostMemberOnMap.Equals(intent.Action)) return;

            //Launch map here.
            var name = intent.GetStringExtra("name");
            var phoneNumber = intent.GetStringExtra("phonenumber");
            var latitude = intent.GetDoubleExtra("latitude", 0);
            var longitude = intent.GetDoubleExtra("longitude", 0);
            var nameOrPhone = ContactsHelper.NameOrPhone(phoneNumber, name);
            await CrossExternalMaps.Current.NavigateTo(nameOrPhone, latitude, longitude, NavigationType.Default);
        }
    }
}