using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private bool _mapInitialized = false;
        private Interval _positionInitializationInterval = new Interval();

        public GroupPositionChangedEvent GroupPositionChangedEvent;
        public GroupLeftEvent GroupLeftEvent;
        public GroupJoinedEvent GroupJoinedEvent;

        public MapPage ()
		{
            Title = "Where's Chris - Map";
            InitializeComponent ();
            GroupMap.MapType = MapType.Hybrid;            
            InitializeMessagingCenterSubscriptions();
            SetFormEnabled(false);
		}

	    private void SetFormEnabled(bool isSelected)
	    {
	        AddMembersButton.IsEnabled = isSelected;
	        ViewMembersButton.IsEnabled = isSelected;
	        LeaveGroupButton.IsEnabled = isSelected;
	    }


	    protected override void OnAppearing()
	    {
	        if (_mapInitialized) return;           
            _positionInitializationInterval.SetInterval(InitializeMap().Wait, 500);
        }

        /// <summary>
        /// This event is fired when a group position update is received from the server
        /// </summary>
	    private void InitializeMessagingCenterSubscriptions()
	    {
            GroupPositionChangedEvent = new GroupPositionChangedEvent(new TimeSpan(0, 0, 30));
            GroupPositionChangedEvent.OnGroupPositionChangedMsg += (sender, args) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    AddMembersButton.TextColor = AddMembersButton.TextColor == Color.Blue ? Color.Black : Color.Blue;
                    SetFormEnabled(true);
                    UpdateMap(args.GroupMembers);
                });
            };
            GroupJoinedEvent = new GroupJoinedEvent();
            GroupJoinedEvent.OnGroupJoinedMsg += (sender, args) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SetFormEnabled(true);
                });
            };

            GroupLeftEvent = new GroupLeftEvent();
            GroupLeftEvent.OnGroupLeftMsg += (sender, args) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SetFormEnabled(false);
                });
            };
        }

        private async Task InitializeMap()
        {
            var justMeList = await GetMyPositionList();
            if (justMeList == null)
            {
                _mapInitialized = false;
                _positionInitializationInterval.SetInterval(InitializeMap().Wait, 3000);
                return;
            };
            HideSpinnerShowMap();
            UpdateMap(justMeList);
            _mapInitialized = true;
        }

	    private static async Task<List<GroupMemberSimpleVm>> GetMyPositionList()
	    {
	        var hasLocationPermissions = await PermissionHelper.HasOrRequestLocationPermission();
	        if (!hasLocationPermissions) return null;

	        var userPosition = await PositionHelper.GetMapPosition();
	        if (!userPosition.HasValue) return null;
	        if (!PositionHelper.LocationValid(userPosition.Value)) return null;

	        var userPhoneNumber = SettingsHelper.GetPhoneNumber();
	        var justMeList = new List<GroupMemberSimpleVm>
	        {
	            new GroupMemberSimpleVm
	            {
	                Latitude = userPosition.Value.Latitude,
	                Longitude = userPosition.Value.Longitude,
	                PhoneNumber = userPhoneNumber,
	                Name = "Me"
	            }
	        };
	        return justMeList;
	    }

	    private void UpdateMap(List<GroupMemberSimpleVm> groupMembers)
	    {            
            var userPhoneNumber = SettingsHelper.GetPhoneNumber();
            var customPins = new List<TKCustomMapPin>();

            foreach (var groupMember in groupMembers)
            {
                var position = new Position(groupMember.Latitude, groupMember.Longitude);
                if (PositionHelper.LocationValid(position))
                {
                    customPins.Add(new TKCustomMapPin
                    {
                        Title = ContactsHelper.NameOrPhone(groupMember.PhoneNumber, groupMember.Name),
                        Position = position,
                        ShowCallout = true,
                        DefaultPinColor = groupMember.PhoneNumber == userPhoneNumber ? Color.Blue : Color.Red
                    });
                }
            }

            var mapCenterPosition = PositionHelper.GetMapCenter(groupMembers);
	        if (!PositionHelper.LocationValid(mapCenterPosition)) return;

	        var radius = PositionHelper.GetRadius(groupMembers, mapCenterPosition);

	        Device.BeginInvokeOnMainThread(() =>
	        {
	            GroupMap.MapType= MapType.Hybrid; //This doesn't seem to work on android
	            GroupMap.MapCenter = mapCenterPosition;
	            GroupMap.MapRegion = MapSpan.FromCenterAndRadius(mapCenterPosition, Distance.FromMiles(radius));
	            GroupMap.CustomPins = customPins;
	        });
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
            var locationSender = await LocationSenderFactory.GetLocationSender();
            //Calling both because I can only leave if I'm not the group leader, otherwise I have to end the group
	        await locationSender.LeaveGroup();
	        await locationSender.EndGroup();
            MessagingCenter.Send<LocationSender>(locationSender, LocationSender.ThisUserLeftGroupMsg);
        }

        private void HideSpinnerShowMap()
        {
            Spinner.IsRunning = false;
            Spinner.IsVisible = false;
            Spinner.IsEnabled = false;
            GroupMap.IsVisible = true;
        }
    }
}