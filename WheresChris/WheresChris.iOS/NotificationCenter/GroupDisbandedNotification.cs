using System.Collections.Generic;
using UIKit;

namespace StayTogether.iOS.NotificationCenter
{
    public class GroupDisbandedNotification: NotificationBase
    {
        public static void DisplayGroupDisbandedNotification()
        {
            var notification = CreateNotification($"Your group has ended.", "Group Ended", 10108);
            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
        }

        public static List<UIAlertAction> OnNotify(UILocalNotification notification)
        {
            var actions = new List<UIAlertAction>();
            var okAction = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null);

            actions.Add(okAction);
            return actions;
        }
    }
}
