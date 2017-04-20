using System;
using Xamarin.Forms;

namespace WheresChris.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public void StartGroup(object sender, EventArgs e)
        {
            var masterPage = Parent.Parent as TabbedPage;
            if (masterPage != null)
            {
                masterPage.CurrentPage = masterPage.Children[1];
            }
        }
        public void JoinGroup(object sender, EventArgs e)
        {

        }
    }
}
