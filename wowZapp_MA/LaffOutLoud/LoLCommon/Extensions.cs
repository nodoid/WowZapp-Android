// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using LOLMessageDelivery;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
using LOLMessageDelivery.Classes.LOLAnimation;

using WZCommon;

namespace LOLApp_Common
{
	public static class Extensions
	{

		public static T MaxItem<T, TCompare>(this IEnumerable<T> collection, Func<T, TCompare> func) where TCompare : IComparable<TCompare>
		{
	         T maxItem = default(T);
	         TCompare maxValue = default(TCompare);
	         foreach (var item in collection)
	         {
	             TCompare temp = func(item);
	             if (maxItem == null || temp.CompareTo(maxValue) > 0)
	             {
	                maxValue = temp;
	                maxItem = item;
	            }
	        }
	        return maxItem;
	    }


		//FIXME: If T is a struct, it will just return default(T)
		// In this case, use MinItemEx instead
		public static T MinItem<T, TCompare>(this IEnumerable<T> collection, Func<T, TCompare> func) where TCompare : IComparable<TCompare>
		{

			T minItem = default(T);
			TCompare minValue = default(TCompare);
	        foreach (var item in collection)
	        {
	            TCompare temp = func(item);

	            if (minItem == null || temp.CompareTo(minValue) < 0)
	            {
	               minValue = temp;
	               minItem = item;
	           }
	        }
	        return minItem;
	    }




		public static T MinItemEx<T, TCompare>(this IEnumerable<T> collection, Func<T, TCompare> func) where TCompare : IComparable<TCompare>
		{

			T minItem = collection.First();
			TCompare minValue = func(collection.First());

			foreach (var item in collection)
			{

				TCompare temp = func(item);

				if (minItem == null || temp.CompareTo(minValue) < 0)
				{

					minValue = temp;
					minItem = item;

				}//end if

			}//end foreach

			return minItem;

		}//end static T




		public static RectangleF Rotate(this RectangleF owner, double degAngle)
		{

			PointF center = new PointF(owner.X + (owner.Width / 2f), owner.Y + (owner.Height / 2f));
			
			double radAngle = degAngle * LOLConstants.DegToRad;
			float angleCos = (float)Math.Cos(radAngle);
			float angleSin = (float)Math.Sin(radAngle);

			List<PointF> rectCorners = new List<PointF>() {

				owner.Location,
				new PointF(owner.Right, owner.Y),
				new PointF(owner.Right, owner.Bottom),
				new PointF(owner.X, owner.Bottom)

			};

			List<PointF> rotatedPoints = new List<PointF>();

			foreach (PointF eachCorner in rectCorners)
			{

				// Translate point
				float tx = eachCorner.X - center.X;
				float ty = eachCorner.Y - center.Y;
				
				// Rotate it
				float rx = (tx * angleCos) - (ty * angleSin);
				float ry = (ty * angleCos) + (tx * angleSin);
				
				// Translate back
				rx += center.X;
				ry += center.Y;
				
				eachCorner.X = rx;
				eachCorner.Y = ry;

				rotatedPoints.Add(eachCorner);
				
			}//end foreach

			return rotatedPoints.GetBoundingBox();

		}//end static RectangleF Rotate





		public static List<PointF> RotatePoints(this IEnumerable<PointF> owner, double degAngle, PointF fixedPoint)
		{

			List<PointF> toReturn = new List<PointF>();

			double radAngle = degAngle * LOLConstants.DegToRad;
			float angleCos = (float)Math.Cos(radAngle);
			float angleSin = (float)Math.Sin(radAngle);

			foreach (PointF eachPoint in owner)
			{

				// Translate point
				float tx = eachPoint.X - fixedPoint.X;
				float ty = eachPoint.Y - fixedPoint.Y;

				// Rotate it
				float rx = (tx * angleCos) - (ty * angleSin);
				float ry = (ty * angleCos) + (tx * angleSin);

				// Translate back
				rx += fixedPoint.X;
				ry += fixedPoint.Y;

				toReturn.Add(new PointF(rx, ry));

			}//end foreach

			return toReturn;

		}//end List<PointF> RotatePoints




		public static List<PointF> GetCorners(this RectangleF owner)
		{

			List<PointF> toReturn = new List<PointF>();
			toReturn.Add(owner.Location);
			toReturn.Add(new PointF(owner.Right, owner.Y));
			toReturn.Add(new PointF(owner.Right, owner.Bottom));
			toReturn.Add(new PointF(owner.Left, owner.Bottom));

			return toReturn;

		}//end static RectangleF GetCorners





		public static RectangleF GetBoundingBox(this IEnumerable<PointF> owner)
		{

			PointF minXPoint = 
				owner
					.MinItemEx<PointF, float>(s => s.X);
			PointF maxXPoint = 
				owner
					.MaxItem<PointF, float>(s => s.X);
			PointF minYPoint = 
				owner
					.MinItemEx<PointF, float>(s => s.Y);
			PointF maxYPoint = 
				owner
					.MaxItem<PointF, float>(s => s.Y);
			
			return new RectangleF(minXPoint.X,
			                      minYPoint.Y,
			                      maxXPoint.X - minXPoint.X,
			                      maxYPoint.Y - minYPoint.Y);

		}//end static RectangleF GetBoundingBox




		public static RectangleF CenterInRect(this RectangleF owner, SizeF size)
		{

			return new RectangleF(owner.X + ((owner.Width / 2f) - (size.Width / 2f)),
			                      owner.Y + ((owner.Height / 2f) - (size.Height / 2f)),
			                      size.Width,
			                      size.Height);

		}//end static RectangleF CenterInRect




		public static bool IsSameAs(this LOLMessageConversation owner, LOLMessageConversation other)
		{

			if (owner.Recipients.Count != other.Recipients.Count)
			{
				return false;
			}//end if

			return owner.Recipients.TrueForAll(s => other.Recipients.Contains(s));

		}//end static bool IsSameAs




		public static LOLMessageConversation Copy(this LOLMessageConversation owner)
		{

			LOLMessageConversation toReturn = new LOLMessageConversation();
			toReturn.LastMessageDate = owner.LastMessageDate;
			toReturn.MessageIDs = new List<Guid>(owner.MessageIDs);
			toReturn.ReadMessageIDs = new List<Guid>(owner.ReadMessageIDs);
			toReturn.Recipients = new List<Guid>(owner.Recipients);

			return toReturn;

		}//end static LOLMessageConversationCopy





		public static Task<TResult> CallAsyncMethod<TResult, TEventArgs>(this LOLConnectClient service,
		                                                                 Action<LOLConnectClient, EventHandler<TEventArgs>, bool> hookAction,
		                                                                 Action<LOLConnectClient> callAction)
			where TEventArgs : AsyncCompletedEventArgs
		{

			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();

			EventHandler<TEventArgs> callback = null;
			callback = (s, e) => {

				try
				{

					if (null == e.Error)
					{
						
						Type argsType = typeof(TEventArgs);
						PropertyInfo propInfo = argsType.GetProperty("Result");
						TResult result = (TResult)propInfo.GetValue(e, null);
						tcs.SetResult(result);
						
					} else if (e.Cancelled)
					{
						tcs.SetCanceled();
					} else
					{
						tcs.SetException(e.Error);
					}//end if else

				} catch (Exception ex)
				{
					tcs.SetException(ex);
				} finally
				{
					hookAction(service, callback, false);
				}//end try catch
				
			};
			
			hookAction(service, callback, true);
			callAction(service);
			
			return tcs.Task;

		}//end static Task<TResult> CallAsyncMethod





		public static Task<TResult> CallAsyncMethod<TResult, TEventArgs>(this LOLMessageClient service,
		                                                                 Action<LOLMessageClient, EventHandler<TEventArgs>, bool> hookAction,
		                                                                 Action<LOLMessageClient> callAction)
			where TEventArgs : AsyncCompletedEventArgs
		{


			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();

			EventHandler<TEventArgs> callback = null;
			callback = (s, e) => {

				try
				{

					if (null == e.Error)
					{

						Type argsType = typeof(TEventArgs);
						PropertyInfo propInfo = argsType.GetProperty("Result");
						TResult result = (TResult)propInfo.GetValue(e, null);
						tcs.SetResult(result);

					} else if (e.Cancelled)
					{
						tcs.SetCanceled();
					} else
					{
						tcs.SetException(e.Error);
					}//end if else

				} catch (Exception ex)
				{
					tcs.SetException(ex);
				} finally
				{
					hookAction(service, callback, false);
				}//end try catch finally

			};

			hookAction(service, callback, true);
			callAction(service);

			return tcs.Task;

		}//end static Task<TResult> CallAsyncMethod



		public static void SetupNulls(this ScreenObject owner)
		{

			if (null == owner)
			{
				throw new NullReferenceException("Owner object is null!");
			}//end if

			if (null == owner.DrawnPath)
			{
				owner.DrawnPath = new List<PointF>();
			}//end if

			if (null == owner.DrawnPathBrushByteData)
			{
				owner.DrawnPathBrushByteData = new byte[0];
			}//end if

			if (null == owner.DrawnPathColor)
			{
				owner.DrawnPathColor = string.Empty;
			}//end if

			if (null == owner.DrawnPathColors)
			{
				owner.DrawnPathColors = new List<string>();
			}//end if

			if (null == owner.DrawnPaths)
			{
				owner.DrawnPaths = new List<List<PointF>>();
			}//end if

			if (null == owner.DrawnPathsBrushByteData)
			{
				owner.DrawnPathsBrushByteData = new List<byte[]>();
			}//end if

			if (null == owner.DrawnPathStrokes)
			{
				owner.DrawnPathStrokes = new List<float>();
			}//end if

			if (null == owner.TextObjectFontSizes)
			{
				owner.TextObjectFontSizes = new List<float>();
			}//end if

			if (null == owner.TextObjectPositions)
			{
				owner.TextObjectPositions = new List<PointF>();
			}//end if

			if (null == owner.TextObjectsContent)
			{
				owner.TextObjectsContent = new List<string>();
			}//end if

			if (null == owner.Transitions)
			{
				owner.Transitions = new List<Transition>();
			}//end if

		}//end static void SetupNulls




		public static void SetupNulls(this Animation owner)
		{

			if (null == owner)
			{
				throw new NullReferenceException("Owner object is null!");
			}//end if

			if (null == owner.ScreenObjects)
			{
				owner.ScreenObjects = new List<ScreenObject>();
			}//end if

		}//end static void SetupNulls




		public static bool IsBetween(this double owner, double left, double right, BoundsEqualityType eqType)
		{

			switch (eqType)
			{

			case BoundsEqualityType.Neither:

				return owner > left && owner < right;

			case BoundsEqualityType.EqualsBoth:

				return owner >= left && owner <= right;

			case BoundsEqualityType.EqualsLeftOnly:

				return owner >= left && owner < right;

			case BoundsEqualityType.EqualsRightOnly:

				return owner > left && owner <= right;

			default:

				throw new InvalidOperationException("Unknown BoundsEqualityType!");

			}//end switch

		}//end static bool IsBetween



		public static double DistanceFrom(this PointF owner, PointF targetPoint)
		{

			double powX = Math.Pow(Convert.ToDouble(owner.X - targetPoint.X), 2);
			double powY = Math.Pow(Convert.ToDouble(owner.Y - targetPoint.Y), 2);

			return Math.Sqrt(powX + powY);

		}//end static double DistanceFrom

	}

}
