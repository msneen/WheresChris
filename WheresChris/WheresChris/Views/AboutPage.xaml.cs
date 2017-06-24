using System.Reflection;
using StayTogether.Helpers;
using Xamarin.Forms;

namespace WheresChris.Views
{
	public partial class AboutPage : ContentPage
	{
		public AboutPage()
		{
			InitializeComponent();
		    Title = "Where's Chris - About";            
		    DisplayVersionNumber();
		    PositionHelper.OnAccuracyChanged += (sender, args) =>
		    {
                GetAccuracy();
		    };
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
	        GetAccuracy();
	    }

	    private void GetAccuracy()
	    {
	        var locationAccuracy =
	            $"\n\r\n\rmin={System.Math.Round(PositionHelper.MinAccuracy)}\n\rmax={System.Math.Round(PositionHelper.MaxAccuracy)}\n\ravg={System.Math.Round(PositionHelper.AvgAccuracy)}";

	        LocationSpan.Text = locationAccuracy;
	    }
	}
}
