// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CatBreed.iOS
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		UIKit.UIActivityIndicatorView activityIndicator { get; set; }

		[Outlet]
		UIKit.UISearchBar searchBar { get; set; }

		[Outlet]
		UIKit.UITableView tableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (searchBar != null) {
				searchBar.Dispose ();
				searchBar = null;
			}

			if (tableView != null) {
				tableView.Dispose ();
				tableView = null;
			}

			if (activityIndicator != null) {
				activityIndicator.Dispose ();
				activityIndicator = null;
			}
		}
	}
}
