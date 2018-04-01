using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Settings;
using StayTogether.Classes;
using StayTogether.Helpers;
using StayTogether.Models;
using WheresChris.Helpers;
using WheresChris.ViewModels;
using Xamarin.Forms;
using ChatMessageVm = StayTogether.Models.ChatMessageVm;
using Position = Xamarin.Forms.Maps.Position;

namespace StayTogether
{
    public delegate void EventHandler<in TLostEventArgs>(object sender, TLostEventArgs e);
    public delegate void EventHandler(object sender, EventArgs e);


	public class LocationSender
	{
	    private static LocationSender _instance;
	    private static readonly Interval RestartTimer = new Interval();

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
	    public const string PhoneNumberMissingMsg = "PHONENUMBERMISSING";
	    public const string ChatReceivedMsg = "CHATRECEIVED";
	    public const string StartOrAddGroupMsg = "STARTORADDGROUP";
	    public const string LeaveGroupMsg = "LEAVEGROUP";
	    public const string EndGroupMsg = "ENDGROUP";
	    public const string SendChatMsg = "SENDCHAT";
	    public const string GroupInvitationsMsg = "GROUPINVITATIONSMSG";
	    public const string GetInvitationsMsg = "GETINVITATIONS";             
        public const string ConfirmGroupInvitationMsg = "CONFIRMGROUPiNVITATION";
	    public const string PositionUpdatedMsg = "POSITIONUPDATED";
	    public const string RequestAdditionalMembersJoinGroup = "REQUESTADDITIONALMEMBERSJOINGROUP";//THIS ORIGINATES THE CALL
	    public const string AdditionalMembersRequestJoinGroup = "ADDITIONALMEMBERSREQUESTJOINGROUP";//This is the signal r call to the group leader when someone requests to add members
	    public const string SendTelemetryMsg = "SENDTELEMETRYMESSAGE";
	    public const string LeaveOrEndGroupMsg = "LEAVEORENDGROUPMESSAGE";

        public bool InAGroup { get; set; }
        public bool GroupLeader { get; set; }
        public bool IsInitialized { get; set; }
        public bool HasBeenConnected { get; set; }
        public ConnectionState ConnectionState { get; set; }
        public List<GroupMemberSimpleVm> GroupMembers { get; set; }
	    public List<GroupMemberVm> InvitedGroupMembers { get; set; } = new List<GroupMemberVm>();

        private HubConnection _hubConnection;
	    private IHubProxy _chatHubProxy;
	    private IGeolocator _geoLocator;
	    private string _phoneNumber;
        private string _groupId = ""; //Creator of group's phone number

	    public static Task<bool> IsGeolocationAvailable()
	    {
	        var geoLocator = CrossGeolocator.Current;
	        var isEnabled = geoLocator.IsGeolocationEnabled && geoLocator.IsGeolocationAvailable;
	        if(isEnabled) return Task.FromResult(true);
	        try
	        {
	            var testLocation =
	                AsyncHelper.RunSync<Plugin.Geolocator.Abstractions.Position>(() => geoLocator.GetPositionAsync());
	            return Task.FromResult(testLocation.LocationValid());
	        }
	        catch(Exception ex)
	        {
	            Crashes.TrackError(ex, new Dictionary<string, string>
	            {
	                {"Source", ex.Source },
	                { "stackTrace",ex.StackTrace}
	            });
	            return Task.FromResult(false);
	        }
	    }
	    public static async Task<LocationSender> GetInstanceAsync()
	    {
	        if(!await IsGeolocationAvailable()) return null;

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
	        if(!IsGeolocationAvailable().Result) return null;
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

        

        public LocationSender ()
	    {
            try
            {
                GetNickname();
                GetPhoneNumber();
                InitializeMessageCenter();
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
	        if (!IsInitialized && await IsGeolocationAvailable())
	        {
	            await InitializeSignalRAsync();
	        }
	    }

	    public void Initialize()
	    {
	        if(!IsGeolocationAvailable().Result) return;
	        InitializeAsync().Wait();
	    }

	    private void InitializeMessageCenter()
	    {
	        MessagingCenter.Subscribe<MessagingCenterSender, GroupMemberSimpleVm>(this, ConfirmGroupInvitationMsg, async (sender, groupMemberSimpleVm) =>
	        {
                await ConfirmGroupInvitation(groupMemberSimpleVm.PhoneNumber, groupMemberSimpleVm.Name);                  
            });
            MessagingCenter.Subscribe<MessagingCenterSender, GroupVm>(this, StartOrAddGroupMsg, async (sender, groupVm) =>
            {
                await StartOrAddToGroup(groupVm);
            });

            MessagingCenter.Subscribe<MessagingCenterSender>(this, LeaveGroupMsg, async (sender) =>
            {
                await LeaveGroup();
            });
            MessagingCenter.Subscribe<MessagingCenterSender>(this, EndGroupMsg, async (sender) =>
            {
                await EndGroup();
            });
            MessagingCenter.Subscribe<MessagingCenterSender, ChatMessageVm>(this, SendChatMsg, async (sender, chatMessageVm) =>
            {
                await SendChatMessage(chatMessageVm.GroupMemberVm, chatMessageVm.Message);
            });
            MessagingCenter.Subscribe<MessagingCenterSender>(this, GetInvitationsMsg, (sender) =>
            {
                GetInvitations();
            });
            MessagingCenter.Subscribe<MessagingCenterSender, Plugin.Geolocator.Abstractions.Position>(this, PositionUpdatedMsg, async (sender, position) =>
            {
                var mapPosition = PositionHelper.GetMapPosition(position);
                await SendUpdatePosition(mapPosition);
            });

	        MessagingCenter.Subscribe<MessagingCenterSender, AdditionalMemberInvitationVm>(this, RequestAdditionalMembersJoinGroup, async (sender, additionalMemberInvitationVm) =>
	        {
	            if( (additionalMemberInvitationVm.Group?.GroupMembers?.Count ?? 0) < 1 || InvitedGroupMembers != null || InvitedGroupMembers.Count < 2)
	            {
	                if(additionalMemberInvitationVm.Group == null)
	                {
	                    additionalMemberInvitationVm.Group = new GroupVm();
	                }

	                //remove the group leader
	                var otherGroupLeader = InvitedGroupMembers.FirstOrDefault(l =>
	                    l.PhoneNumber == additionalMemberInvitationVm.GroupLeaderPhoneNumber);
	                if(otherGroupLeader != null)
	                {
	                    InvitedGroupMembers.Remove(otherGroupLeader);
	                }

	                additionalMemberInvitationVm.Group.GroupMembers = InvitedGroupMembers; 
	            }
	            await RequestAdditionalMembersAddedToGroup(additionalMemberInvitationVm);
	        });

	        MessagingCenter.Subscribe<MessagingCenterSender, string>(this, SendTelemetryMsg, async (sender, message) =>
	        {
	            await SendTelemetry(message);
	        });

	        MessagingCenter.Subscribe<MessagingCenterSender>(this, LeaveOrEndGroupMsg, async (sender) =>
	        {
	            await LeaveOrEndGroup();
	        });
        }

	    public async Task InitializeSignalRAsync()
        {
            try
            {
                if(! await IsGeolocationAvailable()) return;
                // Connect to the server
                _hubConnection = new HubConnection("https://staytogetherserver.azurewebsites.net/"); //mike
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
                _chatHubProxy.On<List<GroupMemberSimpleVm>>("JoinGroupRequest", async groupMembers => await OnRequestJoinGroup(groupMembers));//

                _chatHubProxy.On<GroupMemberVm, string>("GroupMessage", ChatMessageReceived);

                _hubConnection.Closed += () =>
                {
                    
                    //restart the connection in 5 seconds.
                    RestartTimer.SetInterval(StartConnection, 5000);
                };
                _hubConnection.Reconnected += () =>
                {
                    
                };
                _hubConnection.StateChanged += change =>
                {
                    ConnectionState = change.NewState;
                    if(change.NewState == ConnectionState.Connected)
                    {
                        HasBeenConnected = true;
                    }
                };
                               
                await StartConnectionAsync();

                await SetUpLocationEvents();

                IsInitialized = true;
            }
            catch(Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_InitializeSignalRAsync", new Dictionary<string, string>
                {
                    {"Message", ex.Message}
                });
            }
            finally
            {
                Console.WriteLine("LocationSender Loaded");
            }
        }

	    private void StartConnection()
	    {
	        AsyncHelper.RunSync(StartConnectionAsync);
	    }

	    private async Task StartConnectionAsync()
	    {
// Start the connection
	        await _hubConnection.Start();
	    }

	    private async Task RequestAdditionalMembersAddedToGroup(AdditionalMemberInvitationVm additionalMemberInvitationVm)
	    {
	        await EndGroup()
                .ContinueWith((async (task) =>
	            {
	                //Request to add these members to another group, and end the group we are in
	                await InvokeChatHubProxy("requestJoinGroup", additionalMemberInvitationVm.Group, additionalMemberInvitationVm.GroupLeaderPhoneNumber);
	            }));


	    }

	    private Task OnRequestJoinGroup(List<GroupMemberSimpleVm> groupMembers)
	    {
	        try
	        {
	            MessagingCenter.Send<LocationSender, List<GroupMemberSimpleVm>>(this, AdditionalMembersRequestJoinGroup, groupMembers);
	            return Task.CompletedTask;
	        }
	        catch (Exception ex)
	        {
	            Analytics.TrackEvent($"LocationSender_OnRequestJoinGroup", new Dictionary<string, string>
	            {
	                { "Message", ex.Message}
	            });
	            return Task.FromException(ex);
	        }
	    }

	    private void ChatMessageReceived(GroupMemberVm groupMember, string message)
	    {
            var chatMessageVm = new ChatMessageSimpleVm
            {
                Message = message,
                Member = groupMember
            };
            MessagingCenter.Send<LocationSender, ChatMessageSimpleVm>(this, ChatReceivedMsg, chatMessageVm);
        }

	    private async Task InvokeChatHubProxy(string method, params object[] args)
	    {
	        var startTime = DateTime.Now;
	        while(DateTime.Now.Subtract(startTime) < TimeSpan.FromSeconds(20))
	        {
	            if(CanSend())
	            {
	                await _chatHubProxy.Invoke(method, args);
	                return;
	            }
	            await Task.Delay(1000);
	        }
	    }

        public async Task<T> InvokeChatHubProxy<T>(string method, params object[] args) where T:new()
        {
            var startTime = DateTime.Now;
            while(DateTime.Now.Subtract(startTime) < TimeSpan.FromSeconds(20))
            {
                if(CanSend())
                {
                    return await _chatHubProxy.Invoke<T>(method, args);
                }
                await Task.Delay(1000);
            }
            
            return new T();
        }

        private bool CanSend()
        {
            if(!HasBeenConnected && !this.IsInitialized)
            {
                this.Initialize();
            }
            if(ConnectionState != ConnectionState.Connected) return false;

	        if (_phoneNumber.IsValidPhoneNumber() ) return true;

            if (string.IsNullOrWhiteSpace(_phoneNumber))
            {
                GetPhoneNumber();
                if(_phoneNumber.IsValidPhoneNumber())
                {
                    return true;
                }
                else
                {
                    MessagingCenter.Send<LocationSender>(this, PhoneNumberMissingMsg);
                    return false;
                }
            }            
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

        private bool _geolocatorInitialized = false;
	    public async Task SetUpLocationEvents()
        {

            try
            {
                if (_geolocatorInitialized) return;
                _geoLocator = CrossGeolocator.Current;

                _geoLocator.DesiredAccuracy = 30; //100 is new default

                if (!_geoLocator.IsGeolocationEnabled || !_geoLocator.IsGeolocationAvailable) return;

                //Instructions for geoolocator say we need to do this before setting the listener
                var position = await _geoLocator.GetPositionAsync(new TimeSpan(0,0,10));
                if(position != null)
                {
                    MessagingCenter.Send(new MessagingCenterSender(), PositionUpdatedMsg, position);
                }

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

                _geoLocator.PositionChanged += delegate (object o, PositionEventArgs args)
                {
                    //await LocatorOnPositionChanged(o, args);
                    MessagingCenter.Send(new MessagingCenterSender(), PositionUpdatedMsg, args.Position);
                };

                _geolocatorInitialized = true;
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

	    public async Task SendUpdatePosition()
	    {
            try
            {
                var currentPosition = await PositionHelper.GetMapPosition();
                if (await SendUpdatePosition(currentPosition)) return;
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

	    public async Task<bool> SendUpdatePosition(Position? currentPosition)
	    {
	        try
	        {
	            if (!currentPosition.HasValue)
	            {
	                Analytics.TrackEvent("LocationSender_LocatorOnPositionChanged_PositionNull");
	                await SendTelemetry($"LocationSender_LocatorOnPositionChanged_PositionNull");
	                return false;
	            }
	            if (!PositionHelper.LocationValid(currentPosition.Value))
	            {
	                Analytics.TrackEvent("LocationSender_LocatorOnPositionChanged_PositionInvalid");
	                await SendTelemetry($"LocationSender_LocatorOnPositionChanged_PositionInvalid");
	                return false;
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
                return true;
            }
	        catch (Exception ex)
	        {
#if (DEBUG)
	            Debugger.Break();
#endif
	            Analytics.TrackEvent($"LocationSender_SendUpdatePosition", new Dictionary<string, string>
	            {
	                {"Message", ex.Message}
	            });
                return false;
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
	            MessagingCenter.Send<LocationSender, List<GroupMemberSimpleVm>>(this, GroupPositionUpdateMsg, groupMembers);
	        }
            catch (Exception ex)
            {
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
                var groupMemberSimpleVm = new GroupMemberSimpleVm
                {
                    Name = memberName,
                    PhoneNumber = memberPhoneNumber
                };

                MessagingCenter.Send<LocationSender, GroupMemberSimpleVm>(this, MemberAlreadyInGroupMsg, groupMemberSimpleVm);
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
                SendTelemetry($"Cannot process OnMemberAlreadyInGroup.  Error: {ex.Message}").ConfigureAwait(false);
            }
        }

	    private void OnMemberLeftGroup(string memberPhoneNumber, string memberName)
	    {
            try
            {
                if (memberPhoneNumber == _phoneNumber) return; //I dont need to notify myself that I left
                var groupMemberSimpleVm = new GroupMemberSimpleVm
                {
                    Name = memberName,
                    PhoneNumber = memberPhoneNumber
                };
                MessagingCenter.Send<LocationSender, GroupMemberSimpleVm>(this, SomeoneLeftMsg, groupMemberSimpleVm);
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
                SendTelemetry($"Cannot process onMemberLeftGroup.  Error: {ex.Message}").ConfigureAwait(false);
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
                SendTelemetry($"Cannot process GroupDisbanded.  Error: {ex.Message}").ConfigureAwait(false);
            }
        }

        private readonly InvitationList _invitationList = new InvitationList();
	    private string _nickName;

	    private void OnGroupInvitation(string phoneNumber, string name)
        {
            try
            {
                if (phoneNumber.CleanPhoneNumber() == _phoneNumber.CleanPhoneNumber()) return;//don't invite myself to a group

                var invitationVm = new InvitationVm
                {
                    Name = name,
                    PhoneNumber = phoneNumber,
                    ReceivedTime = DateTime.Now
                };

                _invitationList.AddInvitation(invitationVm);
               
                MessagingCenter.Send<LocationSender, InvitationVm>(this, GroupInvitationReceivedMsg, invitationVm);
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
                SendTelemetry($"Cannot process OnGroupInvitation.  Error: {ex.Message}").ConfigureAwait(false);
            }
        }

	    public List<InvitationVm> GetInvitations(int hours = 3)
	    {
            try
            {
                _invitationList.Clean();
                MessagingCenter.Send<LocationSender, InvitationList>(this, GroupInvitationsMsg, _invitationList);
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
                SendTelemetry($"Cannot Get Invitations.  Error: {ex.Message}").ConfigureAwait(false);
            }
	        return null;
	    }

        public void UpdateGroupId(string id)
	    {
            try
            {
                //Might want to send Group Joined message here
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
                SendTelemetry($"Cannot update GroupId.  Error: {ex.Message}").ConfigureAwait(false);
            }
        }

	    public void SomeoneIsLost(LostMemberVm lostMemberVm)//string phoneNumber, string latitude, string longitude, string name, double distance
	    {

            try
            {
                if (!PositionHelper.LocationValid(lostMemberVm)) return;
                if (lostMemberVm.LostDistance > 5280 * 60) return;
                if (string.IsNullOrWhiteSpace(_groupId)) return;

                var groupMember = new GroupMemberVm
                {
                    PhoneNumber = lostMemberVm.PhoneNumber,
                    Name = lostMemberVm.Name,
                    Latitude = Convert.ToDouble(lostMemberVm.Latitude),
                    Longitude = Convert.ToDouble(lostMemberVm.Longitude),
                    LostDistance = lostMemberVm.LostDistance
                };
                MessagingCenter.Send<LocationSender, GroupMemberVm>(this, SomeoneIsLostMsg, groupMember);
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
                SendTelemetry($"Cannot process someoneIsLost.  Error: {ex.Message}").ConfigureAwait(false);
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
                InvitedGroupMembers = groupVm.GroupMembers;
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
                await SendTelemetry($"Cannot Start or Add to Group.  Error: {ex.Message}");
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
                await SendTelemetry($"Cannot Start Group.  Error: {ex.Message}");
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
                await SendTelemetry($"Cannot Add To Group.  Error: {ex.Message}");
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
                else
                {
                    await SendTelemetry($"Cannot End Group.  InAGroup={InAGroup} GroupLeader={GroupLeader}");
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
                await SendTelemetry($"Cannot End Group.  Error: {ex.Message}");
            }
        }

	    public async Task LeaveOrEndGroup()
	    {	        
	        try
	        {
	            await InvokeChatHubProxy("LeaveOrEndGroup", _phoneNumber);
	        }
	        catch(Exception ex)
	        {
	            Analytics.TrackEvent($"LocationSender_LeaveOrEndGroup", new Dictionary<string, string>
	            {
	                { "Message", ex.Message}
	            });
	            await SendTelemetry($"LeaveOrEndGroup: Error: {ex.Message}" );
	        }
	    }

        public async Task LeaveGroup()
        {
            try
            {
                if(InAGroup && !GroupLeader)
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
                await SendTelemetry($"Cannot Leave Group.  Error: {ex.Message}");
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
                if(InAGroup)
                {
                    await SendTelemetry($"Cannot Confirm Group Invitation.  Already in a group");
                    return false; //Don't allow them to join a group
                }            

                UpdateGroupId(phoneNumber);

                var currentPosition = await PositionHelper.GetMapPosition();
                if (!currentPosition.HasValue)
                {
                    Analytics.TrackEvent("LocationSender_ConfirmGroupInvitation_PositionNull");
                    await SendTelemetry($"Cannot Confirm Group Invitation.  PositionNull");
                    return false;
                }

                if (!currentPosition.Value.LocationValid())
                {
                    Analytics.TrackEvent("LocationSender_ConfirmGroupInvitation_PositionInvalid");
                    await SendTelemetry($"Cannot Confirm Group Invitation.  PositionInvalid");
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
                //await SendUpdatePosition(); //I think this happens in confirmGroupInvitation//Send out my current position so group member's maps will update
                MessagingCenter.Send<LocationSender>(this, GroupJoinedMsg);
                await SendTelemetry($"Confirmed Group Invitation");
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

	    public async Task SendChatMessage(GroupMemberVm groupMemberVm, string message)
	    {
            try
            {
                if (InAGroup)
                {
                    groupMemberVm.GroupId = _groupId;
                    await InvokeChatHubProxy("SendToGroup", groupMemberVm, message);
                }
                else
                {
                    var chatMessageVm = new ChatMessageSimpleVm
                    {
                        Message = "Message not sent.  Not currently in a group",
                        Member = new GroupMemberVm { Name = "System"}
                    };
                    MessagingCenter.Send<LocationSender, ChatMessageSimpleVm>(this, ChatReceivedMsg, chatMessageVm);
                }
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Debugger.Break();
#endif
                Analytics.TrackEvent($"LocationSender_SendToGroup", new Dictionary<string, string>
                {
                    { "Message", ex.Message}
                });
            }
        }

	    public async Task SendTelemetry(string message)
	    {
	        await InvokeChatHubProxy("sendTelemetry", message, _phoneNumber);
	    }

        private void GetNickname()
        {
            try
            {
                _nickName = CrossSettings.Current.GetValueOrDefault("nickname", "");
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

    public class MemberEventArgs : EventArgs
    {
        public GroupMemberVm GroupMember { get; set; }
    }

    public class LostEventArgs : MemberEventArgs
    {
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

    public class ChatMessageEventArgs : MemberEventArgs
    {
        public string Message { get; set; }
    }
}

