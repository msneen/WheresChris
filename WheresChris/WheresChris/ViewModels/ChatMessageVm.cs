using System.ComponentModel;
using StayTogether.Classes;

namespace WheresChris.ViewModels
{
    public class ChatMessageVm:INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get { return _name;}
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        private string _message;

        public string Message
        {
            get { return _message;}
            set
            {
                _message = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
            }
        }

        public GroupMemberVm Member { get; set; }

        public override string ToString() => Name;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
