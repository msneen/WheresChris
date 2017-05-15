using System;
using System.Collections.Generic;
using Plugin.Geolocator.Abstractions;
using Plugin.Settings;
using StayTogether.Classes;
using StayTogether.Location;
using WheresChris.Helpers;

namespace StayTogether.Group
{
    public class GroupHelper
    {
        public static GroupVm CreateGroupVm(GroupMemberVm adminMember, List<GroupMemberVm> contactList, int expireInHours = 5)
        {
            var groupVm = new GroupVm
            {
                MaximumDistance = 150,
                PhoneNumber = adminMember.PhoneNumber,
                GroupMembers = contactList,
                GroupCreatedDateTime = DateTime.Now,
                GroupDisbandDateTime = DateTime.Now.AddHours(expireInHours)
            };
            return groupVm;
        }

        public static GroupMemberVm CreateAdminMember(Position position, string phoneNumber, string nickname)
        {
            var adminMember = GroupMemberConverter.Convert(position);
            adminMember.Name = nickname;
            adminMember.PhoneNumber = phoneNumber;
            adminMember.IsAdmin = true;
            return adminMember;
        }

        public static GroupVm InitializeGroupVm(List<GroupMemberVm> contactList, Position position, string phoneNumber, int expireInHours)
        {
            var nickname = SettingsHelper.GetNickname();
            var adminMember = CreateAdminMember(position, phoneNumber, nickname);

            contactList.Insert(0, adminMember);

            var groupVm = CreateGroupVm(adminMember, contactList, expireInHours);
            return groupVm;
        }
    }
}
