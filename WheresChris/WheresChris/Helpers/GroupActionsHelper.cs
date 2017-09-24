using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (!selectedGroupMemberVms.Any()) return;
            if (string.IsNullOrWhiteSpace(userPhoneNumber)) return;

            var userMapPosition = await PositionHelper.GetMapPosition();
            var userPosition = PositionConverter.Convert(userMapPosition.Value);
            var groupVm = GroupHelper.InitializeGroupVm(selectedGroupMemberVms, userPosition, userPhoneNumber, expirationHours);
            MessagingCenter.Send<object, GroupVm>(new MessagingCenterSender(), LocationSender.StartOrAddGroupMsg, groupVm);
        }

        public static List<GroupMemberVm> GetSelectedGroupMembers(ObservableCollection<ContactDisplayItemVm> items)
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
