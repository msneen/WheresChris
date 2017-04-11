using System;
using System.Collections.Generic;
using System.Text;
using Plugin.Geolocator.Abstractions;
using Plugin.Settings;
using StayTogether.Classes;
using StayTogether.Location;

namespace StayTogether.Group
{
    public class GroupHelper
    {
        public static GroupVm CreateGroupVm(GroupMemberVm adminMember, List<GroupMemberVm> contactList, int expireInHours = 5)
        {
            var groupVm = new GroupVm
            {
                MaximumDistance = 100,
                PhoneNumber = adminMember.PhoneNumber,
                GroupMembers = contactList,
                GroupCreatedDateTime = DateTime.Now,
                GroupDisbandDateTime = DateTime.Now.AddHours(expireInHours)
            };
            return groupVm;
        }

        public static GroupMemberVm CreateAdminMember(Position position, string phoneNumber, string nickname)
        {
            var adminMember = GroupMemberPositionAdapter.Adapt(position);
            adminMember.Name = nickname;
            adminMember.PhoneNumber = phoneNumber;
            adminMember.IsAdmin = true;
            return adminMember;
        }

        public static GroupVm InitializeGroupVm(List<GroupMemberVm> contactList, Position position, string phoneNumber, int expireInHours)
        {
            var adminMember = GroupHelper.CreateAdminMember(position, phoneNumber,
                CrossSettings.Current.GetValueOrDefault<string>("nickname"));//Todo: handle phoneNumber and nickname the same way

            contactList.Insert(0, adminMember);

            var groupVm = GroupHelper.CreateGroupVm(adminMember, contactList, expireInHours);
            return groupVm;
        }
    }
}
