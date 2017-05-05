using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Geolocator;
using StayTogether;
using StayTogether.Helpers;
using StayTogether.Models;
using TK.CustomMap;
using WheresChris.Helpers;
using WheresChris.Messaging;
using WheresChris.Views.GroupViews;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;
using Distance = Xamarin.Forms.Maps.Distance;

namespace WheresChris.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
        public GroupPositionChangedEvent GroupPositionChangedEvent;

	    public MapPage ()
		{
            InitializeComponent ();
            InitializeMessagingCenterSubscriptions();
            Title = "Map";
		}

	    protected override async void OnAppearing()
	    {
	        await InitializeMap();	        
	    }

        /// <summary>
        /// This event is fired when a group position update is received from the server
        /// </summary>
	    private void InitializeMessagingCenterSubscriptions()
	    {
            GroupPositionChangedEvent = new GroupPositionChangedEvent(new TimeSpan(0, 0, 30));
            GroupPositionChangedEvent.OnGroupPositionChangedMsg += (sender, args) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    AddMembersButton.TextColor = AddMembersButton.TextColor == Color.Blue ? Color.Black : Color.Blue;
                    await UpdateMap(args.GroupMembers);
                });
            };
        }

        private async Task InitializeMap()
        {
            var justMeList = await GetMyPositionList();
            await UpdateMap(justMeList);
        }

	    private static async Task<List<GroupMemberSimpleVm>> GetMyPositionList()
	    {
	        var userPosition = await GetMapPosition();
	        var userPhoneNumber = SettingsHelper.GetPhoneNumber();
	        var justMeList = new List<GroupMemberSimpleVm>
	        {
	            new GroupMemberSimpleVm
	            {
	                Latitude = userPosition.Latitude,
	                Longitude = userPosition.Longitude,
	                PhoneNumber = userPhoneNumber,
	                Name = "Me"
	            }
	        };
	        return justMeList;
	    }

	    private static async Task<Position> GetMapPosition()
	    {
	        CrossGeolocator.Current.DesiredAccuracy = 5;
            var userPosition = await CrossGeolocator.Current.GetPositionAsync(new TimeSpan(0,0,10));
            
	        if (userPosition == null) return new Position(32.7157, -117.1611);

	        var mapPosition = PositionConverter.Convert(userPosition);
	        return mapPosition;
	    }

        private async Task UpdateMap(List<GroupMemberSimpleVm> groupMembers)
	    {            
            var userPosition = await GetMapPosition();
            var userPhoneNumber = SettingsHelper.GetPhoneNumber();
            var customPins = new List<TKCustomMapPin>();

            foreach (var groupMember in groupMembers)
            {
                var position = new Position(groupMember.Latitude, groupMember.Longitude);
                customPins.Add(new TKCustomMapPin
                {
                    Title = ContactsHelper.NameOrPhone(groupMember.PhoneNumber, groupMember.Name),
                    Position = position,
                    ShowCallout = true,
                    DefaultPinColor = groupMember.PhoneNumber == userPhoneNumber ? Color.Blue : Color.Red
                });
            }

            var mapCenterPosition = GetMapCenter(userPosition, groupMembers);            

            Device.BeginInvokeOnMainThread(() =>
            {
                GroupMap.MapType= MapType.Hybrid; //This doesn't seem to work
                GroupMap.MapCenter = mapCenterPosition;
                GroupMap.MapRegion = MapSpan.FromCenterAndRadius(mapCenterPosition, Distance.FromMeters(80));
                GroupMap.CustomPins = customPins;
            });
        }

	    private static Position GetMapCenter(Position userPosition, List<GroupMemberSimpleVm> groupMembers)
	    {
            //if we have more than one groupMember, calculate the center, otherwise use the currrent user's position
            if (groupMembers.Count > 1)
            {                
                return PositionHelper.ConvertPluginPositionToMapPosition(PositionHelper.GetCentralGeoCoordinate(groupMembers));
            }
            return userPosition;
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