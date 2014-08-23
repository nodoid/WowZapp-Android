using System;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace wowZapp.Animations
{		
	public partial class CreateAnimationActivity : Activity
	{
		private LinearLayout createTransition ()
		{
			LinearLayout drawerView = new LinearLayout (context);
			drawerView.Orientation = Orientation.Horizontal;
			drawerView.LayoutParameters = new ViewGroup.LayoutParams (LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);

			ImageView fadeIn = new ImageView (context);
			LinearLayout.LayoutParams viewPars;
			viewPars = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (20f, context), (int)ImageHelper.convertDpToPixel (20f, context));
			viewPars.SetMargins (0, 0, (int)ImageHelper.convertDpToPixel (5f, context), 0);
			fadeIn.LayoutParameters = viewPars;
			fadeIn.Tag = 1;
			fadeIn.Click += new EventHandler (transition_Selected);
			fadeIn.SetImageResource (Resource.Drawable.fadingin);

			ImageView fadeOut = new ImageView (context);
			fadeOut.LayoutParameters = viewPars;
			fadeOut.Tag = 2;
			fadeOut.Click += new EventHandler (transition_Selected);
			fadeOut.SetImageResource (Resource.Drawable.fadingout);

			ImageView rotate = new ImageView (context);
			rotate.LayoutParameters = viewPars;
			rotate.Tag = 3;
			rotate.Click += new EventHandler (transition_Selected);
			rotate.SetImageResource (Resource.Drawable.wheel);

			ImageView move = new ImageView (context);
			move.LayoutParameters = viewPars;
			move.Tag = 4;
			move.Click += new EventHandler (transition_Selected);
			move.SetImageResource (Resource.Drawable.barndoors);

			drawerView.AddView (fadeIn);
			drawerView.AddView (fadeOut);
			drawerView.AddView (rotate);
			drawerView.AddView (move);

			return drawerView;
		}
		
		private LinearLayout createConfigPicker ()
		{
			LinearLayout holder = new LinearLayout (context);
			holder.Orientation = Orientation.Horizontal;
			holder.LayoutParameters = new ViewGroup.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent);
			holder.SetBackgroundResource (Resource.Drawable.footerback);
			
			LinearLayout level2 = new LinearLayout (context);
			level2.Orientation = Orientation.Vertical;
			level2.LayoutParameters = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (260f, context), LinearLayout.LayoutParams.FillParent);
			
			LinearLayout topSeekHolder = new LinearLayout (context);
			topSeekHolder.Orientation = Orientation.Horizontal;
			LinearLayout.LayoutParams viewParams;
			viewParams = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);
			viewParams.SetMargins ((int)ImageHelper.convertDpToPixel (25f, context), (int)ImageHelper.convertDpToPixel (10f, context), 0, 0);
			topSeekHolder.LayoutParameters = viewParams;
			
			SeekBar topSeek = new SeekBar (context);
			viewParams = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.WrapContent, (int)ImageHelper.convertDpToPixel (10f, context));
			viewParams.SetMargins ((int)ImageHelper.convertDpToPixel (15f, context), 0, (int)ImageHelper.convertDpToPixel (15f, context), 0);
			topSeek.LayoutParameters = viewParams;
			topSeekHolder.AddView (topSeek);
			
			LinearLayout set1 = new LinearLayout (context);
			set1.Orientation = Orientation.Horizontal;
			set1.LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel (25f, context));
			TextView set1Text = new TextView (context);
			set1Text.SetTextColor (Color.Black);
			set1Text.LayoutParameters = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (20f, context), LinearLayout.LayoutParams.FillParent);
			set1Text.Gravity = GravityFlags.CenterHorizontal;
			set1Text.SetTextSize (Android.Util.ComplexUnitType.Dip, 20f);
			set1Text.Text = "1";
			HorizontalScrollView set1HSV = new HorizontalScrollView (context);
			viewParams = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent);
			viewParams.SetMargins ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
			set1HSV.LayoutParameters = viewParams;
			set1.AddView (set1Text);
			set1.AddView (set1HSV);
			
			LinearLayout gap1 = new LinearLayout (context);
			gap1.Orientation = Orientation.Vertical;
			gap1.SetBackgroundColor (Color.Brown);
			viewParams = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel (10f, context));
			viewParams.SetMargins ((int)ImageHelper.convertDpToPixel (25f, context), 0, 0, 0);
			gap1.LayoutParameters = viewParams;
			set1.AddView (gap1);
			
			LinearLayout set2 = new LinearLayout (context);
			set2.Orientation = Orientation.Horizontal;
			set2.LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel (25f, context));
			TextView set2Text = new TextView (context);
			set2Text.SetTextColor (Color.Black);
			set2Text.LayoutParameters = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (20f, context), LinearLayout.LayoutParams.FillParent);
			set2Text.Gravity = GravityFlags.CenterHorizontal;
			set2Text.SetTextSize (Android.Util.ComplexUnitType.Dip, 20f);
			set2Text.Text = "2";
			HorizontalScrollView set2HSV = new HorizontalScrollView (context);
			viewParams = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent);
			viewParams.SetMargins ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
			set2HSV.LayoutParameters = viewParams;
			set2.AddView (set2Text);
			set2.AddView (set2HSV);
			
			LinearLayout gap2 = new LinearLayout (context);
			gap2.Orientation = Orientation.Vertical;
			viewParams = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel (10f, context));
			viewParams.SetMargins ((int)ImageHelper.convertDpToPixel (25f, context), 0, 0, 0);
			gap2.LayoutParameters = viewParams;
			gap2.SetBackgroundColor (Color.Brown);
			set2.AddView (gap2);
			
			LinearLayout set3 = new LinearLayout (context);
			set3.Orientation = Orientation.Horizontal;
			set3.LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel (25f, context));
			TextView set3Text = new TextView (context);
			set3Text.SetTextColor (Color.Black);
			set3Text.LayoutParameters = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (20f, context), LinearLayout.LayoutParams.FillParent);
			set3Text.Gravity = GravityFlags.CenterHorizontal;
			set3Text.SetTextSize (Android.Util.ComplexUnitType.Dip, 20f);
			set3Text.Text = "3";
			HorizontalScrollView set3HSV = new HorizontalScrollView (context);
			viewParams = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent);
			viewParams.SetMargins ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
			set3HSV.LayoutParameters = viewParams;
			set3.AddView (set3Text);
			set3.AddView (set3HSV);
			
			LinearLayout gap3 = new LinearLayout (context);
			gap3.Orientation = Orientation.Vertical;
			viewParams = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel (10f, context));
			viewParams.SetMargins ((int)ImageHelper.convertDpToPixel (25f, context), 0, 0, 0);
			gap3.LayoutParameters = viewParams;
			gap3.SetBackgroundColor (Color.Brown);
			set3.AddView (gap3);
			
			LinearLayout set4 = new LinearLayout (context);
			set4.Orientation = Orientation.Horizontal;
			set4.LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, (int)ImageHelper.convertDpToPixel (25f, context));
			TextView set4Text = new TextView (context);
			set4Text.SetTextColor (Color.Black);
			set4Text.LayoutParameters = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (20f, context), LinearLayout.LayoutParams.FillParent);
			set4Text.Gravity = GravityFlags.CenterHorizontal;
			set4Text.SetTextSize (Android.Util.ComplexUnitType.Dip, 20f);
			set4Text.Text = "4";
			HorizontalScrollView set4HSV = new HorizontalScrollView (context);
			viewParams = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent);
			viewParams.SetMargins ((int)ImageHelper.convertDpToPixel (10f, context), 0, (int)ImageHelper.convertDpToPixel (10f, context), 0);
			set4HSV.LayoutParameters = viewParams;
			set4.AddView (set4Text);
			set4.AddView (set4HSV);
			
			level2.AddView (topSeekHolder);
			level2.AddView (set1);
			level2.AddView (set2);
			level2.AddView (set3);
			level2.AddView (set4);
			
			LinearLayout level3 = new LinearLayout (context);
			level3.LayoutParameters = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (25f, context), LinearLayout.LayoutParams.FillParent);
			level3.SetBackgroundColor (Color.Brown);
			level3.Orientation = Orientation.Vertical;
			
			ImageView addThing = new ImageView (context);
			addThing.SetBackgroundResource (Resource.Drawable.add);
			viewParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (23f, context), (int)ImageHelper.convertDpToPixel (23f, context));
			viewParams.SetMargins (0, (int)ImageHelper.convertDpToPixel (15f, context), 0, 0);
			addThing.LayoutParameters = viewParams;
			ImageView wipeThing = new ImageView (context);
			wipeThing.SetBackgroundResource (Resource.Drawable.delete);
			viewParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (23f, context), (int)ImageHelper.convertDpToPixel (23f, context));
			viewParams.SetMargins (0, (int)ImageHelper.convertDpToPixel (10f, context), 0, 0);
			wipeThing.LayoutParameters = viewParams;
			
			level3.AddView (addThing);
			level3.AddView (wipeThing);
			
			LinearLayout level4 = new LinearLayout (context);
			level4.Orientation = Orientation.Vertical;
			level4.SetBackgroundColor (Color.Brown);
			level4.LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent);
			
			SeekBar scroller = new SeekBar (context);
			viewParams = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.MatchParent);
			viewParams.SetMargins ((int)ImageHelper.convertDpToPixel (5f, context), (int)ImageHelper.convertDpToPixel (15f, context), 0, (int)ImageHelper.convertDpToPixel (20.8f, context));
			scroller.LayoutParameters = viewParams;
			level4.AddView (scroller);
			
			holder.AddView (level2);
			holder.AddView (level3);
			holder.AddView (level4);
			
			return holder;
		}

		private LinearLayout createPaintBrushes ()
		{
			LinearLayout holder = new LinearLayout (context);
			holder.Orientation = Orientation.Vertical;
			holder.SetGravity (GravityFlags.Center);
			holder.LayoutParameters = new ViewGroup.LayoutParams (LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent);

			LinearLayout hardBrushes = new LinearLayout (context);
			hardBrushes.Orientation = Orientation.Horizontal;
			hardBrushes.LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);

			LinearLayout softBrushes = new LinearLayout (context);
			softBrushes.Orientation = Orientation.Horizontal;
			softBrushes.LayoutParameters = new LinearLayout.LayoutParams (LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);

			ImageView brush1 = new ImageView (context);
			LinearLayout.LayoutParams viewParams;
			viewParams = new LinearLayout.LayoutParams ((int)ImageHelper.convertDpToPixel (24f, context), (int)ImageHelper.convertDpToPixel (24f, context));
			viewParams.SetMargins ((int)ImageHelper.convertDpToPixel (5f, context), 0, 
				(int)ImageHelper.convertDpToPixel (5f, context), (int)ImageHelper.convertDpToPixel (5f, context));
			brush1.LayoutParameters = viewParams;
			brush1.Tag = 1;
			brush1.SetImageResource (Resource.Drawable.HardBrush1);
			brush1.Click += new EventHandler (brush_Clicked);
			ImageView brush2 = new ImageView (context);
			brush2.LayoutParameters = viewParams;
			brush2.Tag = 2;
			brush2.Click += new EventHandler (brush_Clicked);
			brush2.SetImageResource (Resource.Drawable.HardBrush2);
			ImageView brush3 = new ImageView (context);
			brush3.LayoutParameters = viewParams;
			brush3.Tag = 3;
			brush3.Click += new EventHandler (brush_Clicked);
			brush3.SetImageResource (Resource.Drawable.HardBrush3);
			ImageView brush4 = new ImageView (context);
			brush4.LayoutParameters = viewParams;
			brush4.Tag = 4;
			brush4.Click += new EventHandler (brush_Clicked);
			brush4.SetImageResource (Resource.Drawable.HardBrush4);
			ImageView brush5 = new ImageView (context);
			brush5.LayoutParameters = viewParams;
			brush5.Tag = 5;
			brush5.Click += new EventHandler (brush_Clicked);
			brush5.SetImageResource (Resource.Drawable.HardBrush5);
			ImageView brush6 = new ImageView (context);
			brush6.LayoutParameters = viewParams;
			brush6.Tag = 6;
			brush6.Click += new EventHandler (brush_Clicked);
			brush6.SetImageResource (Resource.Drawable.HardBrush6);
			
			hardBrushes.AddView (brush1);
			hardBrushes.AddView (brush2);
			hardBrushes.AddView (brush3);
			hardBrushes.AddView (brush4);
			hardBrushes.AddView (brush5);
			hardBrushes.AddView (brush6);

			ImageView sbrush1 = new ImageView (context);
			sbrush1.LayoutParameters = viewParams;
			sbrush1.Tag = 10;
			sbrush1.SetImageResource (Resource.Drawable.SoftBrush1);
			sbrush1.Click += new EventHandler (brush_Clicked);
			ImageView sbrush2 = new ImageView (context);
			sbrush2.LayoutParameters = viewParams;
			sbrush2.Tag = 11;
			sbrush2.Click += new EventHandler (brush_Clicked);
			sbrush2.SetImageResource (Resource.Drawable.SoftBrush2);
			ImageView sbrush3 = new ImageView (context);
			sbrush3.LayoutParameters = viewParams;
			sbrush3.Tag = 12;
			sbrush3.Click += new EventHandler (brush_Clicked);
			sbrush3.SetImageResource (Resource.Drawable.SoftBrush3);
			ImageView sbrush4 = new ImageView (context);
			sbrush4.LayoutParameters = viewParams;
			sbrush4.Tag = 13;
			sbrush4.Click += new EventHandler (brush_Clicked);
			sbrush4.SetImageResource (Resource.Drawable.SoftBrush4);
			ImageView sbrush5 = new ImageView (context);
			sbrush5.LayoutParameters = viewParams;
			sbrush5.Tag = 14;
			sbrush5.Click += new EventHandler (brush_Clicked);
			sbrush5.SetImageResource (Resource.Drawable.SoftBrush5);
			ImageView sbrush6 = new ImageView (context);
			sbrush6.LayoutParameters = viewParams;
			sbrush6.Tag = 15;
			sbrush6.Click += new EventHandler (brush_Clicked);
			sbrush6.SetImageResource (Resource.Drawable.SoftBrush6);
			
			softBrushes.AddView (sbrush1);
			softBrushes.AddView (sbrush2);
			softBrushes.AddView (sbrush3);
			softBrushes.AddView (sbrush4);
			softBrushes.AddView (sbrush5);
			softBrushes.AddView (sbrush6);

			holder.AddView (hardBrushes);
			holder.AddView (softBrushes);

			return holder;
		}
	}
}

