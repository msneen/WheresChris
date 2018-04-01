using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using StayTogether.Extensions;
using WheresChris.Droid;
using Xamarin.Forms;
using Application = Android.App.Application;

namespace StayTogether.Droid.NotificationCenter
{
    public interface INotificationStrategy
    {
        void OnNotify(Intent intent);
    }


    public class NotificationStrategyController
    {
        public static readonly string ChannelId = "channel_01";
        public static readonly string ChannelName = "Channel 1";
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
                case "request_to_join_this_group":
                    return new RequestToJoinGroupNotification();
                default:
                    return null;
            }
        }

        public static void Notify(string title, string body, int notificationId, Intent notificationIntent)
        {
            var color = (Color) Xamarin.Forms.Application.Current.Resources["Primary"];
            var notification = new NotificationCompat.Builder(Application.Context, ChannelId)
                .SetSmallIcon(Resource.Drawable.ic_vol_type_speaker_dark)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetPriority((int) NotificationPriority.High)
                .SetDefaults((int) NotificationDefaults.All)
                .SetColor(Android.Graphics.Color.ParseColor(color.GetHexString()))
                .SetContentIntent(PendingIntent.GetActivity(Application.Context, 0, notificationIntent,
                    PendingIntentFlags.UpdateCurrent | PendingIntentFlags.OneShot))
                .Build();

            notification.Flags = NotificationFlags.AutoCancel;

            var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            NotificationStrategyController.SetChannelForOreoandAbove(notificationManager, ChannelId, ChannelName);

            notificationManager?.Notify(notificationId, notification);
        }

        public static void SetChannelForOreoandAbove(NotificationManager notificationManager, string channelId, string channelName)
        {
            try
            {
                if(Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                {
                    /* Create or update. */
                    NotificationChannel channel = new NotificationChannel(channelId,
                        channelName,
                        NotificationImportance.Default);
                    channel.EnableLights(true);
                    channel.EnableVibration(true);
                    channel.SetShowBadge(true);
                    notificationManager.CreateNotificationChannel(channel);
                }
            }
            catch
            {
            } // System.MissingMethodException: Method 'Android.App.Notification/Builder.SetChannelId' not found.
            // I know this is bad, but I can't replicate it on any version, and many people are experiencing it.
        }
    }
}