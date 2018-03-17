using System;
using System.Collections.Generic;
using StayTogether;
using StayTogether.Classes;
using StayTogether.Droid.NotificationCenter;
using StayTogether.Models;
using Xamarin.Forms;

namespace WheresChris.Droid
{
    public class MessageCenterListener
    {
        private static MessageCenterListener _instance;
        public static MessageCenterListener Instance
        {
            get
            {
                Initialize();
                return _instance;
            }

        }

        public MessageCenterListener()
        {

        }

        public static void Initialize()
        {
            if(_instance != null) return;

            _instance = new MessageCenterListener();
            _instance.InitializeMessagingCenterSubscriptions();
        }

        public void InitializeMessagingCenterSubscriptions()
        {
            MessagingCenter.Subscribe<LocationSender, GroupMemberVm>(this, LocationSender.SomeoneIsLostMsg,
                (sender, groupMember) =>
                {
                    Device.BeginInvokeOnMainThread(() => { LostNotification.DisplayLostNotification(groupMember); });
                });

            MessagingCenter.Subscribe<LocationSender, InvitationVm>(this, LocationSender.GroupInvitationReceivedMsg,
                (sender, invitationVm) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        GroupInvitationNotification.DisplayGroupInvitationNotification(invitationVm.PhoneNumber,
                            invitationVm.Name);
                    });
                });

            MessagingCenter.Subscribe<LocationSender, GroupMemberSimpleVm>(this, LocationSender.SomeoneLeftMsg,
                (sender, groupMemberSimpleVm) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        LeftGroupNotification.DisplayLostNotification(groupMemberSimpleVm.PhoneNumber,
                            groupMemberSimpleVm.Name);
                    });
                });

            MessagingCenter.Subscribe<LocationSender, GroupMemberSimpleVm>(this, LocationSender.MemberAlreadyInGroupMsg,
                (sender, groupMemberSimpleVm) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        InAnotherGroupNotification.DisplayInAnotherGroupNotification(groupMemberSimpleVm.PhoneNumber,
                            groupMemberSimpleVm.Name);
                    });
                });

            MessagingCenter.Subscribe<LocationSender, List<GroupMemberSimpleVm>>(this, LocationSender.AdditionalMembersRequestJoinGroup,
                (sender, groupMemberSimpleListVm) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        RequestToJoinGroupNotification.RequestToJoinThisGroup(groupMemberSimpleListVm);
                    });
                });

            Console.WriteLine("MessageCenterListener Loaded");
        }
    }
}