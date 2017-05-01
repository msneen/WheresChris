using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Geolocator;
using StayTogether.Classes;
using WheresChris.Helpers;
using WheresChris.Views.GroupViews;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace WheresChris.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
		public MapPage ()
		{
            InitializeComponent ();
		}

	    protected override async void OnAppearing()
	    {
            await InitializeMap();
        }


	    private async Task InitializeMap()
	    {
	        var mapPosition = await GetMapPosition();

	        GroupMap.MoveToRegion(
	            MapSpan.FromCenterAndRadius(
	                mapPosition, Distance.FromMiles(1)));
	        UpdateMap();
	    }

	    private static async Task<Position> GetMapPosition()
	    {
	        CrossGeolocator.Current.DesiredAccuracy = 50;
            var userPosition = await CrossGeolocator.Current.GetPositionAsync(new TimeSpan(0,0,10));
            
	        if (userPosition == null) return new Position(32.7157, -117.1611);

	        var mapPosition = PositionConverter.Convert(userPosition);
	        return mapPosition;
	    }

	    private async void UpdateMap()
	    {
	        var groupMembers = await GroupActionsHelper.GetGroupMembers();
	        UpdateMap(groupMembers);
	    }

	    private void UpdateMap(List<GroupMemberVm> groupMembers)
	    {
	        if (groupMembers.Count <= 0) return;

	        GroupMap.Pins.Clear();
	        foreach (var groupMember in groupMembers)
	        {
	            var position = new Position(groupMember.Latitude, groupMember.Longitude);
	            var pin = new Pin
	            {
	                Type = PinType.Place,
	                Position = position,
	                Label = groupMember.Name,
	                Address = groupMember.PhoneNumber
	            };
	            GroupMap.Pins.Add(pin);
	        }
	    }

	    private void UpdateMemberPosition(GroupMemberVm groupMemberVm)
	    {
	        var pin = GroupMap.Pins.FirstOrDefault(x => x.Address == groupMemberVm.PhoneNumber);
	        if (pin == null) return;

	        var position = new Position(groupMemberVm.Latitude, groupMemberVm.Longitude);
	        pin.Position = position;
	    }

	    private async void AddMembersButton_OnClicked(object sender, EventArgs e)
	    {
	        var addMemberPage = new AddMemberPage();
	        await Navigation.PushAsync(addMemberPage);
	    }

	    private async void ViewMembersButton_OnClicked(object sender, EventArgs e)
	    {
	        var memberPage = new MemberPage();
	        await Navigation.PushAsync(memberPage);
	    }

	    private async void LeaveGroupButton_OnClicked(object sender, EventArgs e)
	    {
            var locationSender = LocationSenderFactory.GetLocationSender();
	        await locationSender.LeaveGroup();
	    }
	}
}