using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LOLApp_Common;
using LOLMessageDelivery.Classes.LOLAnimation;

using wowZapp.Animate;

namespace wowZapp.Animations
{
    public partial class CreateAnimationActivity : Activity
    {
		
        private void SaveAnimation()
        {
            /*ThreadPool.QueueUserWorkItem (delegate {
				try {
					dbm.SaveAnimation (FrameItems, AudioItems);
				} catch {
					return;
				}
		
				List<UndoInfo> undoInfo = undoManager ().EnumerateUndoItems ().ToList ();
				int sortOrder = 0;
				foreach (UndoInfo eachUndoItem in undoInfo)
					eachUndoItem.SortOrder = sortOrder++;
		
				dbm.SaveAnimationUndoInfo (undoInfo);
			});*/
        }
		
    }
}