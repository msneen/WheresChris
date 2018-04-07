using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Toasts;
using Xamarin.Forms;
using StayTogether.Extensions;

namespace StayTogether.Helpers
{
    public class ToastHelper
    {
        private static string _lastTitle = "";
        private static string _lastBody = "";

        public static void Display(string title, string body, IDictionary<string, string> customArgs = null, bool isClickable = false, Action action = null)
        {
            var options = new NotificationOptions()
            {
                Title = title,
                Description = body,
                IsClickable = isClickable,
                CustomArgs = customArgs,
            };
            Display(options, action);
        }

        public static void Display(NotificationOptions options, Action action = null)
        {
            if(DebounceNotification(options)) return;

            //options.AllowTapInNotificationCenter = false;
            options.ClearFromHistory = true;
            var color = (Color) Application.Current.Resources["Primary"];
            
            options.AndroidOptions = new AndroidOptions
            {
                HexColor = color.GetHexString()
            };
            
            var notifier = DependencyService.Get<IToastNotificator>();

            notifier.Notify((INotificationResult result) =>
            {
                if(options.IsClickable && result.Action == NotificationAction.Clicked)
                {
                    action?.Invoke();
                }
            }   
            ,options);
        }

        public static Task CancelToasts()
        {
            var notification = DependencyService.Get<IToastNotificator>();
            notification.CancelAllDelivered();
            return Task.CompletedTask;
        }
        private static bool DebounceNotification(NotificationOptions options)
        {
            if(_lastTitle == options.Title && options.Description == _lastBody) return true;
            _lastTitle = options.Title;
            _lastBody = options.Description;
            return false;
        }
    }
}
