using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Geolocator;
using StayTogether;
using StayTogether.Classes;
using WheresChris.Helpers;
using WheresChris.Views.GroupViews;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;
#if __ANDROID__
using StayTogether.Droid.Services;
#endif
#if __IOS__
using WheresChris.iOS;
#endif

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
            InitializeMap();
        }


	    private async void InitializeMap()
	    {
            try
            {
#if __ANDROID__
                var mapPosition = await GetMapPosition();

                GroupMap.MoveToRegion(
                    MapSpan.FromCenterAndRadius(
                        mapPosition, Distance.FromMiles(1)));
#endif
            }
            catch (Exception ex) { }

	    }

	    private static async Task<Position> GetMapPosition()
	    {
	        var userPosition = await CrossGeolocator.Current.GetLastKnownLocationAsync();
            
	        if (userPosition == null) return new Position(32.7157, -117.1611);

	        var mapPosition = PositionConverter.Convert(userPosition);
	        return mapPosition;
	    }

	    private void UpdateMap(GroupVm groupVm)
	    {
	        var groupMembers = groupVm.GroupMembers;
	        UpdateMap(groupMembers);
	    }

	    private void UpdateMap(List<GroupMemberVm> groupMembers)
	    {
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
	        await Navigation.PushModalAsync(addMemberPage);
	    }

	    private async void ViewMembersButton_OnClicked(object sender, EventArgs e)
	    {
	        var memberPage = new MemberPage();
	        await Navigation.PushModalAsync(memberPage);
	    }

	    private async void LeaveGroupButton_OnClicked(object sender, EventArgs e)
	    {
            var locationSender = LocationSenderFactory.GetLocationSender();
	        await locationSender.LeaveGroup();
	    }
	}
}
//#if __ANDROID__
//	        userPosition = LocationSenderService.Instance.GetPosition();
//#endif
//#if __IOS__
//	        if (AppDelegate.LocationManager != null && AppDelegate.LocationManager.ClLocationManager != null)
//	        {
//	            userPosition = AppDelegate.LocationManager.GetPosition();
//	        }
//#endif