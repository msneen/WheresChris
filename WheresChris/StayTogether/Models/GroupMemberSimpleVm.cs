namespace StayTogether.Models
{
    public class GroupMemberSimpleVm : Plugin.Geolocator.Abstractions.Position
    {
        private string _phoneNumber;

        public string PhoneNumber
        {
            get { return ContactsHelper.CleanPhoneNumber(_phoneNumber); }
            set { _phoneNumber = value; }
        }
        public string Name { get; set; }
    }
}
