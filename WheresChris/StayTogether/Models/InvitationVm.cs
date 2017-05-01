using System;
using System.Collections.Generic;
using System.Text;

namespace StayTogether.Models
{
    public class InvitationVm
    {
        private string _phoneNumber;

        public string PhoneNumber
        {
            get { return ContactsHelper.CleanPhoneNumber(_phoneNumber); }
            set { _phoneNumber = value; }
        }

        public string Name { get; set; }
        public DateTime ReceivedTime { get; set; }

        public string DisplayName()
        {
            return ContactsHelper.NameOrPhone(PhoneNumber, Name);
        }

    }
}
