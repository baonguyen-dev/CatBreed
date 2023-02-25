
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace CatBreed.Droid.Activities
{
	[Activity (Label = "BaseActivity")]			
	public class BaseActivity : AppCompatActivity
	{
		private ProgressBar _pbWaiting;
        private RelativeLayout _rltProgressBarLayout;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

            // Create your application here
        }
	}
}

