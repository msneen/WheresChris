using WheresChris.Helpers;
using WheresChris.Models;
using WheresChris.Services;
using WheresChris.Views;
using Xamarin.Forms;

namespace WheresChris.ViewModels
{
	public class BaseViewModel : ObservableObject
	{
		/// <summary>
		/// Get the azure service instance
		/// </summary>
		public IDataStore<ContactDisplayItemVm> DataStore => DependencyService.Get<IDataStore<ContactDisplayItemVm>>();

		bool isBusy = false;
		public bool IsBusy
		{
			get { return isBusy; }
			set { SetProperty(ref isBusy, value); }
		}
		/// <summary>
		/// Private backing field to hold the title
		/// </summary>
		string title = string.Empty;
		/// <summary>
		/// Public property to set and get the title of the item
		/// </summary>
		public string Title
		{
			get { return title; }
			set { SetProperty(ref title, value); }
		}

	    string name = string.Empty;

	    public string Name
	    {
            get { return name; }
            set { SetProperty(ref name, value); }
	    }
	}
}

