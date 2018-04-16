using System;
using System.Collections.Generic;
using Authy.Net;
using Microsoft.AppCenter.Crashes;
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

	        if(PermissionHelper.IsAuthyAuthenticated())
	        {
	            Device.BeginInvokeOnMainThread(() =>
	            {
	                Navigation.PopModalAsync();
	            });
	        }
#if (DEBUG)
	        _testMode = true;
#endif
            _authyClient = new AuthyClient("OKkpKcWSzDvBs4Fbfm6nSpp905BFHAOD", _testMode);

	        _authyUser = SettingsHelper.GetAuthyUser();
	        if(!string.IsNullOrWhiteSpace(_authyUser?.UserId))
	        {
	            RegistrationForm.IsVisible = false;
	            EmailAddress.Text = _authyUser.Email;

	            ConfirmationForm.IsVisible = true;
	        }
	        else
	        {
	            PhoneNumber.Text = SettingsHelper.GetPhoneNumber(); 
#if (DEBUG)
	        PhoneNumber.Text = "6199284340";
#endif     
	        }
	    }

	    private void AuthenticatePhoneButton_OnClicked(object sender, EventArgs e)
	    {
	        if(string.IsNullOrWhiteSpace(PhoneNumber.Text)) return;
	        if(string.IsNullOrWhiteSpace(EmailAddress.Text)) return;

            _authyUser = _authyUser ?? new AuthyUser(EmailAddress.Text, PhoneNumber.Text);
	        var authyResult = _authyClient.RegisterUser
                                    (_authyUser.Email, 
                                    _authyUser.PhoneNumber.Replace($"+{_authyUser.CountryCode}", string.Empty), 
                                    Convert.ToInt32(_authyUser.CountryCode)
                                    );

	        _authyUser.UserId = authyResult.UserId;           
            SettingsHelper.SaveAuthyUser(_authyUser);
	        RegistrationForm.IsVisible = false;
	        ConfirmationForm.IsVisible = true;
	    }

	    private void ConfirmCodeButton_OnClicked(object sender, EventArgs e)
	    {
	        try
	        {
	            if(_authyUser == null)
	            {
	                _authyUser = SettingsHelper.GetAuthyUser();
	                if(_authyUser == null) return;
//Leave this here.  Enable for Emulators that can't receive the text message
//#if (DEBUG)
//	                _authyUser.TokenResult.Success = true;
//	                _authyUser.TokenResult.Status = AuthyStatus.Success;
//	                SettingsHelper.SaveAuthyUser(_authyUser);
//	                return;
//#endif
	            }
	            var result = _authyClient.VerifyToken(_authyUser.UserId, AuthyToken.Text);
	            _authyUser.TokenResult = result;
	            AuthyTokenResult = result; 
	            SettingsHelper.SaveAuthyUser(_authyUser);

	            ConfirmationForm.IsVisible = false;
                Device.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopModalAsync(true);
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