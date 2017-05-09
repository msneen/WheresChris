using System.ComponentModel;
using StayTogether.Models;
using Xamarin.Forms;

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
                BackgroundColor = _selected ? Color.Blue : Color.White;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected"));
            }
        }

        private Color _backgroundColor;
        public Color BackgroundColor
        {
            get { return _backgroundColor;}
            set
            {
                _backgroundColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BackGroundColor"));
            }
        }

        public InvitationVm Invitation { get; set; }

        public override string ToString() => Name;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}