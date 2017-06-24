using System.Threading.Tasks;
using Plugin.Settings;
using StayTogether;
#if __ANDROID__
using Android.App;
using Android.Content;
using Android.Telephony;
#endif


namespace WheresChris.Helpers
{
    public class SettingsHelper
    {
        public static string GetPhoneNumber()
        {
            var existingNumber = CrossSettings.Current.GetValueOrDefault<string>("phonenumber");
            var cleanExistingPhone = ContactsHelper.CleanPhoneNumber(existingNumber);
            return !string.IsNullOrWhiteSpace(cleanExistingPhone) ? cleanExistingPhone : string.Empty;
        }

        public static async Task<string> GetPhoneNumberFromService()
        {
#if __ANDROID__
            var phonePermission = await PermissionHelper.HasOrRequestPhonePermission();
            if (!phonePermission) return string.Empty;

            var info = (TelephonyManager)Application.Context.GetSystemService(Context.TelephonyService);
            var phoneNumber = info.Line1Number;
            var cleanPhone = ContactsHelper.CleanPhoneNumber(phoneNumber);
            CrossSettings.Current.AddOrUpdateValue("phonenumber", cleanPhone);
            return cleanPhone;
#else
            return string.Empty;
#endif
        }

        public static string SavePhoneNumber(string phoneNumber)
        {
            var cleanPhone = ContactsHelper.CleanPhoneNumber(phoneNumber);
            if (string.IsNullOrWhiteSpace(cleanPhone)) return string.Empty;

            CrossSettings.Current.AddOrUpdateValue("phonenumber", cleanPhone);
            return cleanPhone;
        }

        public static string GetNickname()
        {
            return CrossSettings.Current.GetValueOrDefault<string>("nickname");
        }

        public static string SaveNickname(string nickname)
        {
            
            if (string.IsNullOrWhiteSpace(nickname.Trim())) return "";

            CrossSettings.Current.AddOrUpdateValue("nickname", nickname.Trim());
            return nickname.Trim();
        }
    }
}
