using System;
using System.Collections.Generic;
using System.Text;

namespace StayTogether.Models
{
    public class GroupMemberSimpleVm
    {
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
