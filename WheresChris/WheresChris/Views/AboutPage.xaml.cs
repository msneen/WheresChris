
using System.Reflection;
using WheresChris.Helpers;
using Xamarin.Forms;

namespace WheresChris.Views
{
	public partial class AboutPage : ContentPage
	{
		public AboutPage()
		{
			InitializeComponent();
		    Title = "About Where's Chris";
		    DisplayVersionNumber();
		}

	    private void DisplayVersionNumber()
	    {
	        var permissions = PermissionHelper.GetNecessaryPermissionInformation().Result;
	        var version = Assembly.GetExecutingAssembly().GetName().Version;
	        var versionNumber = $"{version.Major}.{version.Minor}.{version.Build}";
	        VersionSpan.Text = versionNumber + permissions;
	    }
	}
}
