using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content.PM;
using StayTogether;
using StayTogether.Helpers;
using StayTogether.Helpers.DistanceCalculator;
using StayTogether.Models;
using TK.CustomMap;
using WheresChris.Helpers;
using WheresChris.Views.GroupViews;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Distance = Xamarin.Forms.Maps.Distance;

#if (__ANDROID__)
using Android.Content;
#endif
#if (__IOS__)
using Foundation;
using UIKit;
#endif

namespace WheresChris.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
        private bool _mapInitialized = false;
        private Interval _positionInitializationInterval = new Interval();
	    private Xamarin.Forms.Maps.Position? _mapPosition;
        private DateTime _lastPositionUpdateTime = DateTime.Now;

	    public MapPage ()
		{
            Title = "Where's Chris - Map";
            InitializeComponent ();
            GroupMap.MapType = MapType.Hybrid;
    
                     
            InitializeMessagingCenterSubscriptions();
            SetFormEnabled(false);

//#if (__ANDROID__)
		   // ViewARButton.IsVisible = true;
//#endif
		}



	    private void SetMapInitialPosition()
	    {
	        var rollerCoasterPosition = new List<GroupMemberSimpleVm>
            {
                new GroupMemberSimpleVm
                {
                    Latitude = 32.7714,
                    Longitude = -117.2517,
                    PhoneNumber = "",
                    Name = ""
                }
            };
            UpdateMap(rollerCoasterPosition);
        }

	    private void SetFormEnabled(bool isSelected)
	    {
	        AddMembersButton.IsEnabled = isSelected;
	        ViewMembersButton.IsEnabled = isSelected;
	        LeaveGroupButton.IsEnabled = isSelected;
	        ViewARButton.IsEnabled = isSelected;
	    }


	    protected override void OnAppearing()
	    {
	        if (_mapInitialized) return;
            SetMapInitialPosition();
            _positionInitializationInterval.SetInterval(InitializeMap().Wait, 500);
        }

        /// <summary>
        /// This event is fired when a group position update is received from the server
        /// </summary>
	    private void InitializeMessagingCenterSubscriptions()
	    {
            MessagingCenter.Subscribe<LocationSender, List <GroupMemberSimpleVm >> (this, LocationSender.GroupPositionUpdateMsg, (sender, groupMemberSimpleVm) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (!GroupMap.IsVisible) return;
                    AddMembersButton.TextColor = AddMembersButton.TextColor == Color.Blue ? Color.Black : Color.Blue;
                    SetFormEnabled(true);
                    UpdateMap(groupMemberSimpleVm);
                });
            });

            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupJoinedMsg,
            (sender) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SetFormEnabled(true);
                });
            });
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupCreatedMsg,
            (sender) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SetFormEnabled(true);
                });
            });

            //If the group is disbanded, it means this user also left the group with everyone else
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.GroupDisbandedMsg,
            (sender) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SetFormEnabled(false);
                });
            });
            //This user left the group
            MessagingCenter.Subscribe<LocationSender>(this, LocationSender.ThisUserLeftGroupMsg,
            (sender) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    SetFormEnabled(false);
                });
            });
            MessagingCenter.Subscribe<MessagingCenterSender, Plugin.Geolocator.Abstractions.Position>(this, LocationSender.PositionUpdatedMsg, (sender, position) =>
            {
                _mapPosition = PositionHelper.GetMapPosition(position);
                _lastPositionUpdateTime = DateTime.Now;
            });
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
                         
             UpdateMap(justMeList);
             HideSpinnerShowMap();
            _mapInitialized = true;
        }

	    private async Task<List<GroupMemberSimpleVm>> GetMyPositionList()
	    {
	        var hasLocationPermissions = await PermissionHelper.HasOrRequestLocationPermission();
	        if (!hasLocationPermissions) return null;

	        if (DateTime.Now.Subtract(new TimeSpan(0, 0, 10)) > _lastPositionUpdateTime)
	        {
                _mapPosition = await PositionHelper.GetMapPosition();
                _lastPositionUpdateTime = DateTime.Now;
	        }
            var userPosition = _mapPosition;

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
                if (PositionHelper.LocationValid(position.ToGeolocatorPosition()))
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
                GroupMap.MapType = MapType.Hybrid; //This doesn't seem to work on android
                //GroupMap.MapCenter = mapCenterPosition;
                GroupMap.MapRegion = MapSpan.FromCenterAndRadius(mapCenterPosition.ToTkPosition(), Distance.FromMiles(radius).ToTkDistance());
                GroupMap.Pins = customPins;
            });
            return;
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

	    private void LeaveGroupButton_OnClicked(object sender, EventArgs e)
	    {
            MessagingCenter.Send<MessagingCenterSender>(new MessagingCenterSender(), LocationSender.LeaveGroupMsg);
            MessagingCenter.Send<MessagingCenterSender>(new MessagingCenterSender(), LocationSender.EndGroupMsg);
            MessagingCenter.Send<MessagingCenterSender>(new MessagingCenterSender(), LocationSender.ThisUserLeftGroupMsg);
        }

        private void HideSpinnerShowMap()
        {
            Spinner.IsRunning = false;
            Spinner.IsVisible = false;
        }

	    private void ViewARButton_OnClicked(object sender, EventArgs e)
	    {
#if (__ANDROID__)
            var userPhoneNumber = SettingsHelper.GetPhoneNumber();
            Intent i = new Intent();
            i.SetAction(Intent.ActionView);
	        i.SetFlags(ActivityFlags.NewTask);
	        var uri =
	            Android.Net.Uri.Parse(
	                string.Format("http://whereschrisardata.azurewebsites.net/api/GroupData?code=MG80/ufNZ3YbsUw6Q/tJelkgtcSoEaD7OdB1hHUPq6zZdrM2M3Xb/A==&phone={0}", userPhoneNumber));
            i.SetDataAndType(uri, "application/mixare-json");
	        Android.App.Application.Context.StartActivity(i);
#endif
#if (__IOS__)
	        NSUrl request = new NSUrl("mixare://");
            var isOpened = UIApplication.SharedApplication.OpenUrl(request);
#endif
        }
	}
}