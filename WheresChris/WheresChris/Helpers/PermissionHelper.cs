﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace WheresChris.Helpers
{
    public class PermissionHelper
    {
        public static async Task<bool> HasNecessaryPermissionsWithRequest()
        {
            //var phonePermissionGranted = await HasOrRequestPhonePermission();
            var locationPermissionGranted = await HasOrRequestLocationPermission();
            var contactPermissionGranted = await HasOrRequestContactPermission();
            return /*phonePermissionGranted &&*/ locationPermissionGranted && contactPermissionGranted;
        }

        public static async Task<bool> HasNecessaryPermissions()
        {
            var locationPermissionsGranted = await HasLocationPermission();
            var contactsPermissionsGranted = await HastContactPermission();
            return locationPermissionsGranted && contactsPermissionsGranted;
        }


        public static async Task<bool> HasOrRequestPhonePermission()
        {
            var phonePermissionStatus = await RequestPhonePermission();
            return phonePermissionStatus == PermissionStatus.Granted;
        }
        public static async Task<bool> HasOrRequestLocationPermission()
        {
            var locationPermissionStatus = await RequestLocationPermission();
            return locationPermissionStatus == PermissionStatus.Granted;
        }

        public static async Task<bool> HasOrRequestContactPermission()
        {
            var contactPermissionStatus =  await RequestContactPermission();
            return contactPermissionStatus == PermissionStatus.Granted;
        }

        public static async Task<bool> HasPhonePermission()
        {
            return await HasPermission(Permission.Phone);
        }
        public static async Task<bool> HasLocationPermission()
        {
            return await HasPermission(Permission.Location);
        }

        public static async Task<bool> HastContactPermission()
        {
            return await HasPermission(Permission.Contacts);
        }

        private static async Task<bool> HasPermission(Permission permission)
        {            
            var phonePermission = await CrossPermissions.Current.CheckPermissionStatusAsync(permission);
            return phonePermission == PermissionStatus.Granted;
        }

        public static async Task<PermissionStatus> RequestPhonePermission()
        {
            return await RequestPermission(Permission.Phone, "Phone Permission", "We need permission to access your phone");
        }
        public static async Task<PermissionStatus> RequestLocationPermission()
        {
            return await RequestPermission(Permission.Location, "Location Permission", "We need permission to access your location");
        }

        public static async Task<PermissionStatus> RequestContactPermission()
        {
            return await RequestPermission(Permission.Contacts, "Contacts Permission", "We need permission to access your contacts");
        }

        private static async Task<PermissionStatus> RequestPermission(Permission permission, string title, string body)
        {
            var existingPermission = await CrossPermissions.Current.CheckPermissionStatusAsync(permission);
            if (existingPermission == PermissionStatus.Granted) return existingPermission;


            if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(permission))
            {
                Plugin.LocalNotifications.CrossLocalNotifications.Current.Show(title, body);

            }
            var permissionStatus = await CrossPermissions.Current.RequestPermissionsAsync(permission);

            return permissionStatus.ContainsKey(permission) ? permissionStatus[permission] : PermissionStatus.Unknown;
        }

        public static async Task<string> GetNecessaryPermissionInformation()
        {
            var contacts = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Contacts);
            var phone = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Phone);
            var location = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);


            if (contacts == PermissionStatus.Granted && phone == PermissionStatus.Granted &&
                location == PermissionStatus.Granted)
            {
                Analytics.TrackEvent("Permissions", new Dictionary<string, string>
                {
                    {"Phone" , phone.ToString()},
                    {"Location", location.ToString() },
                    {"Contacts", contacts.ToString() }
                });                
            }

            var information = $"\n\rContacts={contacts}\n\rPhone={phone}\n\rLocation={location}";
            return information;
        }
    }
}
