using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Plugin.Geolocator;
using StayTogether.Classes;
using StayTogether.Models;
using WheresChris;
using WheresChris.Helpers;
using Xamarin.Forms;

namespace StayTogether.Helpers
{
    public class ChatHelper
    {
        public static async Task SendChatMessage(string message)
        {
            var userPosition = await CrossGeolocator.Current.GetLastKnownLocationAsync();
            var userPhoneNumber = SettingsHelper.GetPhoneNumber();
            var nickname = SettingsHelper.GetNickname();
            var groupMemberVm = new GroupMemberVm()
            {
                Latitude = userPosition.Latitude,
                Longitude = userPosition.Longitude,
                PhoneNumber = userPhoneNumber,
                Name = nickname
            };
  
            var chatMessageVm = new ChatMessageVm
            {
                GroupMemberVm = groupMemberVm,
                Message = message
            };       
            MessagingCenter.Send<MessagingCenterSender, ChatMessageVm>(new MessagingCenterSender(), LocationSender.SendChatMsg, chatMessageVm);
        }
    }
}
