using Foundation;
using UIKit;

namespace StayTogether.iOS.NotificationCenter
{
    public class NotificationManager
    {
        public static void RegisterNotifications(UIApplication application)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var notificationSettings = UIUserNotificationSettings.GetSettingsForTypes(
                    UIUserNotificationType.Alert | UIUserNotificationType.Badge/* | UIUserNotificationType.Sound*/, null
                    );

                application.RegisterUserNotificationSettings(notificationSettings);
            }
        }

        public static void InitializeNotifications(NSDictionary launchOptions, UIWindow window)
        {
            // check for a notification
            if (launchOptions == null) return;
            // check for a local notification
            if (!launchOptions.ContainsKey(UIApplication.LaunchOptionsLocalNotificationKey)) return;


            var localNotification =
                launchOptions[UIApplication.LaunchOptionsLocalNotificationKey] as UILocalNotification;
            if (localNotification == null) return;

            var okayAlertController = UIAlertController.Create(localNotification.AlertAction,
                localNotification.AlertBody, UIAlertControllerStyle.Alert);
            okayAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            window.RootViewController.PresentViewController(okayAlertController, true, null);

            // reset our badge
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        }
    }
}