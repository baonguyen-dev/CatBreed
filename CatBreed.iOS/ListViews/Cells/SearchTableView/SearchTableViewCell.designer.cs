// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CatBreed.iOS.ListViews.Cells.SearchTableView
{
	[Register ("SearchTableViewCell")]
	partial class SearchTableViewCell
	{
		[Outlet]
		UIKit.UILabel lblName { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblName != null) {
				lblName.Dispose ();
				lblName = null;
			}
		}
	}
}
