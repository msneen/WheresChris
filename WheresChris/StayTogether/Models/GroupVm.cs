using System;
using System.Collections.Generic;
using StayTogether.Classes;

namespace StayTogether
{
    public class GroupVm
    {
        private string _phoneNumber;

        public string PhoneNumber
        {
            get { return ContactsHelper.CleanPhoneNumber(_phoneNumber); }
            set { _phoneNumber = value; }
        }

        public int MaximumDistance { get; set; }
        public DateTime GroupCreatedDateTime { get; set; }
        public DateTime GroupDisbandDateTime { get; set; }
        public DateTime LastContactDateTime { get; set; }

        public List<GroupMemberVm> GroupMembers { get; set; }
    }
}
