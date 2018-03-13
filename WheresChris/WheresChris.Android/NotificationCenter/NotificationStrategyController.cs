using Android.Content;

namespace StayTogether.Droid.NotificationCenter
{
    public interface INotificationStrategy
    {
        void OnNotify(Intent intent);
    }


    public class NotificationStrategyController
    {
        public static INotificationStrategy GetNotificationHandler(Intent intent)
        {
            switch (intent.Action)
            {
                case "show_member_on_map":
                    return new LostNotification();
                case "groupInvitation":
                    return new GroupInvitationNotification();
                case "member_in_another_group":
                    return new InAnotherGroupNotification();
                default:
                    return null;
            }
        }
    }
}