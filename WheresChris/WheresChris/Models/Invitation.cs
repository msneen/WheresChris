using System;
using System.Collections.Generic;
using System.Text;
using StayTogether.Classes;

namespace WheresChris.Models
{
    public class Invitation
    {
        public List<GroupMemberVm> Members { get; set; }
        public string UserPhoneNumber { get; set; }
        public int ExpirationHours { get; set; }
    }
}
