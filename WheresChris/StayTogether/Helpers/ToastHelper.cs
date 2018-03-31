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
        public static async Task Display(string title, string body, IDictionary<string, string> customArgs = null, bool isClickable = false, Action action = null)
        {
            var options = new NotificationOptions()
            {
                Title = title,
                Description = body,
                IsClickable = isClickable,
                CustomArgs = customArgs,                
            };
            await Display(options, action);
        }

        public static async Task Display(NotificationOptions options, Action action = null)
        {
            //await CancelToasts();
            options.AllowTapInNotificationCenter = true;
            var color = (Color) Application.Current.Resources["Primary"];
            
            options.AndroidOptions = new AndroidOptions
            {
                HexColor = color.GetHexString()
            };
            var notification = DependencyService.Get<IToastNotificator>();
            var delivered = await notification.GetDeliveredNotifications();
            var already = delivered.FirstOrDefault(n => n.Title == options.Title && n.Description == options.Description);
            if(already != null) return;

            notification.Notify((INotificationResult result) =>
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
