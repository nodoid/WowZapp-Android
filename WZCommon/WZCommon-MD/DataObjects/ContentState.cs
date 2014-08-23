// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using System.Linq;
using System.Collections.Generic;
using LOLMessageDelivery;
using SQLite;
using WZCommon;
using PreserveProps = Android.Runtime.PreserveAttribute;
namespace WZCommon
{
    [PreserveProps(AllMembers=true)]
    public class ContentState
    {
		#region Constructors

        public ContentState(MessageDB message)
        {

            this.Message = message;
            this.MessageSteps = this.Message.MessageStepDBList
				.ToDictionary(s => s.StepNumber, s => s);

            this.ContentPackIDQ = new Queue<int>();
            this.VoiceIDQ = new Queue<int>();
            this.PollingIDQ = new Queue<int>();
            this.AnimationIDQ = new Queue<int>();

            this.CreateCriteriaQueues();
        }



        public ContentState()
        {

        }//end default ctor

		#endregion Constructors



		#region Properties

        [PrimaryKey, AutoIncrement]
        public int ID
        {
            get;
            private set;
        }//end int ID


        [Indexed]
        public int ContentInfoID
        {
            get;
            set;
        }//end int ContentInfoID



        [Ignore]
        public MessageDB Message
        {
            get;
            private set;
        }//end MessageDB Message



        [Ignore]
        public Dictionary<int, MessageStepDB> MessageSteps
        {
            get;
            private set;
        }//end Dictionary<int, MessageStepDB> MessageSteps




        /// <summary>
        /// ContentPackItemIDs for downloading content pack items.
        /// </summary>
        [Ignore]
        public Queue<int> ContentPackIDQ
        {
            get;
            private set;
        }//end Queue<int> ContentPackIDQ



        /// <summary>
        /// StepNumbers for downloading voice recordings.
        /// </summary>
        [Ignore]
        public Queue<int> VoiceIDQ
        {
            get;
            private set;
        }//end Queue<int> VoiceIDQ



        /// <summary>
        /// StepNumbers for downloading polling steps.
        /// </summary>
        [Ignore]
        public Queue<int> PollingIDQ
        {
            get;
            private set;
        }//end Queue<int> PollingIDQ



        /// <summary>
        /// StepNumbers for downloading animation steps
        /// </summary>
        [Ignore]
        public Queue<int> AnimationIDQ
        {
            get;
            private set;
        }//end Queue<int> AnimationIDQ




        [Ignore]
        public bool HasContentPackItems
        {
            get
            {
                return this.ContentPackIDQ.Count > 0;
            }//end get

        }//end bool HasContentPackItems



        [Ignore]
        public bool HasVoiceRecordings
        {
            get
            {
                return this.VoiceIDQ.Count > 0;
            }//end get

        }//end bool HasVoiceRecordings



        [Ignore]
        public bool HasPollingSteps
        {
            get
            {
                return this.PollingIDQ.Count > 0;
            }//end get

        }//end bool HasPollingSteps





        [Ignore]
        public bool HasAnimationSteps
        {
            get
            {
                return this.AnimationIDQ.Count > 0;
            }//end get

        }//end bool HAsAnimationSteps




        [Ignore]
        public bool HasContentForDownload
        {
            get
            {
                return this.HasContentPackItems || this.HasVoiceRecordings || this.HasPollingSteps || this.HasAnimationSteps;
            }//end get

        }//end bool HasContentForDownload

		#endregion Properties




		#region Private methods

        private void CreateCriteriaQueues()
        {

            foreach (MessageStepDB eachMessageStep in this.MessageSteps.Values)
            {

                switch (eachMessageStep.StepType)
                {

                    case MessageStep.StepTypes.Comix:
                    case MessageStep.StepTypes.Comicon:
                    case MessageStep.StepTypes.SoundFX:
                    case MessageStep.StepTypes.Emoticon:

                        this.ContentPackIDQ.Enqueue(eachMessageStep.ContentPackItemID);

                        break;

                    case MessageStep.StepTypes.Voice:

                        this.VoiceIDQ.Enqueue(eachMessageStep.StepNumber);

                        break;

                    case MessageStep.StepTypes.Polling:

                        this.PollingIDQ.Enqueue(eachMessageStep.StepNumber);

                        break;

                    case MessageStep.StepTypes.Animation:

                        this.AnimationIDQ.Enqueue(eachMessageStep.StepNumber);

                        break;

                }//end switch

            }//end foreach

        }//end void CreateCriteriaQueues

		#endregion Private methods



		#region Public methods

        /// <summary>
        /// Removes the locally existing items, so they won't have to be downloaded again.
        /// </summary>
        /// <param name='existingContentPackItemIDs'>
        /// Existing content pack item Ids to remove.
        /// </param>
        /// <param name='existingVoiceIDs'>
        /// Existing voice Ids to remove.
        /// </param>
        /// <param name='existingPollingSteps'>
        /// Existing polling steps to remove.
        /// </param>
        public void RemoveExistingItems(List<int> existingContentPackItemIDs, List<int> existingVoiceIDs, List<int> existingPollingSteps, List<int> existingAnimationSteps)
        {

            if (null != existingContentPackItemIDs &&
                existingContentPackItemIDs.Count > 0 &&
                this.HasContentPackItems)
            {

                List<int> contentPackItemIDListInMessage = this.ContentPackIDQ.ToList();
                foreach (int contentPackItemID in existingContentPackItemIDs)
                {

                    if (contentPackItemIDListInMessage.Contains(contentPackItemID))
                    {
                        contentPackItemIDListInMessage.Remove(contentPackItemID);
                    }//end if

                }//end foreach

                this.ContentPackIDQ.Clear();
                contentPackItemIDListInMessage.ForEach(s => {

                    this.ContentPackIDQ.Enqueue(s);

                });

            }//end if



            if (null != existingVoiceIDs &&
                existingVoiceIDs.Count > 0 &&
                this.HasVoiceRecordings)
            {

                List<int> voiceIDListInMessage = this.VoiceIDQ.ToList();
                foreach (int voiceFileID in existingVoiceIDs)
                {
                    if (voiceIDListInMessage.Contains(voiceFileID))
                    {
                        voiceIDListInMessage.Remove(voiceFileID);
                    }//end if
                }//end foreach

                this.VoiceIDQ.Clear();
                voiceIDListInMessage.ForEach(s => {

                    this.VoiceIDQ.Enqueue(s);

                });

            }//end if



            if (null != existingPollingSteps &&
                existingPollingSteps.Count > 0 &&
                this.HasPollingSteps)
            {

                List<int> pollingStepIDListInMessage = this.PollingIDQ.ToList();
                foreach (int pollingStepID in existingPollingSteps)
                {
                    if (pollingStepIDListInMessage.Contains(pollingStepID))
                    {
                        pollingStepIDListInMessage.Remove(pollingStepID);
                    }//end if
                }//end foreach

                this.PollingIDQ.Clear();
                pollingStepIDListInMessage.ForEach(s => {

                    this.PollingIDQ.Enqueue(s);

                });

            }//end if



            if (null != existingAnimationSteps &&
                existingAnimationSteps.Count > 0 &&
                this.HasAnimationSteps)
            {

                List<int> animationStepIDListInMessage = this.AnimationIDQ.ToList();
                foreach (int animationStepID in existingAnimationSteps)
                {
                    if (animationStepIDListInMessage.Contains(animationStepID))
                    {
                        animationStepIDListInMessage.Remove(animationStepID);
                    }//end if

                }//end foreach
                this.AnimationIDQ.Clear();
                animationStepIDListInMessage.ForEach(s => {

                    this.AnimationIDQ.Enqueue(s);

                });

            }//end if

        }//end void RemoveExistingItems

		#endregion Public methods
    }
}

