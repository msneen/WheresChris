using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Newtonsoft.Json;
using StayTogether.Classes;
using StayTogether.Helpers;
using StayTogether.Models;
using WheresChris.Droid;
using WheresChris.Helpers;
using WheresChris.NotificationCenter;
using Xamarin.Forms;
using Application = Android.App.Application;

namespace StayTogether.Droid.NotificationCenter
{
    public class RequestToJoinGroupNotification : INotificationStrategy
    {
        public static readonly int NotificationId = 504;
        public static readonly string RequestToJoinThisGroupReceived = "request_to_join_this_group";

        public static void RequestToJoinThisGroup(List<GroupMemberSimpleVm> groupmembers)
        {
            var notificationIntent = new Intent(Application.Context, typeof(MainActivity));
            notificationIntent.SetAction(RequestToJoinThisGroupReceived);
            notificationIntent.SetFlags(ActivityFlags.ReorderToFront);

            var groupMemberJson = JsonConvert.SerializeObject(groupmembers);
            notificationIntent.PutExtra("groupmembers", groupMemberJson);

            var title = "Request to join your Group";
            var body = $"People would like to join your group:";

            var notification = new Notification.Builder(Application.Context)
                .SetSmallIcon(Resource.Drawable.ic_vol_type_speaker_dark)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetContentIntent(PendingIntent.GetActivity(Application.Context, 0, notificationIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.OneShot))
                .Build();

            notification.Flags = NotificationFlags.AutoCancel;

            var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager?.Notify(NotificationId, notification);

            void SendNotificationsAction() => RequestToJoinGroupNotificationResponse.HandleRequestToJoinMyGroup(groupmembers);
            ToastHelper.Display(title, body, null, true, SendNotificationsAction).ConfigureAwait(true);
        }

        public void OnNotify(Intent intent)
        {
            if (!RequestToJoinThisGroupReceived.Equals(intent.Action)) return;
            var groupMembersJson = intent.GetStringExtra("groupmembers");

            var groupMembers = JsonConvert.DeserializeObject<List<GroupMemberSimpleVm>>(groupMembersJson);

            RequestToJoinGroupNotificationResponse.HandleRequestToJoinMyGroup(groupMembers);
        }
    }
}