using System;
using System.Collections.Generic;
using System.Linq;

using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace ScrollViewTest
{
	public class LScrollView : ScrollView
	{

		#region Constructors

		public LScrollView (Context context, IAttributeSet attrs) :
			base (context, attrs)
		{
			Initialize ();
		}



		public LScrollView (Context context, IAttributeSet attrs, int defStyle) :
			base (context, attrs, defStyle)
		{
			Initialize ();
		}


		#endregion Constructors



		#region Fields

		private LinearLayout baseLayout;
		private Dictionary<string, View> items;
		private Dictionary<int, LinearLayout> rows;
		private MotionEventActions lastTouchAction;

		#endregion Fields



		#region Events

		public event Action<string, View> ItemSelected;

		#endregion Events



		#region Private methods

		private void Initialize ()
		{
			this.baseLayout = new LinearLayout(this.Context);
			this.baseLayout.Orientation = Orientation.Vertical;
			this.baseLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
			this.baseLayout.SetGravity(GravityFlags.Center);

			this.baseLayout.SetBackgroundColor(Color.Transparent);

			this.AddView(this.baseLayout);

			this.items = new Dictionary<string, View>();
			this.rows = new Dictionary<int, LinearLayout>();
		}

		#endregion Private methods



		#region Overrides

		public override bool OnTouchEvent (MotionEvent e)
		{
			MotionEventActions action = e.Action;

			if (action == MotionEventActions.Up)
			{

				float cx = e.RawX;
				float cy = e.RawY;

				int x = (int)cx;
				int y = (int)cy;

				foreach (KeyValuePair<string, View> eachItem in this.items)
				{

					int[] location = new int[2];
					eachItem.Value.GetLocationOnScreen(location);

					int itemWidth = eachItem.Value.Width;
					int itemHeight = eachItem.Value.Height;

					if (x > location[0] && x < (location[0] + itemWidth) &&
					    y > location[1] && y < (location[1] + itemHeight))
					{

						if (null != this.ItemSelected)
						{
							this.ItemSelected(eachItem.Key, eachItem.Value);
						}//end if

						break;
					}//end if

				}//end foreach

			}//end if

			// TODO: Determine what the last action was
			this.lastTouchAction = action;

			return base.OnTouchEvent(e);
		}

		#endregion Overrides



		#region Public methods

		public void AddItem(string itemID, View item)
		{

			if (this.items.Count == 0)
			{

				item.LayoutParameters = new ViewGroup.LayoutParams(92, 92);
				item.SetPadding(10, 10, 10, 10);

				this.items[itemID] = item;

				LinearLayout firstRow = new LinearLayout(this.Context);
				firstRow.Orientation = Orientation.Horizontal;
				firstRow.SetHorizontalGravity(GravityFlags.Center);
				firstRow.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);

				firstRow.AddView(item);

				this.rows[0] = firstRow;

				this.baseLayout.AddView(firstRow);


			} else
			{

				if (this.items.ContainsKey(itemID))
				{
					throw new InvalidOperationException("An item with the same key already exists in the scroll view!");
				}//end if

				// Get the last row (LinearLayout)
				int lastRowIndex = this.rows.Keys.Max();
				LinearLayout lastRow = this.rows[lastRowIndex];

				item.LayoutParameters = new ViewGroup.LayoutParams(92, 92);
				item.SetPadding(10, 10, 10, 10);

				this.items[itemID] = item;

				if (lastRow.ChildCount < 3)
				{

					// Last row still has room
					lastRow.AddView(item);


				} else
				{

					LinearLayout newRow = new LinearLayout(this.Context);
					newRow.Orientation = Orientation.Horizontal;
					newRow.SetHorizontalGravity(GravityFlags.Center);
					newRow.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);

					newRow.AddView(item);

					this.rows[++lastRowIndex] = newRow;

					this.baseLayout.AddView(newRow);

				}//end if else

			}//end if else

		}//end void AddItem

		#endregion Public methods

	}

}

