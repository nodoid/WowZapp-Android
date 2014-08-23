// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;

namespace WZCommon
{
	public class Pair<TItemA, TItemB>
	{
		
		public Pair(TItemA itemA, TItemB itemB)
		{
			
			this.ItemA = itemA;
			this.ItemB = itemB;
			
		}//end ctor
		
		
		
		public TItemA ItemA
		{
			get;
			set;
		}//end TItemA ItemA
		
		
		
		public TItemB ItemB
		{
			get;
			set;
		}//end TItemB ItemB
		
		
		
		public override string ToString ()
		{
			return string.Format ("[Pair: ItemA={0}, ItemB={1}]", ItemA, ItemB);
		}
		
		
		
		public override int GetHashCode ()
		{
			return this.ItemA.GetHashCode() ^ this.ItemB.GetHashCode();
		}
		
		
		
		public override bool Equals (object obj)
		{
			
			if (null == obj)
			{
				return false;
			}//end if
			
			Pair<TItemA, TItemB> otherObj = obj as Pair<TItemA, TItemB>;
			if ((object)otherObj == null)
			{
				return false;
			}//end if
			
			return this.ItemA.Equals(otherObj.ItemA) && this.ItemB.Equals(otherObj.ItemB);
		}
		
		
		
		public static bool operator == (Pair<TItemA, TItemB> first, Pair<TItemA, TItemB> second)
		{
			
			if (object.ReferenceEquals(first, second))
			{
				return true;
			}//end if
			
			if ((object)first == null ||
			    (object)second == null)
			{
				return false;
			}//end if
			
			return first.ItemA.Equals(second.ItemA) && first.ItemB.Equals(second.ItemB);
			
		}//end static bool operator ==
		
		
		
		public static bool operator != (Pair<TItemA, TItemB> first, Pair<TItemA, TItemB> second)
		{
			
			return !(first == second);
			
		}//end static bool operator !=
		
	}//end class Pair
}

