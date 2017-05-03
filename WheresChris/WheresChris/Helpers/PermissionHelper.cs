using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace WheresChris.Helpers
{
    public class PermissionHelper
    {
        public static bool HasPhonePermission()
        {
            return HasPermission(Permission.Phone);
        }
        public static bool HasLocationPermission()
        {
            return HasPermission(Permission.Location);
        }

        public static bool HasContactPermission()
        {
            return HasPermission(Permission.Contacts);
        }

        private static bool HasPermission(Permission permission)
        {
            var phonePermission =
                CrossPermissions.Current.CheckPermissionStatusAsync(permission).Result;
            return phonePermission == PermissionStatus.Granted;
        }



        public static async Task RequestPhonePermission()
        {
            await RequestPermission(Permission.Phone);
        }
        public static async Task RequestLocationPermission()
        {
            await RequestPermission(Permission.Location);
        }

        public static async Task RequestContactPermission()
        {
            await RequestPermission(Permission.Contacts);
        }

        private static async Task RequestPermission(Permission permission)
        {
            await CrossPermissions.Current.RequestPermissionsAsync(new[] {permission});
        }
    }
}
