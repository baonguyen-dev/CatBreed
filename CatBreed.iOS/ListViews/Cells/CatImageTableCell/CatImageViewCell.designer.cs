// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CatBreed.iOS.ListViews.Cells.CatImageTableCell
{
	[Register ("CatImageViewCell")]
	partial class CatImageViewCell
	{
		[Outlet]
		UIKit.UIImageView IvImage { get; set; }

		[Outlet]
		UIKit.UILabel TvDownload { get; set; }

		[Outlet]
		UIKit.UILabel TvName { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TvName != null) {
				TvName.Dispose ();
				TvName = null;
			}

			if (TvDownload != null) {
				TvDownload.Dispose ();
				TvDownload = null;
			}

			if (IvImage != null) {
				IvImage.Dispose ();
				IvImage = null;
			}
		}
	}
}
