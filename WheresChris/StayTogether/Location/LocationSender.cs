using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Azure.Mobile.Analytics;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.LocalNotifications;
using Plugin.Settings;
using StayTogether.Classes;
using StayTogether.Helpers;
using StayTogether.Models;
using Xamarin.Forms;

namespace StayTogether
{
    public delegate void EventHandler<in TLostEventArgs>(object sender, TLostEventArgs e);
    public delegate void EventHandler(object sender, EventArgs e);

	public class LocationSender
	{
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
	        GetNickname();
            GetPhoneNumber();
	    }

	    public void InitializeSignalRAsync()
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
            _chatHubProxy.On<string, string>("BroadcastMessage", ReceiveGroupMessage);
            _chatHubProxy.On<string>("UpdateGroupId", UpdateGroupId);
            _chatHubProxy.On<LostMemberVm>("SomeoneIsLost", SomeoneIsLost);
            _chatHubProxy.On<string>("GroupDisbanded", GroupDisbanded);
            _chatHubProxy.On<string, string>("MemberLeft", OnMemberLeftGroup);
            _chatHubProxy.On<string, string>("GroupInvitation", OnGroupInvitation);
            _chatHubProxy.On<string, string>("MemberAlreadyInGroup", OnMemberAlreadyInGroup);
            _chatHubProxy.On<List<GroupMemberSimpleVm>>("GroupPositionUpdate", OnGroupPositionUpdate);

            // Start the connection
            _hubConnection.Start().Wait();

            SetUpLocationEvents();

	        IsInitialized = true;
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
            catch(Exception) { }

        }

        private void OnMemberAlreadyInGroup(string memberPhoneNumber, string memberName)
	    {
            OnSomeoneAlreadyInAnotherGroup?.Invoke(this, new MemberMinimalEventArgs
            {
                Name = memberName,
                PhoneNumber = memberPhoneNumber
            });
	        
            MessagingCenter.Send<LocationSender>(this, MemberAlreadyInGroupMsg);
        }

	    private void OnMemberLeftGroup(string memberPhoneNumber, string memberName)
	    {
            OnSomeoneLeft?.Invoke(this, new MemberMinimalEventArgs
            {
                Name = memberName,
                PhoneNumber = memberPhoneNumber
            });
            MessagingCenter.Send<LocationSender>(this, SomeoneLeftMsg);
        }


	    public void SetUpLocationEvents()
	    {

	        _geoLocator = CrossGeolocator.Current;

	        _geoLocator.DesiredAccuracy = 1; //100 is new default

	        if (!_geoLocator.IsGeolocationEnabled || !_geoLocator.IsGeolocationAvailable) return;

            _geoLocator.StartListeningAsync(TimeSpan.FromSeconds(5), 5, false, new Plugin.Geolocator.Abstractions.ListenerSettings
            {
                ActivityType = Plugin.Geolocator.Abstractions.ActivityType.Fitness,
                AllowBackgroundUpdates = true,
                DeferLocationUpdates = true,
                DeferralDistanceMeters = 1,
                DeferralTime = TimeSpan.FromSeconds(1),
                ListenForSignificantChanges = true,
                PauseLocationUpdatesAutomatically = false
            });

            _geoLocator.PositionChanged += LocatorOnPositionChanged;
        }

	    private async void LocatorOnPositionChanged(object sender, PositionEventArgs positionEventArgs)
	    {
            var currentPosition = await PositionHelper.GetMapPosition();
	        if (!PositionHelper.LocationValid(currentPosition))
	        {
                Analytics.TrackEvent("LocationSender_LocatorOnPositionChanged_PositionInvalid");
                return;
	        }

	        var groupMemberVm = new GroupMemberVm()
	        {
	            Latitude = positionEventArgs.Position.Latitude,
	            Longitude = positionEventArgs.Position.Longitude,
	            PhoneNumber = _phoneNumber,
                Name = _nickName
	        };

	        SendUpdatePosition(groupMemberVm);
            Analytics.TrackEvent("LocationSender_LocatorOnPositionChanged_PositionSent");
        }



	    private void GroupDisbanded(string groupId)
	    {
	        InAGroup = false;
	        _groupId = "";
            GroupLeader = false;
	        GroupMembers = null;
            
            //AddNotification("Group Disbanded", "Your Group has been disbanded");
            OnGroupDisbanded?.Invoke(this, new EventArgs());
            MessagingCenter.Send<LocationSender>(this, GroupDisbandedMsg);
        }

        private readonly InvitationList _invitationList = new InvitationList();
	    private string _nickName;

	    private void OnGroupInvitation(string phoneNumber, string name)
        {
            // TODO: consider cleaning phoneNumber
            if (phoneNumber == _phoneNumber) return;//don't invite myself to a group

            OnGroupInvitationReceived?.Invoke(this, new InvitedEventArgs
            {
                Name = name,
                GroupId =   phoneNumber
            });

            _invitationList.AddInvitation(new InvitationVm
            {
                Name = name,
                PhoneNumber = phoneNumber,
                ReceivedTime = DateTime.Now
            });
            MessagingCenter.Send<LocationSender>(this, GroupInvitationReceivedMsg);
        }

	    public List<InvitationVm> GetInvitations(int hours = 3)
	    {
	        _invitationList.Clean();
           return _invitationList;
        }

        public void UpdateGroupId(string id)
	    {
            OnGroupJoined?.Invoke(this, new EventArgs());
            _groupId = id;
	        InAGroup = true;
	    }

	    public void SomeoneIsLost(LostMemberVm lostMemberVm)//string phoneNumber, string latitude, string longitude, string name, double distance
        {
	        if (!string.IsNullOrWhiteSpace(_groupId))
	        {
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
	    }

	    public void ReceiveGroupMessage(string phoneNumber, string message)
	    {
	        AddNotification("Where's Chris Update", message);
	    }


	    private void AddNotification(string title, string message)
	    {
            CrossLocalNotifications.Current.Show(title, message);
        }

	    public async Task StartOrAddToGroup(GroupVm groupVm)
	    {
	        if (GroupLeader && InAGroup)
	        {
	            await AddToGroup(groupVm);
	        }
	        else if(!InAGroup)
	        {
	            await StartGroup(groupVm);
	        }
            GroupMembers = new List<GroupMemberSimpleVm>();
	    }

	    public async Task StartGroup(GroupVm groupVm)
	    {
            await _chatHubProxy.Invoke("CreateGroup", groupVm);
	        GroupLeader = true;
	        InAGroup = true;
            _groupId = _phoneNumber;
            MessagingCenter.Send<LocationSender>(this, GroupCreatedMsg);
        }

	    public async Task AddToGroup(GroupVm groupVm)
	    {
            await _chatHubProxy.Invoke("AddToGroup", groupVm);
            MessagingCenter.Send<LocationSender>(this, SomeoneAddedToGroupMsg);
        }

	    public async Task EndGroup()
	    {
	        if (InAGroup && GroupLeader)
	        {
	            await _chatHubProxy.Invoke("EndGroup", _phoneNumber);
	            InAGroup = false;
	            GroupLeader = false;
	            _groupId = "";
	            GroupMembers = null;
                MessagingCenter.Send<LocationSender>(this, GroupDisbandedMsg);
            }
	    }

        public async Task LeaveGroup()
        {
            if (InAGroup && !GroupLeader)
            {
                await _chatHubProxy.Invoke("LeaveGroup", _groupId, _phoneNumber);
                InAGroup = false;
                GroupLeader = false;
                _groupId = "";
                GroupMembers = null;
                MessagingCenter.Send<LocationSender>(this, ThisUserLeftGroupMsg);
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
            if (InAGroup) return false;//Don't allow them to join a group

            UpdateGroupId(phoneNumber);

            var currentPosition = await PositionHelper.GetMapPosition();
            if (!PositionHelper.LocationValid(currentPosition))
            {
                Analytics.TrackEvent("LocationSender_ConfirmGroupInvitation_PositionInvalid");

            }

            var groupMemberVm = new GroupMemberVm
            {
                GroupId = phoneNumber,
                PhoneNumber = _phoneNumber,
                Latitude = currentPosition.Latitude,
                Longitude = currentPosition.Longitude,
                InvitationConfirmed = true
            };
            await _chatHubProxy.Invoke("confirmGroupInvitation", groupMemberVm);
            GroupMembers = new List<GroupMemberSimpleVm>();
            MessagingCenter.Send<LocationSender>(this, GroupJoinedMsg);
            return true;
        }

	    public void SendUpdatePosition(GroupMemberVm groupMemberVm)
	    {
	        groupMemberVm.PhoneNumber = _phoneNumber;
	        groupMemberVm.GroupId = _groupId;
            if(GroupLeader && InAGroup)
            {
                groupMemberVm.InvitationConfirmed = true;
            }
            _chatHubProxy.Invoke("updatePosition", groupMemberVm);
            MessagingCenter.Send<LocationSender>(this, LocationSentMsg);
        }

	    public async Task<List<GroupMemberVm>> GetMembers(GroupMemberVm groupMemberVm)
	    {
	        if (InAGroup)
	        {
	            return await _chatHubProxy.Invoke<List<GroupMemberVm>>("GetGroupMembers", groupMemberVm);
	        }
	        else
	        {
	            return new List<GroupMemberVm>();
	        }
	    }


        private void GetNickname()
        {
            _nickName = CrossSettings.Current.GetValueOrDefault<string>("nickname");
        }

	    private void GetPhoneNumber()
        {
            var phoneNumber = CrossSettings.Current.GetValueOrDefault<string>("phonenumber");
            _phoneNumber = ContactsHelper.CleanPhoneNumber( phoneNumber);
            if (string.IsNullOrWhiteSpace(_phoneNumber))
            {
                AddNotification("Where's Chris PhoneNumber", "Please Add your Phone Number in settings");
            }
        }

        public Task SendError(string message)
	    {
	        _chatHubProxy.Invoke("SendErrorMessage", message, _phoneNumber);
            return Task.CompletedTask;
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

