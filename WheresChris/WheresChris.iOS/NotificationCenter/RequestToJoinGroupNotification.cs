using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.ExternalMaps;
using Plugin.ExternalMaps.Abstractions;
using StayTogether;
using StayTogether.Classes;
using StayTogether.Group;
using StayTogether.iOS.NotificationCenter;
using StayTogether.Models;
using UIKit;
using WheresChris.Helpers;
using WheresChris.NotificationCenter;

namespace WheresChris.iOS.NotificationCenter
{
    public class RequestToJoinGroupNotification : NotificationBase
    {
        public static void RequestToJoinThisGroup(List<GroupMemberSimpleVm> groupmembers)
        {
            if(groupmembers?.Count < 1) return;

            var groupMemberJson = JsonConvert.SerializeObject(groupmembers);
            var memberInfo = groupmembers.Aggregate("", (current, member) => current + $"\n\r {ContactsHelper.NameOrPhone(member.PhoneNumber, member.Name)}");


            var notification = CreateNotification($"The following people would like to join your group: \n\r Swipe to Clear, Click to Invite \n\r {memberInfo}", "Request to join your Group", 10106);
            var dictionary = GetDictionary(notification);

            AddValue("groupmembers", groupMemberJson, ref dictionary);

            notification.UserInfo = dictionary;

            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
        }

        public static List<UIAlertAction> OnNotify(UILocalNotification notification)
        {
            var actions = new List<UIAlertAction>();
            var dictionary = notification.UserInfo;
            var groupMembersJson = GetValue("groupmembers", ref dictionary);
            var groupMembers = JsonConvert.DeserializeObject<List<GroupMemberSimpleVm>>(groupMembersJson);

           
            var okAction =  UIAlertAction.Create("OK", UIAlertActionStyle.Default, alertAction =>
            {
                SendInvitations(groupMembers);
            });
            var ignoreAction =  UIAlertAction.Create("Ignore", UIAlertActionStyle.Default, alertAction =>
            {
                //for now do nothing   
            });
            actions.Add(okAction);
            actions.Add(ignoreAction);
            return actions;
        }

        private static void SendInvitations(List<GroupMemberSimpleVm> groupMembersSimple)
        {
            RequestToJoinGroupNotificationResponse.HandleRequestToJoinMyGroup(groupMembersSimple);
        }
    }
}