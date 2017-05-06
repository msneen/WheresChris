
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
		    var permissions = PermissionHelper.GetNecessaryPermissionInformation().Result;
		    VersionSpan.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString() + "    " + permissions;
		}
	}
}
