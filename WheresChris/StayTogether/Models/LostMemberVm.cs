using System;
using System.Collections.Generic;
using System.Text;

namespace StayTogether.Models
{
    public class LostMemberVm
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double LostDistance { get; set; }
    }
}
