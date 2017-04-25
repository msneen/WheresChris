using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StayTogether;
using StayTogether.Classes;
using WheresChris.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;
#if __ANDROID__
using StayTogether.Droid.Services;
#endif
#if __IOS__
//using WheresChris.iOS;        
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


	    private void InitializeMap()
	    {
            try
            {
#if __ANDROID__
                var mapPosition = GetMapPosition();

                GroupMap.MoveToRegion(
                    MapSpan.FromCenterAndRadius(
                        mapPosition, Distance.FromMiles(1)));
#endif
            }
            catch (Exception ex) { }

	    }

	    private static Position GetMapPosition()
	    {
	        Plugin.Geolocator.Abstractions.Position userPosition = new Plugin.Geolocator.Abstractions.Position();
#if __ANDROID__
	        userPosition = LocationSenderService.Instance.GetPosition();
#endif
#if __IOS__
                //userPosition = AppDelegate.LocationManager.GetPosition();
#endif
	        if (userPosition == null) return new Position(32.7157, -117.1611);

	        Xamarin.Forms.Maps.Position mapPosition = PositionConverter.Convert(userPosition);
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
	}
}
