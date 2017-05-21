using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Azure.Mobile.Analytics;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Settings;
using StayTogether.Classes;
using StayTogether.Helpers;
using StayTogether.Models;
using WheresChris.Helpers;
using Xamarin.Forms;

namespace StayTogether
{
    public delegate void EventHandler<in TLostEventArgs>(object sender, TLostEventArgs e);
    public delegate void EventHandler(object sender, EventArgs e);

	public class LocationSender
	{
	    private static LocationSender _instance;

	    public static async Task<LocationSender> GetInstanceAsync()
	    {
            //Make m new instance if null when requested
	        if (_instance == null)
	        {
	            _instance = new LocationSender();
	        }

            //if the instance is initialized, return it
	        if (_instance.IsInitialized) return _instance;

            //if instance not initialized, but we have a valid number now, initialize it
            //on ios, when app is first run, there will not be a phone number.  Once user enters it
            //we can start sending messages
            var phoneNumber = SettingsHelper.GetPhoneNumber();
            if (phoneNumber.IsValidPhoneNumber())
	        {
	            await _instance.InitializeSignalRAsync();
	        }

	        return _instance;
	    }

	    public static LocationSender GetInstance()
	    {
            //Make m new instance if null when requested
            if (_instance == null)
            {
                _instance = new LocationSender();
            }

            //if the instance is initialized, return it
            if (_instance.IsInitialized) return _instance;

            //if instance not initialized, but we have a valid number now, initialize it
            //on ios, when app is first run, there will not be a phone number.  Once user enters it
            //we can start sending messages
            var phoneNumber = SettingsHelper.GetPhoneNumber();
            if (phoneNumber.IsValidPhoneNumber())
            {
                _instance.Initialize();
            }

            return _instance;
        }

	    public event EventHandler OnPhoneNumberMissing;
	    public event EventHandler<LostEventArgs> OnSomeoneIsLost;
        public event EventHandler<InvitedEventArgs> OnGroupInvitationReceived;
        public event EventHandler OnGroupJoined;
        public event EventHandler OnGroupDisbanded;
        public event EventHandler<MemberMinimalEventArgs> OnSomeoneLeft;
        public event EventHandler<MemberMinimalEventArgs> OnSomeoneAlreadyInAnotherGroup;

        public const string MemberAlreadyInGroupMsg = "MEMBERINGROUP";
	    public const string SomeoneIsLostMsg = "SOMEONEISLOST";
	    public const string GroupJoinedMsg = "GROUPJOINED";
	    public const string GroupDisbandedMsg = "GROUPDISBANDED";
	    public const string GroupCreatedMsg = "GROUPCREATED";
	    public const string SomeoneAddedToGroupMsg = "SOMEONEADDEDTOGROUP";
	    public const string SomeoneLeftMsg = "SOMEONELEFT";
	    public const string ThisUserLeftGroupMsg = "ILEFTGROUP";
	    public const string SomeoneAlreadyInAnotherGroupMsg = "SOMEONEALREADYINANOTHERGROUP";
	    public const string GroupInvitationReceivedMsg = "GROUPINVITATIONRECEIVED";
	    public const string LocationSentMsg = "LOCATIONSENT";
	    public const string GroupPositionUpdateMsg = "GROUPPOSITIONUPDATE";
        


        public bool InAGroup { get; set; }
        public bool GroupLeader { get; set; }
        public bool IsInitialized { get; set; }
        public List<GroupMemberSimpleVm> GroupMembers { get; set; }

        private HubConnection _hubConnection;
	    private IHubProxy _chatHubProxy;
	    private IGeolocator _geoLocator;
	    private string _phoneNumber;
        private string _groupId = ""; //Creator of group's phone number
        

        public LocationSender ()
	    {
            try
            {
                GetNickname();
                GetPhoneNumber();
            }
            catch (Exception ex)
            {
#if (DEBUG)
Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_LocationSender", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	    public async Task InitializeAsync()
	    {
            if (!IsInitialized)
            {
                await InitializeSignalRAsync();
            }
        }

	    public void Initialize()
	    {
	        InitializeAsync().Wait();
	    }

	    public async Task InitializeSignalRAsync()
        {
            try
            {
                // Connect to the server
                _hubConnection = new HubConnection("https://staytogetherserver.azurewebsites.net/");//mike
                                                                                                    //_hubConnection = new HubConnection("http://162.231.59.41/StayTogetherServer/");//mike
                _hubConnection.Headers.Add("AuthToken", "x0y2!tJyHR%$Sip@*%amaGxvs");

                // Create a proxy to the 'ChatHub' SignalR Hub
                _chatHubProxy = _hubConnection.CreateHubProxy("StayTogetherHub");
                //I think this string will be the name of Jeff's main class

                // Wire up a handler for the 'UpdateChatMessage' for the server
                // to be called on our client
                _chatHubProxy.On<string>("UpdateGroupId", UpdateGroupId);
                _chatHubProxy.On<LostMemberVm>("SomeoneIsLost", SomeoneIsLost);
                _chatHubProxy.On<string>("GroupDisbanded", GroupDisbanded);
                _chatHubProxy.On<string, string>("MemberLeft", OnMemberLeftGroup);
                _chatHubProxy.On<string, string>("GroupInvitation", OnGroupInvitation);
                _chatHubProxy.On<string, string>("MemberAlreadyInGroup", OnMemberAlreadyInGroup);
                _chatHubProxy.On<List<GroupMemberSimpleVm>>("GroupPositionUpdate", OnGroupPositionUpdate);
                _chatHubProxy.On<string>("RequestMemberLocations", async s => await RequestMemberPositions(s));
                
                // Start the connection
                await _hubConnection.Start();

                await SetUpLocationEvents();

                IsInitialized = true;
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_InitializeSignalRAsync", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	    private async Task InvokeChatHubProxy(string method, params object[] args)
	    {
	        if (CanSend())
	        {
	            await _chatHubProxy.Invoke(method, args);
	        }
	    }

        public async Task<T> InvokeChatHubProxy<T>(string method, params object[] args) where T:new()
        {
            if (CanSend())
            {
                return await _chatHubProxy.Invoke<T>(method, args);
            }
            return new T();
        }

        private bool CanSend()
	    {
	        if (_phoneNumber.IsValidPhoneNumber() && this.IsInitialized) return true;

	        OnPhoneNumberMissing?.Invoke(this, new EventArgs());
	        return false;
	    }

	    private async Task RequestMemberPositions(string leaderPhoneNumber)
	    {
            try
            {
                await SendUpdatePosition();
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_RequestMemberPositions", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	    public async Task SetUpLocationEvents()
        {

            try
            {
                _geoLocator = CrossGeolocator.Current;

                _geoLocator.DesiredAccuracy = 100; //100 is new default

                if (!_geoLocator.IsGeolocationEnabled || !_geoLocator.IsGeolocationAvailable) return;

                //Instructions for geoolocator say we need to do this before setting the listener
                var position = await _geoLocator.GetPositionAsync(new TimeSpan(0,0,10));

                await _geoLocator.StartListeningAsync(TimeSpan.FromSeconds(5), 10, false, new Plugin.Geolocator.Abstractions.ListenerSettings
                {
                    ActivityType = ActivityType.Fitness,
                    AllowBackgroundUpdates = true,
                    DeferLocationUpdates = false,
                    DeferralDistanceMeters = 1,
                    DeferralTime = TimeSpan.FromSeconds(1),
                    ListenForSignificantChanges = false,
                    PauseLocationUpdatesAutomatically = false
                });

                _geoLocator.PositionChanged +=
                    async delegate (object o, PositionEventArgs args) { await LocatorOnPositionChanged(o, args); };
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_SetUpLocationEvents", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

        private async Task LocatorOnPositionChanged(object sender, PositionEventArgs positionEventArgs)
        {
            try
            {
                Analytics.TrackEvent($"LocationSender_LocatorOnPositionChanged");
                await SendUpdatePosition();
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_LocatorOnPositionChanged", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	    public async Task SendUpdatePosition()
	    {
            try
            {
                var currentPosition = await PositionHelper.GetMapPosition();
                if (!currentPosition.HasValue)
                {
                    Analytics.TrackEvent("LocationSender_LocatorOnPositionChanged_PositionNull");
                    return;
                }
                if (!PositionHelper.LocationValid(currentPosition.Value))
                {
                    Analytics.TrackEvent("LocationSender_LocatorOnPositionChanged_PositionInvalid");
                    return;
                }

                var groupMemberVm = new GroupMemberVm()
                {
                    Latitude = currentPosition.Value.Latitude, //positionEventArgs.Position.Latitude,
                    Longitude = currentPosition.Value.Longitude, //positionEventArgs.Position.Longitude,
                    PhoneNumber = _phoneNumber,
                    Name = _nickName
                };

                Analytics.TrackEvent("LocationSender_SendUpdatePosition");
                await SendUpdatePosition(groupMemberVm);
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_SendUpdatePosition", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }


	    private void OnGroupPositionUpdate(List<GroupMemberSimpleVm> groupMembers)
        {
	        try
	        {
	            if (GroupMembers != null)
	            {
	                GroupMembers.Clear();
	                GroupMembers.AddRange(groupMembers);
	            }
	            MessagingCenter.Send<LocationSender>(this, GroupPositionUpdateMsg);
	        }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_OnGroupPositionUpdate", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

        private void OnMemberAlreadyInGroup(string memberPhoneNumber, string memberName)
	    {
            try
            {
                OnSomeoneAlreadyInAnotherGroup?.Invoke(this, new MemberMinimalEventArgs
                {
                    Name = memberName,
                    PhoneNumber = memberPhoneNumber
                });

                MessagingCenter.Send<LocationSender>(this, MemberAlreadyInGroupMsg);
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_OnMemberAlreadyInGroup", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	    private void OnMemberLeftGroup(string memberPhoneNumber, string memberName)
	    {
            try
            {
                if (memberPhoneNumber == _phoneNumber) return; //I dont need to notify myself that I left
                OnSomeoneLeft?.Invoke(this, new MemberMinimalEventArgs
                {
                    Name = memberName,
                    PhoneNumber = memberPhoneNumber
                });
                MessagingCenter.Send<LocationSender>(this, SomeoneLeftMsg);
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_OnMemberLeftGroup", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	    private void GroupDisbanded(string groupId)
	    {
            try
            {
                InAGroup = false;
                _groupId = "";
                GroupLeader = false;
                GroupMembers = null;

                //AddNotification("Group Disbanded", "Your Group has been disbanded");
                OnGroupDisbanded?.Invoke(this, new EventArgs());
                MessagingCenter.Send<LocationSender>(this, GroupDisbandedMsg);
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_GroupDisbanded", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

        private readonly InvitationList _invitationList = new InvitationList();
	    private string _nickName;

	    private void OnGroupInvitation(string phoneNumber, string name)
        {
            try
            {
                if (phoneNumber.CleanPhoneNumber() == _phoneNumber.CleanPhoneNumber()) return;//don't invite myself to a group

                OnGroupInvitationReceived?.Invoke(this, new InvitedEventArgs
                {
                    Name = name,
                    GroupId = phoneNumber
                });

                _invitationList.AddInvitation(new InvitationVm
                {
                    Name = name,
                    PhoneNumber = phoneNumber,
                    ReceivedTime = DateTime.Now
                });
                MessagingCenter.Send<LocationSender>(this, GroupInvitationReceivedMsg);
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_OnGroupInvitation", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	    public List<InvitationVm> GetInvitations(int hours = 3)
	    {
            try
            {
                _invitationList.Clean();
                return _invitationList;
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_GetInvitations", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
	        return null;
	    }

        public void UpdateGroupId(string id)
	    {
            try
            {
                OnGroupJoined?.Invoke(this, new EventArgs());
                _groupId = id;
                InAGroup = true;
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_UpdateGroupId", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	    public void SomeoneIsLost(LostMemberVm lostMemberVm)//string phoneNumber, string latitude, string longitude, string name, double distance
	    {

            try
            {
                if (!PositionHelper.LocationValid(lostMemberVm)) return;
                if (lostMemberVm.LostDistance > 5280 * 60) return;
                if (string.IsNullOrWhiteSpace(_groupId)) return;

                OnSomeoneIsLost?.Invoke(this, new LostEventArgs
                {
                    GroupMember = new GroupMemberVm
                    {
                        PhoneNumber = lostMemberVm.PhoneNumber,
                        Name = lostMemberVm.Name,
                        Latitude = Convert.ToDouble(lostMemberVm.Latitude),
                        Longitude = Convert.ToDouble(lostMemberVm.Longitude),
                        LostDistance = lostMemberVm.LostDistance
                    }
                });
                MessagingCenter.Send<LocationSender>(this, SomeoneIsLostMsg);
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_SomeoneIsLost", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }         
	    }



	    public async Task StartOrAddToGroup(GroupVm groupVm)
	    {
            try
            {
                if (GroupLeader && InAGroup)
                {
                    await AddToGroup(groupVm);
                }
                else if (!InAGroup)
                {
                    await StartGroup(groupVm);
                }
                GroupMembers = new List<GroupMemberSimpleVm>();
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_StartOrAddToGroup", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }
        
        public async Task StartGroup(GroupVm groupVm)
	    {
            try
            {
                await InvokeChatHubProxy("CreateGroup", groupVm);
                GroupLeader = true;
                InAGroup = true;
                _groupId = _phoneNumber;
                MessagingCenter.Send<LocationSender>(this, GroupCreatedMsg);
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_StartGroup", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	    public async Task AddToGroup(GroupVm groupVm)
	    {
            try
            {
                await InvokeChatHubProxy("AddToGroup", groupVm);
                MessagingCenter.Send<LocationSender>(this, SomeoneAddedToGroupMsg);
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_AddToGroup", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	    public async Task EndGroup()
	    {
            try
            {
                if (InAGroup && GroupLeader)
                {
                    await InvokeChatHubProxy("EndGroup", _phoneNumber);
                    InAGroup = false;
                    GroupLeader = false;
                    _groupId = "";
                    GroupMembers = null;
                    MessagingCenter.Send<LocationSender>(this, GroupDisbandedMsg);
                }
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_EndGroup", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

        public async Task LeaveGroup()
        {
            try
            {
                if (InAGroup && !GroupLeader)
                {
                    await InvokeChatHubProxy("LeaveGroup", _groupId, _phoneNumber);
                    InAGroup = false;
                    GroupLeader = false;
                    _groupId = "";
                    GroupMembers = null;
                    MessagingCenter.Send<LocationSender>(this, ThisUserLeftGroupMsg);
                }
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_LeaveGroup", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

        /// <summary>
        /// Confirms this user is joining the group
        /// </summary>
        /// <param name="phoneNumber">The Group Leader Phone Number</param>
        /// <param name="name">The Group Leader's Name. Could be empty</param>
        /// <returns></returns>
	    public async Task<bool> ConfirmGroupInvitation(string phoneNumber, string name)
        {
            try
            {
                if (InAGroup) return false;//Don't allow them to join a group

                UpdateGroupId(phoneNumber);

                var currentPosition = await PositionHelper.GetMapPosition();
                if (!currentPosition.HasValue)
                {
                    Analytics.TrackEvent("LocationSender_ConfirmGroupInvitation_PositionNull");
                    return false;
                }

                if (!PositionHelper.LocationValid(currentPosition.Value))
                {
                    Analytics.TrackEvent("LocationSender_ConfirmGroupInvitation_PositionInvalid");
                }

                var groupMemberVm = new GroupMemberVm
                {
                    GroupId = phoneNumber,
                    PhoneNumber = _phoneNumber,
                    Latitude = currentPosition.Value.Latitude,
                    Longitude = currentPosition.Value.Longitude,
                    InvitationConfirmed = true
                };
                await InvokeChatHubProxy("confirmGroupInvitation", groupMemberVm);
                GroupMembers = new List<GroupMemberSimpleVm>();
                await SendUpdatePosition();//Send out my current position so group member's maps will update
                MessagingCenter.Send<LocationSender>(this, GroupJoinedMsg);
                return true;
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_ConfirmGroupInvitation", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
            return false;
        }

	    public async Task SendUpdatePosition(GroupMemberVm groupMemberVm)
	    {
            try
            {
                groupMemberVm.PhoneNumber = _phoneNumber;
                groupMemberVm.Name = _nickName;
                groupMemberVm.GroupId = _groupId;
                if (GroupLeader && InAGroup)
                {
                    groupMemberVm.InvitationConfirmed = true;
                }
                await InvokeChatHubProxy("updatePosition", groupMemberVm);

                MessagingCenter.Send(this, LocationSentMsg);
                Analytics.TrackEvent("LocationSender_SendUpdatePosition_groupMemberVm");
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_SendUpdatePosition", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	    public async Task<List<GroupMemberVm>> GetMembers(GroupMemberVm groupMemberVm)
	    {
            try
            {
                if (InAGroup)
                {
                    return await InvokeChatHubProxy<List<GroupMemberVm>>("GetGroupMembers", groupMemberVm);
                }
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_GetMembers", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }

            return new List<GroupMemberVm>();
	    }

        private void GetNickname()
        {
            try
            {
                _nickName = CrossSettings.Current.GetValueOrDefault<string>("nickname");
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_GetNickname", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	    private void GetPhoneNumber()
        {
            try
            {
                _phoneNumber = SettingsHelper.GetPhoneNumber();
                if (string.IsNullOrWhiteSpace(_phoneNumber))
                {
                    //Todo:  figure out a way to do this
                    //AddNotification("Where's Chris PhoneNumber", "Please Add your Phone Number in settings");
                }
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_GetPhoneNumber", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	}

    public class LostEventArgs : EventArgs
    {
        public GroupMemberVm GroupMember { get; set; }
    }

    public class InvitedEventArgs: EventArgs
    {
        public string Name { get; set; }
        public string GroupId { get; set; }
    }
    public class MemberMinimalEventArgs : EventArgs
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
    }
}

