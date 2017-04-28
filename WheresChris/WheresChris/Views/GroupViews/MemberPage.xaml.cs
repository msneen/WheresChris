using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using StayTogether;
using StayTogether.Classes;
using WheresChris.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WheresChris.Views.GroupViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MemberPage : ContentPage
    {
        public MemberPage()
        {
			InitializeComponent ();
            BindingContext = new MemberPageViewModel();
        }


    }



    class MemberPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ContactDisplayItemVm> Items { get; }

        public MemberPageViewModel()
        {
            var groupMembers = GroupActionsHelper.GetGroupMembers().Result;
            Items = new ObservableCollection<ContactDisplayItemVm>();
            UpdateGroupMembers(groupMembers);
        }

        private void UpdateGroupMembers(List<GroupMemberVm> groupMembers)
        {
            Items.Clear();
            foreach (var groupMemberVm in groupMembers)
            {
                var item = new ContactDisplayItemVm
                {
                    Text = ContactsHelper.NameOrPhone(groupMemberVm.PhoneNumber, groupMemberVm.Name)
                };
                Items.Add(item);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
