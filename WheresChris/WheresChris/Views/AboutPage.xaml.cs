
using System.Reflection;
using Xamarin.Forms;

namespace WheresChris.Views
{
	public partial class AboutPage : ContentPage
	{
		public AboutPage()
		{
			InitializeComponent();
		    Title = "About Where's Chris";
		    VersionSpan.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}
	}
}
