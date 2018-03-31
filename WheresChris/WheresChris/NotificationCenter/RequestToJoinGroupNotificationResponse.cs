﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
using StayTogether;
using StayTogether.Classes;
using StayTogether.Helpers;
using StayTogether.Models;
using WheresChris.Helpers;
using WheresChris.Views.Popup;
using Xamarin.Forms;

namespace WheresChris.NotificationCenter
{
    public class RequestToJoinGroupNotificationResponse
    {
        public static void HandleRequestToJoinMyGroup(List<GroupMemberSimpleVm> groupMembersSimple)
        {
            var memberInfo = groupMembersSimple.Aggregate("", (current, member) => current + $"\n\r {ContactsHelper.NameOrPhone(member.PhoneNumber, member.Name)}");

            var items = new ObservableCollection < PopupItem >
            {
                new PopupItem($"The following people would like to join your group: \n\r{memberInfo}", () =>
                {
                    try
                    {
                        var userPhoneNumber = SettingsHelper.GetPhoneNumber();//this should be the other leader's phone number

                        //This groupMember is null
                        var groupMembers = groupMembersSimple.Select(member => new GroupMemberVm
                        {
                            Latitude = member.Latitude,
                            Longitude = member.Longitude,
                            Name = member.Name,
                            PhoneNumber = member.PhoneNumber
                        }).ToList();

                        AsyncHelper.RunSync(async () =>
                        {
                            try
                            {
                                await GroupActionsHelper.StartOrAddToGroup(groupMembers, userPhoneNumber);
                            }
                            catch(Exception ex)
                            {
                                Crashes.TrackError(ex, new Dictionary<string, string>
                                {
                                    {"Source", ex.Source },
                                    { "stackTrace",ex.StackTrace}
                                });
                            }
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
                }),
                new PopupItem("Ignore and try to invite them later", null),
            };

            ((App)Xamarin.Forms.Application.Current).ShowPopup(items);
        }
    }
}
