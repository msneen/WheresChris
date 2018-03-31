using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
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
using Android.Content.PM;
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
        private readonly Interval _positionInitializationInterval = new Interval();
	    private Xamarin.Forms.Maps.Position? _mapPosition;
        private DateTime _lastPositionUpdateTime = DateTime.Now;
	    private Xamarin.Forms.Maps.Position _mapCenterPosition;
	    private double _radius = 1000; //meters
	    private bool _inAGroup = false;

	    public MapPage ()
		{
		    try
		    {
                Title = "Where's Chris - Map";
                InitializeComponent ();
                GroupMap.MapType = MapType.Street;
    
                     
                InitializeMessagingCenterSubscriptions();
                SetFormEnabled(false);
		    }
		    catch(Exception ex)
		    {
		        Crashes.TrackError(ex, new Dictionary<string, string>
		        {
		            {"Source", ex.Source },
		            { "stackTrace",ex.StackTrace}
		        });
		    }
		}



	    private void SetMapInitialPosition()
	    {
	        try
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
	        catch(Exception ex)
	        {
	            Crashes.TrackError(ex, new Dictionary<string, string>
	            {
	                {"Source", ex.Source },
	                { "stackTrace",ex.StackTrace}
	            });
	        }
        }

	    private Task SetFormEnabled(bool isSelected)
	    {
	        try
	        {
	            _inAGroup = isSelected;
	            GroupInfo.IsVisible = _inAGroup;
	        }
	        catch(Exception ex)
	        {
	            Crashes.TrackError(ex, new Dictionary<string, string>
	            {
	                {"Source", ex.Source },
	                { "stackTrace",ex.StackTrace}
	            });
	        }
	        return Task.CompletedTask;
	    }


	    protected override void OnAppearing()
	    {
	        try
	        {
	            if (_mapInitialized) return;
	            SetMapInitialPosition();
	            _positionInitializationInterval.SetInterval(InitializeMap().Wait, 500);
	        }
	        catch(Exception ex)
	        {
	            Crashes.TrackError(ex, new Dictionary<string, string>
	            {
	                {"Source", ex.Source },
	                { "stackTrace",ex.StackTrace}
	            });
	        }
        }

        /// <summary>
        /// This event is fired when a group position update is received from the server
        /// </summary>
	    private void InitializeMessagingCenterSubscriptions()
	    {
	        try
	        {
                MessagingCenter.Subscribe<LocationSender, List <GroupMemberSimpleVm >> (this, LocationSender.GroupPositionUpdateMsg, (sender, groupMemberSimpleVm) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (!GroupMap.IsVisible) return;
                        //AddMembersButton.TextColor = AddMembersButton.TextColor == Color.Blue ? Color.Black : Color.Blue;
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
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await SetFormEnabled(false);
                        await ResetMap();
                    });
                });
                //This user left the group
                MessagingCenter.Subscribe<LocationSender>(this, LocationSender.ThisUserLeftGroupMsg,
                (sender) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await SetFormEnabled(false);
                        await ResetMap();
                    });
                });
                MessagingCenter.Subscribe<MessagingCenterSender, Plugin.Geolocator.Abstractions.Position>(this, LocationSender.PositionUpdatedMsg, (sender, position) =>
                {
                    _mapPosition = PositionHelper.GetMapPosition(position);
                    _lastPositionUpdateTime = DateTime.Now;
                });
	            MessagingCenter.Subscribe<LocationSender, GroupMemberSimpleVm>(this, LocationSender.SomeoneLeftMsg,
	                (sender, groupMemberSimpleVm) =>
	                {
	                    Device.BeginInvokeOnMainThread(() =>
	                    {
	                        //Remove pin
	                        var pins = GroupMap.Pins.ToList();

                            var userPin  = pins.FirstOrDefault(x=>x.Title == ContactsHelper.NameOrPhone(groupMemberSimpleVm.PhoneNumber,
                                                                      groupMemberSimpleVm.Name));
	                        if(userPin == null) return;
	                        pins.Remove(userPin);
	                        GroupMap.Pins = pins;
	                    });
	                });
	        }
	        catch(Exception ex)
	        {
	            Crashes.TrackError(ex, new Dictionary<string, string>
	            {
	                {"Source", ex.Source },
	                { "stackTrace",ex.StackTrace}
	            });
	        }
        }

        private async Task<bool> InitializeMap()
        {        

            try
            {
                var justMeList = await GetMyPositionList();
                if (justMeList == null)
                {
                    _mapInitialized = false;
                    _positionInitializationInterval.SetInterval(InitializeMap().Wait, 3000);
                    return true;
                };
                         
                 UpdateMap(justMeList);
                 HideSpinnerShowMap();
                _mapInitialized = true;
                return true;
            }
            catch(Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    {"Source", ex.Source },
                    { "stackTrace",ex.StackTrace}
                });
            }
            return false;
        }

	    private async Task ResetMap()
	    {
	        var justMeList = await GetMyPositionList();
	        UpdateMap(justMeList);
	    }

	    private async Task<List<GroupMemberSimpleVm>> GetMyPositionList()
	    {
	        try
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
	            if (!userPosition.Value.LocationValid()) return null;

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
	        catch(Exception ex)
	        {
	            Crashes.TrackError(ex, new Dictionary<string, string>
	            {
	                {"Source", ex.Source },
	                { "stackTrace",ex.StackTrace}
	            });
	        }
	        return null;
	    }

	    private void UpdateMap(List<GroupMemberSimpleVm> groupMembers)
	    {    
	        try
	        {
                var userPhoneNumber = SettingsHelper.GetPhoneNumber();
                var customPins = new List<TKCustomMapPin>();

                foreach (var groupMember in groupMembers)
                {
                    var position = new Position(groupMember.Latitude, groupMember.Longitude);
                    if (position.ToGeolocatorPosition().LocationValid())
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

                _mapCenterPosition = PositionHelper.GetMapCenter(groupMembers);
	            if (!_mapCenterPosition.LocationValid()) return;

	            _radius = PositionHelper.GetRadius(groupMembers, _mapCenterPosition);

                Device.BeginInvokeOnMainThread(() =>
                {
                    GroupMap.MapType = MapType.Street;                              
                    GroupMap.MapRegion = MapSpan.FromCenterAndRadius(_mapCenterPosition.ToTkPosition(), Distance.FromMiles(_radius).ToTkDistance());
                    GroupMap.Pins = customPins;
                });
	        }
	        catch(Exception ex)
	        {
	            Crashes.TrackError(ex, new Dictionary<string, string>
	            {
	                {"Source", ex.Source },
	                { "stackTrace",ex.StackTrace}
	            });
	        }

	    }

	    private async Task AddMembers()
	    {
	        try
	        {
	            var addMemberPage = new AddMemberPage();
	            await Navigation.PushAsync(addMemberPage);
	        }
	        catch(Exception ex)
	        {
	            Crashes.TrackError(ex, new Dictionary<string, string>
	            {
	                {"Source", ex.Source },
	                { "stackTrace",ex.StackTrace}
	            });
	        }
	    }

	    private async Task ViewMembers()
	    {
	        try
	        {
	            var memberPage = new MemberPage();
	            await memberPage.RefreshMembers();
	            await Navigation.PushAsync(memberPage);
	        }
	        catch(Exception ex)
	        {
	            Crashes.TrackError(ex, new Dictionary<string, string>
	            {
	                {"Source", ex.Source },
	                { "stackTrace",ex.StackTrace}
	            });
	        }
	    }

	    private async Task LeaveGroup()
	    {
	        try
	        {
                MessagingCenter.Send<MessagingCenterSender>(new MessagingCenterSender(), LocationSender.LeaveGroupMsg);
                MessagingCenter.Send<MessagingCenterSender>(new MessagingCenterSender(), LocationSender.EndGroupMsg);
                MessagingCenter.Send<MessagingCenterSender>(new MessagingCenterSender(), LocationSender.ThisUserLeftGroupMsg);
	            await InitializeMap();
	        }
	        catch(Exception ex)
	        {
	            Crashes.TrackError(ex, new Dictionary<string, string>
	            {
	                {"Source", ex.Source },
	                { "stackTrace",ex.StackTrace}
	            });
	        }
	    }

        private void HideSpinnerShowMap()
        {
            try
            {
                Spinner.IsRunning = false;
                Spinner.IsVisible = false;
            }
            catch(Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    {"Source", ex.Source },
                    { "stackTrace",ex.StackTrace}
                });
            }
        }

        private void ViewARButton_OnClicked(object sender, EventArgs e)
	    {
	        try
	        {
#if (__ANDROID__)
                var userPhoneNumber = SettingsHelper.GetPhoneNumber();
                Intent i = new Intent();
                i.SetAction(Intent.ActionView);
	            i.SetFlags(ActivityFlags.NewTask);
	            var uri =
	                Android.Net.Uri.Parse(
	                    $"http://whereschrisardata.azurewebsites.net/api/GroupData?code=MG80/ufNZ3YbsUw6Q/tJelkgtcSoEaD7OdB1hHUPq6zZdrM2M3Xb/A==&phone={userPhoneNumber}");
                i.SetDataAndType(uri, "application/mixare-json");
	            Android.App.Application.Context.StartActivity(i);
#endif
#if (__IOS__)
	            NSUrl request = new NSUrl("mixare://");
                var isOpened = UIApplication.SharedApplication.OpenUrl(request);
#endif
	        }
	        catch(Exception ex)
	        {
	            Crashes.TrackError(ex, new Dictionary<string, string>
	            {
	                {"Source", ex.Source },
	                { "stackTrace",ex.StackTrace}
	            });
	        }

        }

	    private void MyLocation_OnTapped(object sender, EventArgs e)
	    {
	        try
	        {
	            if(Math.Abs(_radius) < 0.00001) return;

	            var centerPosition = _mapCenterPosition;
	            if(centerPosition.LocationValid() && _mapPosition.HasValue)
	            {
	                centerPosition = _mapPosition.Value;
	            }

	            if(!centerPosition.LocationValid()) return;

	            GroupMap.MapRegion = MapSpan.FromCenterAndRadius(centerPosition.ToTkPosition(), Distance.FromMiles(_radius).ToTkDistance());	    
	        }
	        catch(Exception ex)
	        {
	            Crashes.TrackError(ex, new Dictionary<string, string>
	            {
	                {"Source", ex.Source },
	                { "stackTrace",ex.StackTrace}
	            });
	        }
}

	    private async Task GroupInfo_OnTapped(object sender, EventArgs e)
	    {
	        try
	        {
	            if(_inAGroup)
	            {
	                var action = await DisplayActionSheet("Group", "Cancel", null, "Add", "Members", "Leave");
	                switch(action)
	                {
                        case "Add":
                            await AddMembers();
                            break;
                        case "Members":
                            await ViewMembers();
                            break;
                        case "Leave":
                            await LeaveGroup();
                            break;
                        default:
                            break;
	                }
	            }
	        }
	        catch(Exception ex)
	        {
	            Crashes.TrackError(ex, new Dictionary<string, string>
	            {
	                {"Source", ex.Source },
	                { "stackTrace",ex.StackTrace}
	            });
	        }

	    }
	}
}