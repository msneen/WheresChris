using System;
using System.Collections.Generic;
using System.Reflection;
using StayTogether;
using StayTogether.Classes;
using StayTogether.Helpers;
using StayTogether.Models;
using WheresChris.Helpers;
using WheresChris.ViewModels;
using Xamarin.Forms;

namespace WheresChris.Views
{
	public partial class AboutPage : ContentPage
	{
	    private int _tapCount = 0;
		public AboutPage()
		{
			InitializeComponent();
		    Title = "Where's Chris - About";            
		    DisplayVersionNumber();
                       
		    VersionGestureRecognizer.Tapped += (sender, eventArgs) =>
		    {
		        _tapCount ++;
		        if (_tapCount <= 5) return;

		        LastOutboundMessage.IsVisible = true;
		        LastInboundMessage.IsVisible = true;
		        BtnReset.IsVisible = true;
		        InitializeMessagingCenter();
		    };
		}

	    private void InitializeMessagingCenter()
	    {
	        MessagingCenter.Subscribe<LocationSender>(this, LocationSender.LocationSentMsg,
	        (sender) =>
	        {
	            Device.BeginInvokeOnMainThread(
	                () => { LastOutboundMessage.Text = $"Location Sent at {DateTime.Now.ToLongTimeString()}"; });
	        });

            MessagingCenter.Subscribe<LocationSender, ChatMessageSimpleVm>(this, LocationSender.ChatReceivedMsg,
            (sender, chatMessageVm) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    LastInboundMessage.Text = $"Message Received at {DateTime.Now.ToLongTimeString()}";
                });
            });
            MessagingCenter.Subscribe<LocationSender, GroupMemberVm>(this, LocationSender.SomeoneIsLostMsg,
            (sender, groupMember) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    LastInboundMessage.Text = $"Message Received at {DateTime.Now.ToLongTimeString()}";
                });
            });

            MessagingCenter.Subscribe<LocationSender, InvitationVm>(this, LocationSender.GroupInvitationReceivedMsg,
            (sender, invitationVm) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    LastInboundMessage.Text = $"Message Received at {DateTime.Now.ToLongTimeString()}";
                });
            });
            MessagingCenter.Subscribe<LocationSender, List<GroupMemberSimpleVm>>(this, LocationSender.GroupPositionUpdateMsg, (sender, groupMemberSimpleVm) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    LastInboundMessage.Text = $"Message Received at {DateTime.Now.ToLongTimeString()}";
                });
            });
        }

	    private void DisplayVersionNumber()
	    {
	        //var permissions = PermissionHelper.GetNecessaryPermissionInformation().Result;
	        var version = Assembly.GetExecutingAssembly().GetName().Version;
	        var versionNumber = $"{version.Major}.{version.Minor}.{version.Build}";
	        VersionSpan.Text = versionNumber;
	    }

	    protected override void OnAppearing()
	    {
	        
	    }


	    private void BtnReset_OnClicked(object sender, EventArgs e)
	    {
	        SettingsHelper.ResetData();
	    }
	}
}
