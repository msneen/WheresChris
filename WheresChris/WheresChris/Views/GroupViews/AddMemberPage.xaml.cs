using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using StayTogether;
using WheresChris.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace WheresChris.Views.GroupViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddMemberPage : ContentPage
    {
        public AddMemberPage()
        {
			InitializeComponent ();
            BindingContext = new AddMemberPageViewModel();
        }

        protected override async void OnAppearing()
        {
            await ((AddMemberPageViewModel)BindingContext).InitializeContacts();
            ContactsListView.ItemsSource = ((AddMemberPageViewModel)BindingContext).Items;

        }

        private async void AddButton_OnClicked(object sender, EventArgs e)
        {
            var addMemberPageViewModel = BindingContext as AddMemberPageViewModel;
            if (addMemberPageViewModel == null) return;
            var selectedGroupMemberVms = GroupActionsHelper.GetSelectedGroupMembers(addMemberPageViewModel.Items);
            var userPhoneNumber = SettingsHelper.GetPhoneNumber();
            await GroupActionsHelper.StartOrAddToGroup(selectedGroupMemberVms, userPhoneNumber);
        }
        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            if (ContactsListView.IsEnabled)
            {
                var selectedColor = Color.LightSkyBlue;
                object themeColor;
                if (Application.Current.Resources.TryGetValue("ListViewSelectedColor", out themeColor))
                {
                    selectedColor = (Color)themeColor;
                }
                var contactDisplayItemVm = (ContactDisplayItemVm)((RelativeLayout)sender).BindingContext;
                contactDisplayItemVm.Selected = !contactDisplayItemVm.Selected;
                var color = contactDisplayItemVm.Selected ? selectedColor : ContactsListView.BackgroundColor;
                ((RelativeLayout)sender).BackgroundColor = color;
            }
        }

    }


    public class AddMemberPageViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; } = "Add Members";
        public ObservableCollection<ContactDisplayItemVm> Items { get; set; }

        public async Task InitializeContacts()
        {
            var contactsHelper = new ContactsHelper();
            Items = await contactsHelper.GetContactsAsync();

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
