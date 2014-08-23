// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Drawing;
using SQLite;
using LOLMessageDelivery.Classes.LOLAnimation;

namespace WZCommon
{
    public class BrushItem
    {

		#region Constructors

//		public BrushItem (float thickness, BrushType brushType, CGColor brushColor)
//		{
//			this.Thickness = thickness;
//			this.BrushType = brushType;
//			this.BrushColor = brushColor;
//
//			if (this.BrushType == BrushType.Spray)
//			{
//
//				this.IsSprayBrushActive = true;
//
//			}//end if
//
//			this.inactiveColor = new CGColor(DrawingCanvasViewEx.InactiveColorValues[0],
//			                                 DrawingCanvasViewEx.InactiveColorValues[1],
//			                                 DrawingCanvasViewEx.InactiveColorValues[2],
//			                                 DrawingCanvasViewEx.InactiveColorValues[3]);
//
//		}



        public BrushItem(float thickness, AnimationTypesBrushType brushType, WZColor brushColor, WZColor inactiveBrushColor, byte[] brushImage)
        {

            this.Thickness = thickness;
            this.BrushType = brushType;
            this.BrushColor = brushColor;
            this.BrushImageBuffer = brushImage;

            if (this.BrushType == AnimationTypesBrushType.Spray)
            {
                this.IsSprayBrushActive = true;
            }//end if

            this.InactiveBrushColor = inactiveBrushColor;

        }//end ctor




        public BrushItem()
        {

        }//end BrushItem

		#endregion Constructors





		#region Properties

        [PrimaryKey, AutoIncrement]
        public int DBID
        {
            get;
            private set;
        }//end int DBID




        public float Thickness
        {
            get;
            private set;
        }//end float Thickness




        public AnimationTypesBrushType BrushType
        {
            get;
            private set;
        }//end BrushType BrushType





        public byte[] BrushImageBuffer
        {
            get;
            set;
        }//end byte[] BrushImageBuffer





		#region BrushColor

        [Ignore]
        public WZColor BrushColor
        {
            get;
            private set;
        }//end CGColor BrushColor



        public byte[] BrushColorBuffer
        {
            get
            {
                if (null != this.BrushColor)
                {
//					return AnimationUtils.ConvertRGBAColorToByteArray(this.BrushColor);
                    return this.BrushColor.ToByteArray();
                } else
                {
                    return null;
                }//end if else

            }
            private set
            {

                byte[] buffer = value;
                if (null != buffer)
                {
//					this.BrushColor = AnimationUtils.ConvertByteArrayToRGBAColor(buffer);
                    this.BrushColor = WZColor.ConvertByteArrayToRGBAColor(buffer);
                } else
                {
                    this.BrushColor = null;
                }//end if else

            }//end get private set

        }//end byte[] BrushColorBuffer




        [Ignore]
        public WZColor InactiveBrushColor
        {
            get;
            private set;
        }//end WZColor InactiveBrushColor




        public byte[] InactiveBrushColorBuffer
        {
            get
            {

                if (null != this.InactiveBrushColor)
                {
                    return this.InactiveBrushColor.ToByteArray();
                } else
                {
                    return null;
                }//end if else

            }
            private set
            {

                byte[] buffer = value;
                if (null != buffer)
                {
                    this.InactiveBrushColor = WZColor.ConvertByteArrayToRGBAColor(buffer);
                } else
                {
                    this.InactiveBrushColor = null;
                }//end if else

            }//end get private set
        }//end byte[] InactiveBrushColorBuffer



		#endregion BrushColor

        public bool IsSprayBrushActive
        {
            get;
            private set;
        }//end bool IsSprayBrushActive

		#endregion Properties




		#region Private methods




		#endregion Private methods





		#region Public methods

        public void ChangeBrushImageColor(WZColor toColor, byte[] imageBrush)
        {

            this.BrushColor = toColor;
            if (this.BrushType == AnimationTypesBrushType.Spray)
            {
//				this.pBrushImage = this.GetBrushImageWithColor(this.BrushImage, toColor);
                this.BrushImageBuffer = imageBrush;
            }//end if

        }//end void ChangeBrushImageColor




        public void SetBrushActive(bool active, byte[] imageBrush)
        {

            if (this.BrushType == AnimationTypesBrushType.Spray)
            {

                this.IsSprayBrushActive = active;
                this.BrushImageBuffer = imageBrush;

            } else
            {

                throw new InvalidOperationException("Cannot activate/deactivate the brush for Normal brush type.");

            }//end if else

        }//end void SetBrushActive

		#endregion Public methods





		#region Overrides

        public override int GetHashCode()
        {
            return this.Thickness.GetHashCode() ^ this.BrushType.GetHashCode();
        }



        public override bool Equals(object obj)
        {
            BrushItem other = obj as BrushItem;
            if (null == (object)other)
            {
                return false;
            } else
            {

                return this.Thickness.Equals(other.Thickness) &&
                    this.BrushType.Equals(other.BrushType);

            }//end if else
        }


        public override string ToString()
        {
            return string.Format("[BrushItem: Thickness={0}, BrushType={1}, BrushImage={2}]", Thickness, BrushType, BrushImageBuffer);
        }



		#region Operators

        public static bool operator ==(BrushItem first, BrushItem second)
        {

            if (object.ReferenceEquals(first, second))
            {
                return true;
            }//end if

            if ((object)first == null || (object)second == null)
            {
                return false;
            }//end if

            return first.Equals(second);

        }//end static bool operator





        public static bool operator !=(BrushItem first, BrushItem second)
        {

            return !(first == second);

        }//end static bool operator !=

		#endregion Operators


		#endregion Overrides


    }
}

