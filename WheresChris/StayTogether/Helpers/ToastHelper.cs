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
    }
}
