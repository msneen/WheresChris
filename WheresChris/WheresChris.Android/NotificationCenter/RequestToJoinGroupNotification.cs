using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Newtonsoft.Json;
using StayTogether.Helpers;
using StayTogether.Models;
using WheresChris.Droid;
using WheresChris.NotificationCenter;
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

            NotificationStrategyController.Notify(title, body, NotificationId, notificationIntent);

            void SendNotificationsAction()
            {
                NotificationStrategyController.Cancel(NotificationId);
                RequestToJoinGroupNotificationResponse.HandleRequestToJoinMyGroup(groupmembers);
            }

            ToastHelper.Display(title, body, null, true, SendNotificationsAction);
           
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