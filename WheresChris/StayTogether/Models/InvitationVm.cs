using System;

namespace StayTogether.Models
{
    public class InvitationVm : GroupMemberSimpleVm
    {
        public DateTime ReceivedTime { get; set; }

        public string DisplayName()
        {
            return ContactsHelper.NameOrPhone(PhoneNumber, Name);
        }

    }
}
