using Foundation;
using UIKit;

namespace StayTogether.iOS.NotificationCenter
{
    public class NotificationBase
    {
        protected static UILocalNotification CreateNotification(string body, string title, int badgeNumber)
        {
            var notification = new UILocalNotification
            {
                FireDate = NSDate.Now,
                AlertAction = "View Alert",
                AlertBody = body,
                AlertTitle = title,
                ApplicationIconBadgeNumber = badgeNumber/*,
                SoundName = UILocalNotification.DefaultSoundName*/
            };            
            return notification;
        }



        protected static NSMutableDictionary GetDictionary(UILocalNotification notification)
        {
            var dictionary = new NSMutableDictionary();
            return dictionary;
        }

        protected static void AddValue(string keyName, string propertyValue, ref NSMutableDictionary dictionary)
        {
            var key = new NSString(keyName);
            var stringValue = propertyValue ?? "";
            var value = new NSString(stringValue);
            dictionary.Add(key, value);
        }

        protected static string GetValue(string keyName, ref NSDictionary dictionary)
        {
            var key = new NSString(keyName);
            var value = dictionary.ValueForKey(key).ToString();
            return value;
        }
    }
}
