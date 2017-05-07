using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

        private async void RefreshButton_OnClicked(object sender, EventArgs e)
        {
            await ((MemberPageViewModel) BindingContext).RefreshMembers();
        }
    }

    internal class MemberPageViewModel
    {
        public ObservableCollection<ContactDisplayItemVm> Items { get; }

        public MemberPageViewModel()
        {            
            Items = new ObservableCollection<ContactDisplayItemVm>();           
        }

        public async Task RefreshMembers()
        {
            var groupMembers = await GroupActionsHelper.GetGroupMembers();
            UpdateGroupMembers(groupMembers);
        }

        private void UpdateGroupMembers(IEnumerable<GroupMemberVm> groupMembers)
        {
            Items.Clear();
            foreach (var groupMemberVm in groupMembers)
            {
                var item = new ContactDisplayItemVm
                {
                    Name = ContactsHelper.NameOrPhone(groupMemberVm.PhoneNumber, groupMemberVm.Name)
                };
                Items.Add(item);
            }
        }
    }
}
