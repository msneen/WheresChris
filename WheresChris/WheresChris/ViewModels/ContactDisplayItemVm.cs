using System.ComponentModel;
using StayTogether.Models;

namespace WheresChris.Views
{
    public class ContactDisplayItemVm : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }

        private bool _selected;
        public bool Selected
        {
            get { return _selected;}
            set
            {
                _selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected"));
            }
        }

        public InvitationVm Invitation { get; set; }

        public override string ToString() => Name;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}