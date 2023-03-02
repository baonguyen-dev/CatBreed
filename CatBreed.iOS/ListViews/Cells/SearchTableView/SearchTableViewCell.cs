using System;
using CatBreed.ApiClient.Models;
using Foundation;
using UIKit;

namespace CatBreed.iOS.ListViews.Cells.SearchTableView
{
	public partial class SearchTableViewCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString ("SearchTableViewCell");
		public static readonly UINib Nib;

		static SearchTableViewCell ()
		{
			Nib = UINib.FromName ("SearchTableViewCell", NSBundle.MainBundle);
		}

		protected SearchTableViewCell (IntPtr handle) : base (handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public void UpdateData(CatTypeModel item)
		{
			lblName.Text = item.Name;
		}
	}
}
