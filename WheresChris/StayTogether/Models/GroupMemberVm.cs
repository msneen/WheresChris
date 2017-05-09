using StayTogether.Models;

namespace StayTogether.Classes
{
    public class GroupMemberVm : LostMemberVm
    {
        public bool IsAdmin { get; set; }
        public string GroupId { get; set; }
        public string ConnectionId { get; set; }
        public bool Selected { get; set; }
        public bool InvitationConfirmed { get; set; }

        public override string ToString()
        {
            return $"{Name}-{PhoneNumber}";
        }
    }
}