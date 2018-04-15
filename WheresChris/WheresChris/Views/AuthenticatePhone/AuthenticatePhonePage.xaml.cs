using System;
using Authy.Net;
using WheresChris.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WheresChris.Views.AuthenticatePhone
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	// ReSharper disable once RedundantExtendsListEntry
	public partial class AuthenticatePhonePage : ContentPage
	{
	    private readonly AuthyClient _authyClient;
	    private bool _testMode = false;
	    private AuthyUser _authyUser;
	    public VerifyTokenResult AuthyTokenResult { get; set; }

	    public AuthenticatePhonePage ()
	    {
	        InitializeComponent ();
#if (DEBUG)
	        _testMode = true;
#endif
            _authyClient = new AuthyClient("Todo:get authy apikey", _testMode);

	        PhoneNumber.Text = SettingsHelper.GetPhoneNumber();
	    }

	    private void AuthenticatePhoneButton_OnClicked(object sender, EventArgs e)
	    {
	        if(string.IsNullOrWhiteSpace(PhoneNumber.Text)) return;
	        if(string.IsNullOrWhiteSpace(EmailAddress.Text)) return;

            _authyUser = new AuthyUser(EmailAddress.Text, PhoneNumber.Text);
	        _authyUser.UserId = _authyClient.RegisterUser
                                    (_authyUser.Email, 
                                    _authyUser.PhoneNumber.Replace($"+{_authyUser.CountryCode}", string.Empty), 
                                    Convert.ToInt32(_authyUser.CountryCode)
                                    ).UserId;
            
            SettingsHelper.SaveAuthyUser(_authyUser);
	        RegistrationForm.IsVisible = false;
	        ConfirmationForm.IsVisible = true;
	    }

	    private void ConfirmCodeButton_OnClicked(object sender, EventArgs e)
	    {
	        if(_authyUser == null)
	        {
	            _authyUser = SettingsHelper.GetAuthyUser();
	            if(_authyUser == null) return;
	        }
	        var result = _authyClient.VerifyToken(_authyUser.UserId, AuthyToken.Text);
	        _authyUser.TokenResult = result;
	        AuthyTokenResult = result;
            SettingsHelper.SaveAuthyUser(_authyUser);

	        ConfirmationForm.IsVisible = false;
	    }
	}

    public class AuthyUser
    {
        public AuthyUser()
        {
            
        }

        public AuthyUser(string email, string phoneNumber)
        {
            Email = email;
            PhoneNumber = phoneNumber;
        }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; } = "1";//only usa and canada for now
        public string UserId { get; set; }
        public VerifyTokenResult TokenResult { get; set; }
    }
}