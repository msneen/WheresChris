namespace StayTogether.Classes
{
    public class GroupMemberVm
    {
        private string _phoneNumber;

        public string PhoneNumber
        {
            get { return ContactsHelper.CleanPhoneNumber(_phoneNumber); }
            set { _phoneNumber = value; }
        }

        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsAdmin { get; set; }
        public string GroupId { get; set; }
        public string ConnectionId { get; set; }
        public bool Selected { get; set; }
        public bool InvitationConfirmed { get; set; }
        public double LostDistance { get; set; }

        public override string ToString()
        {
            return $"{Name}-{PhoneNumber}";
        }
    }
}