// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace WZCommon
{
	public class AnimationAudioInfo
	{

		#region Constructors

		public AnimationAudioInfo (int id, int frameID, byte[] audioBuffer, double duration)
		{
			this.ID = id;
			this.AudioBuffer = audioBuffer;
			this.Duration = duration;
			this.FrameID = frameID;
		}



		public AnimationAudioInfo()
		{

		}//end if

		#endregion Constructors




		#region Properties

		public int ID
		{
			get;
			private set;
		}//end int ID



		[PrimaryKey, AutoIncrement]
		public int DBID
		{
			get;
			private set;
		}//end int DBID



		[Indexed]
		public int FrameID
		{
			get;
			private set;
		}//end int FrameID



		[Indexed]
		public int FrameDBID
		{
			get;
			set;
		}//end int FrameDBID



		public byte[] AudioBuffer
		{
			get;
			private set;
		}//end byte[] AudioBuffer



		public double Duration
		{
			get;
			private set;
		}//end double Duration

		#endregion Properties



		#region Public methods

		public void SetToFrameID(int frameID)
		{

			this.FrameID = frameID;

		}//end void SetToFrameID

		#endregion Public methods




		#region Overrides

		public override string ToString ()
		{
			return string.Format ("[AnimationAudioInfo: ID={0}, FrameID={1}, AudioBuffer={2}, Duration={3}]", ID, FrameID, AudioBuffer, Duration);
		}

		#endregion Overrides
	}
}

