using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Geolocator;
using StayTogether.Classes;
using StayTogether.Location;
using StayTogether.Models;
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

	    private void InitializeMessagingCenterSubscriptions()
	    {
            GroupPositionChangedEvent = new GroupPositionChangedEvent(new TimeSpan(0, 0, 30));
            GroupPositionChangedEvent.OnGroupPositionChangedMsg += async (sender, args) =>
            {
                AddMembersButton.TextColor = AddMembersButton.TextColor == Color.Blue ? Color.Black : Color.Blue;
                await UpdateMap(args.GroupMembers);
            };
        }
        //Problem:  We can get the map to show up in OnAppearing, but we can't get the pins to update.
        //If we make pins in on appearing, they show up
        //if we try to rewrite the pins every time we get a signalR group update, they aren't visible
        //if we try to move the pins, they don't move
        //1. Find a different map control
        //2. Use the guy's custom Renderer solution, so we can update the view model and have it update the map



        //private async Task InitializeMap()
        //{
        //    var mapPosition = await GetMapPosition();

        //       GroupMap = new Map();

        //    GroupMap.MoveToRegion(
        //        MapSpan.FromCenterAndRadius(
        //            mapPosition, Distance.FromMiles(.1)));
        //    UpdateMap();//Todo:  Add me back
        //}

        private async Task InitializeMap()
	    {
	        //var groupMembersSimple = await GetGroupMembers();
            await UpdateMap(null);
        }

	    private static async Task<Position> GetMapPosition()
	    {
	        CrossGeolocator.Current.DesiredAccuracy = 5;
            var userPosition = await CrossGeolocator.Current.GetPositionAsync(new TimeSpan(0,0,10));
            
	        if (userPosition == null) return new Position(32.7157, -117.1611);

	        var mapPosition = PositionConverter.Convert(userPosition);
	        return mapPosition;
	    }

        private async Task<List<GroupMemberSimpleVm>> GetGroupMembers() //UpdateMap()
        {
            var groupMembers = await GroupActionsHelper.GetGroupMembers();
            var groupMembersSimple = groupMembers.Select(groupMember => new GroupMemberSimpleVm
            {
                PhoneNumber = groupMember.PhoneNumber,
                Name = groupMember.Name,
                Latitude = groupMember.Latitude,
                Longitude = groupMember.Longitude
            }).ToList();
            return groupMembersSimple;
        }

        private async Task UpdateMap(List<GroupMemberSimpleVm> groupMembers)
	    {
            var mapPosition = await GetMapPosition();

            //GroupMap = new Map();

            GroupMap.MoveToRegion(
                MapSpan.FromCenterAndRadius(
                    mapPosition, Distance.FromMiles(.1)));

            if (groupMembers == null) return;
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
            //foreach (var groupMember in groupMembers)
            //{
            //    var pin = GroupMap.Pins.FirstOrDefault(x => x.Address == groupMember.PhoneNumber);
            //    if (pin == null)
            //    {
            //        var position = new Position(groupMember.Latitude, groupMember.Longitude);
            //        pin = new Pin
            //        {
            //            Type = PinType.Place,
            //            Position = position,
            //            Label = groupMember.Name,
            //            Address = groupMember.PhoneNumber
            //        };
            //        GroupMap.Pins.Add(pin);

            //    }
            //    pin.Position = new Position(groupMember.Latitude, groupMember.Longitude);
            //}
            //This was my original code


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