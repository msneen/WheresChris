using Newtonsoft.Json;
using Plugin.Settings;
using WheresChris.Models;

namespace WheresChris.Helpers
{
    public class InvitationHelper
    {
        public static void SaveInvitation(Invitation invitation)
        {
            var jsonInvitation = JsonConvert.SerializeObject(invitation);
            CrossSettings.Current.AddOrUpdateValue("lastinvitation", jsonInvitation);
        }

        public static Invitation LoadInvitation()
        {
            var lastInvitationJson = CrossSettings.Current.GetValueOrDefault("lastinvitation", "");
            var lastInvitation = JsonConvert.DeserializeObject<Invitation> (lastInvitationJson);
            return lastInvitation;            
        }
    }
}
