using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
using Plugin.Geolocator;
using StayTogether;
using StayTogether.Classes;
using StayTogether.Group;
using StayTogether.Helpers;
using StayTogether.Models;
using WheresChris.Views;
using Xamarin.Forms;

namespace WheresChris.Helpers
{
    public class GroupActionsHelper
    {
        public static async Task StartGroup(List<GroupMemberVm> selectedGroupMemberVms, string userPhoneNumber, int expirationHours)
        {
            if (expirationHours < 2) return;

            await StartOrAddToGroup(selectedGroupMemberVms, userPhoneNumber, expirationHours);
        }

        public static async Task StartOrAddToGroup(List<GroupMemberVm> selectedGroupMemberVms, string userPhoneNumber, int expirationHours = 0)
        {
            try
            {
                if (!selectedGroupMemberVms.Any()) throw new System.Exception("SelectedGroupMemberVms must be populated");
                if (string.IsNullOrWhiteSpace(userPhoneNumber))  throw new System.Exception("userPhoneNumber must be populated");

                var userMapPosition = await PositionHelper.GetMapPosition();
                if(!userMapPosition.HasValue || !userMapPosition.Value.LocationValid())throw new System.Exception("userMapPosition is invalid");

                var userPosition = PositionConverter.Convert(userMapPosition.Value);
                if(!userPosition.LocationValid())throw new System.Exception("userPosition is invalid");

                var groupVm = GroupHelper.InitializeGroupVm(selectedGroupMemberVms, userPosition, userPhoneNumber, expirationHours);
                if(groupVm == null || groupVm.GroupMembers.Count < 1 || string.IsNullOrWhiteSpace(groupVm.PhoneNumber))throw new System.Exception("GroupVm is invalid");

                MessagingCenter.Send<object, GroupVm>(new MessagingCenterSender(), LocationSender.StartOrAddGroupMsg, groupVm);
            }
            catch(Exception ex)
            {
                Crashes.TrackError(ex);
            }
       }

        public static List<GroupMemberVm> GetSelectedGroupMembers(IList<ContactDisplayItemVm> items)
        {
            List<GroupMemberVm> selectedGroupMemberVms = new List<GroupMemberVm>();
            foreach (var item in items)
            {
                if (item.Selected)
                {
                    selectedGroupMemberVms.Add(new GroupMemberVm
                    {
                        Name = item.Name,
                        PhoneNumber = item.PhoneNumber
                    });
                }
            }
            return selectedGroupMemberVms;
        }

        public static async Task<List<GroupMemberVm>> GetGroupMembers()
        {
            var userPosition = await CrossGeolocator.Current.GetLastKnownLocationAsync();
            var userPhoneNumber = SettingsHelper.GetPhoneNumber();
            var groupMemberVm = new GroupMemberVm()
            {
                Latitude = userPosition.Latitude,
                Longitude = userPosition.Longitude,
                PhoneNumber = userPhoneNumber
            };
            var locationSender = await LocationSenderFactory.GetLocationSender();
            if(locationSender == null) return new List<GroupMemberVm>();

            var groupMembers = await locationSender.GetMembers(groupMemberVm); //Todo: turn this into a Message
            return groupMembers;
        }
    }
}
