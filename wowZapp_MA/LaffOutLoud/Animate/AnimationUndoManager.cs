using System;
using System.Collections.Generic;

namespace wowZapp.Animate
{
	public class AnimationUndoManager<TUndoInfo> where TUndoInfo : class
	{

		public AnimationUndoManager (List<TUndoInfo> undoItems)
		{
			this.undoStack = new Stack<TUndoInfo> ();
			
			foreach (TUndoInfo eachUndoItem in undoItems) {
				this.PushUndoItem (eachUndoItem);
			}//end foreach
		}

		private Stack<TUndoInfo> undoStack;

		public int ItemCount {
			get {
				return this.undoStack.Count;
			}//end get
			
		}//end int ItemCount

		public void PushUndoItem (TUndoInfo undoItem)
		{
#if(DEBUG)
			System.Diagnostics.Debug.WriteLine ("Will push undo item: {0}", undoItem);
#endif
			this.undoStack.Push (undoItem);
		}//end void PushUndoItem

		public TUndoInfo PopUndoItem ()
		{
			if (this.undoStack.Count == 0) {
#if(DEBUG)
				System.Diagnostics.Debug.WriteLine ("No undo items in the stack!");
#endif
				return null;
			} else {
				TUndoInfo undoItem = this.undoStack.Pop ();	
#if(DEBUG)
				System.Diagnostics.Debug.WriteLine ("Popped undo item: {0}", undoItem);
#endif
				return undoItem;			
			}//end if else
		}//end TUndoInfo PopUndoItem

		public TUndoInfo PeekUndoItem ()
		{			
			return this.undoStack.Peek ();	
		}//end TUndoInfo PeekUndoItem

		public IEnumerable<TUndoInfo> EnumerateUndoItems ()
		{
			foreach (TUndoInfo eachItem in this.undoStack) {		
				yield return eachItem;	
			}//end foreach
			
		}//end TUndoInfo EnumerateUndoItems
	}
}

