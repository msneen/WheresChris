﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace WheresChris.Helpers
{
    public class PermissionHelper
    {
        public static bool HasNecessaryPermissions()
        {
            var phonePermissionGranted = PermissionHelper.HasPhonePermission();
            var locationPermissionGranted = PermissionHelper.HasLocationPermission();
            var contactPermissionGranted = PermissionHelper.HasContactPermission();
            return phonePermissionGranted && locationPermissionGranted && contactPermissionGranted;
        }


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
            await RequestPermission(Permission.Phone, "Phone Permission", "We need permission to access your phone");
        }
        public static async Task RequestLocationPermission()
        {
            await RequestPermission(Permission.Location, "Location Permission", "We need permission to access your location");
        }

        public static async Task RequestContactPermission()
        {
            await RequestPermission(Permission.Contacts, "Contacts Permission", "We need permission to access your contacts");
        }

        private static async Task<PermissionStatus> RequestPermission(Permission permission, string title, string body)
        {
            if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(permission))
            {
                Plugin.LocalNotifications.CrossLocalNotifications.Current.Show(title, body);
            }
            var permissionStatus = await CrossPermissions.Current.RequestPermissionsAsync(new[] {permission});
            return permissionStatus[permission];
        }

        public static async Task<string> GetNecessaryPermissionInformation()
        {
            var contacts = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Contacts);
            var phone = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Phone);
            var location = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
            var information = $"\n\rContacts={contacts}\n\rPhone={phone}\n\rLocation={location}";
            return information;
        }
    }
}
