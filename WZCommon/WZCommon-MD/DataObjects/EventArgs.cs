// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
using LOLMessageDelivery;
using System.Collections.Generic;
using LOLAccountManagement;
using System.Drawing;
using WZCommon;


namespace WZCommon
{
    public class IncomingMessageEventArgs : EventArgs
    {
        public IncomingMessageEventArgs(List<Message> messages) : base()
        {
            this.Messages = messages;
        }



        public List<Message> Messages
        {
            get;
            private set;
        }//end List<Message> Messages
    }



    public class MessageBubbleTouchedEventArgs : EventArgs
    {

        public MessageBubbleTouchedEventArgs(Message message, List<MessageStep> msgSteps, User userFrom)
        {

            this.Message = message;
            this.MessageSteps = msgSteps;
            this.UserFrom = userFrom;

        }//end MessageBubbleTouchedEventArgs



        public Message Message
        {
            get;
            private set;
        }//end Message Message



        public List<MessageStep> MessageSteps
        {
            get;
            private set;
        }//end List<MessageStep> MessageSteps



        public User UserFrom
        {
            get;
            private set;
        }//end User UserFrom


    }//end class MessageBubbleTouchedEventArgs



    public class MessageStepCreatedEventArgs : EventArgs
    {

        public MessageStepCreatedEventArgs(MessageStep msgStep, ContentPackItem packItem, string voiceRecordingFile, PollingStep pollStep, AnimationInfo animationInfo)
        {

            this.MessageStep = msgStep;
            this.ContentPackItem = packItem;
            this.VoiceRecordingFile = voiceRecordingFile;
            this.PollingStep = pollStep;
            this.AnimationInfo = animationInfo;

        }//end MessageStepCreatedEventArgs



        public MessageStep MessageStep
        {
            get;
            private set;
        }//end MessageStep MessageStep



        public ContentPackItem ContentPackItem
        {
            get;
            private set;
        }//end ContentPackItem ContentPackItem



        public string VoiceRecordingFile
        {
            get;
            private set;
        }//end string VoiceRecordingFile



        public PollingStep PollingStep
        {
            get;
            private set;
        }//end Pollingstep PollingStep



        public AnimationInfo AnimationInfo
        {
            get;
            private set;
        }//end AnimationInfo AnimationInfo


    }//end class MessageStepCreatedEventArgs







    public class AudioPlayProgressEventArgs : EventArgs
    {

        public AudioPlayProgressEventArgs(double progress, double duration, bool finished)
        {

            this.Progress = progress;
            this.Duration = duration;
            this.Finished = finished;

        }//end public AudioPlayProgressEventArgs



        public double Progress
        {
            get;
            private set;
        }//end double Progress




        public double Duration
        {
            get;
            private set;
        }//end double Duration



        public bool Finished
        {
            get;
            private set;
        }//end bool Finished


    }//end class AudioPlayProgressEventArgs

    public class VideoPlayProgressEventArgs : EventArgs
    {
		
        public VideoPlayProgressEventArgs(double progress, double duration, bool finished)
        {
			
            this.Progress = progress;
            this.Duration = duration;
            this.Finished = finished;
			
        }//end public AudioPlayProgressEventArgs
		
		
		
        public double Progress
        {
            get;
            private set;
        }//end double Progress
		
		
		
		
        public double Duration
        {
            get;
            private set;
        }//end double Duration
		
		
		
        public bool Finished
        {
            get;
            private set;
        }//end bool Finished
		
		
    }


    public class ImagePickerEventArgs<TImage> : EventArgs
		where TImage : class
    {

        public ImagePickerEventArgs(TImage image, string mediaUrl, MediaPickType mediaType, object state)
        {

            this.Image = image;
            this.MediaUrl = mediaUrl;
            this.MediaType = mediaType;
            this.UserState = state;

        }//end ImagePickerEventArgs



        public TImage Image
        {
            get;
            private set;
        }//end TImage Image



        public string MediaUrl
        {
            get;
            private set;
        }//end string MediaUrl



        public MediaPickType MediaType
        {
            get;
            private set;
        }//end MediaType



        public object UserState
        {
            get;
            private set;
        }//end object UserState

    }//end class ImagePickerEventArgs



    public class PollingStepVoteEventArgs : EventArgs
    {

        public PollingStepVoteEventArgs(PollingStep pollStep, int vote)
        {

            this.PollStep = pollStep;
            this.Vote = vote;

        }//end ctor


        public PollingStep PollStep
        {
            get;
            private set;
        }//end PollingStep PollStep


        public int Vote
        {
            get;
            private set;
        }//end int Vote

    }//end class PollingStepVoteEventArgs





    public class ImageLayerViewEventArgs<TImage> : EventArgs
    {

        public ImageLayerViewEventArgs(TImage image, RectangleF imageFrame, DrawingLayerType drType, int contentPackItemID)
        {

            this.Image = image;
            this.ImageFrame = imageFrame;
            this.DrawingType = drType;
            this.ContentPackItemID = contentPackItemID;

        }//end ImageLayerViewEventArgs



        public TImage Image
        {
            get;
            private set;
        }//end TImage Image



        public RectangleF ImageFrame
        {
            get;
            private set;
        }//end RectangleF ImageFrame



        public DrawingLayerType DrawingType
        {
            get;
            private set;
        }//end DrawingLayerType DrawingType



        public int ContentPackItemID
        {
            get;
            private set;
        }//end int ContentPackItemID

    }//end class ImageLayerViewEventArgs



    public class GridViewItemSelectedEventArgs<TItem> : EventArgs
    {

        public GridViewItemSelectedEventArgs(TItem item, int row, int column)
        {

            this.Item = item;
            this.Row = row;
            this.Column = column;

        }//end ctor



        public int Row
        {
            get;
            private set;
        }//end int Row



        public int Column
        {
            get;
            private set;
        }//end int Column



        public TItem Item
        {
            get;
            private set;
        }//end TDataObject DataObject

    }//end class GridViewItemSelectedEventArgs






    public class GridViewItemDraggedEventArgs<TItem> : EventArgs
    {

        public GridViewItemDraggedEventArgs(TItem draggedItem, bool started, bool ended)
        {

            this.DraggedItem = draggedItem;
            this.Started = started;
            this.Ended = ended;

        }//end ctor



        public TItem DraggedItem
        {
            get;
            private set;
        }//end TItem DraggedItem



        public bool Ended
        {
            get;
            private set;
        }//end bool Ended



        public bool Started
        {
            get;
            private set;
        }//end bool Started

    }//end GridViewItemDraggedEventArgs






    public class MessageSendConfirmEventArgs : EventArgs
    {

        public MessageSendConfirmEventArgs(MessageDB message)
        {
            this.Message = message;
        }//end ctor



        public MessageDB Message
        {
            get;
            private set;
        }//end MessageDB Message

    }//end class MessageSendConfirmEventArgs



    /*public class CanvasButtonPanelButtonClickedEventArgs : EventArgs
    {

        public CanvasButtonPanelButtonClickedEventArgs(CanvasButtonPanelButton button)
        {

            this.ButtonType = button;

        }


        public CanvasButtonPanelButton ButtonType
        {
            get;
            private set;
        }

    }*/



    /*
    public class LayerHandlerEventArgs : EventArgs
    {

        public LayerHandlerEventArgs(CanvasButtonPanelButton buttonType, 
		                             LayerInfo sourceLayer, LayerInfo targetLayer, 
		                             FrameLayerPair sourceLayerAddress,
		                             LayerHandlerMode layerMode, 
		                             DrawingLayerType layerType,
		                             TransitionInfo activeTransition, object userState)
        {

            this.ButtonType = buttonType;
            this.SourceLayer = sourceLayer;
            this.TargetLayer = targetLayer;
            this.ActiveTransition = activeTransition;
            this.LayerMode = layerMode;
            this.LayerType = layerType;
            this.SourceLayerAddress = sourceLayerAddress;
            this.UserState = userState;

        }



        public CanvasButtonPanelButton ButtonType
        {
            get;
            private set;
        }


        public LayerInfo SourceLayer
        {
            get;
            private set;
        }//end LayerInfo SourceLayer



        public LayerInfo TargetLayer
        {
            get;
            private set;
        }//end LayerInfo TargetLayer




        public FrameLayerPair SourceLayerAddress
        {
            get;
            private set;
        }//end FrameLayerPair SourceLayerAddress




        public TransitionInfo ActiveTransition
        {
            get;
            private set;
        }//end TransitionInfo ActiveTransition



        public LayerHandlerMode LayerMode
        {
            get;
            private set;
        }//end LayerHandlerMode LayerMode




        public DrawingLayerType LayerType
        {
            get;
            private set;
        }//end DrawingLayerType LayerType




        public object UserState
        {
            get;
            private set;
        }//end object UserState


    }//end class LayerHandlerEventArgs





    public class LayerHandlerModeChangedEventArgs : EventArgs
    {

        public LayerHandlerModeChangedEventArgs(GraphicsManipulationMode oldMode, 
		                                        GraphicsManipulationMode newMode, 
		                                        FrameLayerPair sourceLayerAddress, 
		                                        LayerInfo targetLayer)
        {

            this.NewMode = newMode;
            this.OldMode = oldMode;
            this.SourceLayerAddress = sourceLayerAddress;
            this.TargetLayer = targetLayer;

        }//end ctor



        public GraphicsManipulationMode NewMode
        {
            get;
            private set;
        }//end GraphicsManipulationMode NewMode



        public GraphicsManipulationMode OldMode
        {
            get;
            private set;
        }//end GraphicsManipulationMode OldMode



        public FrameLayerPair SourceLayerAddress
        {
            get;
            private set;
        }//end FrameLayerPair SourceLayerAddress



        public LayerInfo TargetLayer
        {
            get;
            private set;
        }//end LayerInfo TargetLayer

    }//end LayerHandlerModeChangedEventArgs






    public class DrawingInfoCreatedEventArgs : EventArgs
    {

        public DrawingInfoCreatedEventArgs(DrawingInfo drItem)
        {

            this.DrawingItem = drItem;

        }//end ctor



        public DrawingInfo DrawingItem
        {
            get;
            private set;
        }//end DrawingInfo DrawingItem

    }//end class DrawingCanvasDrawingInfoCreatedEventArgs




    public class TransitionIconTappedEventArgs : EventArgs
    {

        public TransitionIconTappedEventArgs(TransitionEffectMapping trEffectMap, GridRow row, int rowIndex, int columnIndex)
        {

            this.EffectMap = trEffectMap;
            this.Row = row;
            this.RowIndex = rowIndex;
            this.ColumnIndex = columnIndex;

        }//end ctor



        public TransitionEffectMapping EffectMap
        {
            get;
            private set;
        }//end TransitionEffectType EffectMap



        public int ColumnIndex
        {
            get;
            private set;
        }//end int ColumnIndex



        public GridRow Row
        {
            get;
            private set;
        }//end GridRow Row



        public int RowIndex
        {
            get;
            private set;
        }//end int RowIndex

    }//end class TransitionIconTappedEventArgs




    public class LayerThumbTappedEventArgs : EventArgs
    {

        public LayerThumbTappedEventArgs(AnimationLayerThumbItem thumbItem)
        {
            this.ThumbItem = thumbItem;
        }//end ctor



        public AnimationLayerThumbItem ThumbItem
        {
            get;
            private set;
        }//end AnimationLayerThumbItem ThumbItem

    }//end class LayerThumbItemEventArgs






    public class AudioThumbTappedEventArgs : EventArgs
    {

        public AudioThumbTappedEventArgs(AnimationAudioThumbItem thumbItem)
        {
            this.ThumbItem = thumbItem;
        }//end ctor



        public AnimationAudioThumbItem ThumbItem
        {
            get;
            private set;
        }//end AnimationAudioThumbItem ThumbItem

    }//end class AudioThumbTappedEventArgs





    public class RowRemoveTappedEventArgs : EventArgs
    {

        public RowRemoveTappedEventArgs(FrameLayerPair layerAddress, GridRow row, int rowIndex, int columnIndex, Pair<AnimationGridRowItemType, UIView> item)
        {

            this.LayerAddress = layerAddress;
            this.Row = row;
            this.RowIndex = rowIndex;
            this.ColumnIndex = columnIndex;
            this.Item = item;

        }//end class RowRemoveTappedEventArgs



        public FrameLayerPair LayerAddress
        {
            get;
            private set;
        }//end FrameLayerPair LayerAddress



        public GridRow Row
        {
            get;
            private set;
        }//end GridRow Row



        public int RowIndex
        {
            get;
            private set;
        }//end int RowIndex



        public int ColumnIndex
        {
            get;
            private set;
        }//end int ColumnIndex



        public Pair<AnimationGridRowItemType, UIView> Item
        {
            get;
            private set;
        }//end Pair<AnimationGridRowItemType, UIView> Item

    }//end class RowRemoveTappedEventArgs





    public class RowDraggedEventArgs : EventArgs
    {

        public RowDraggedEventArgs(DraggableRow<GridRow> draggableRow, bool started, bool ended)
        {

            this.DraggableRow = draggableRow;
            this.Started = started;
            this.Ended = ended;

        }//end ctor




        public DraggableRow<GridRow> DraggableRow
        {
            get;
            private set;
        }//end GridRow Row



        public bool Started
        {
            get;
            private set;
        }//end bool Started



        public bool Ended
        {
            get;
            private set;
        }//end bool Ended



    }//end class RowDraggedEventArgs





    public class RowRemovedEventArgs : EventArgs
    {

        public RowRemovedEventArgs(int rowIndex, GridRow row)
        {

            this.RowIndex = rowIndex;
            this.Row = row;

        }//end ctor



        public int RowIndex
        {
            get;
            private set;
        }//end int RowIndex



        public GridRow Row
        {
            get;
            private set;
        }//end GridRow Row

    }//end class RowRemovedEventArgs





    public class LocationManagerEventArgs : EventArgs
    {

        public LocationManagerEventArgs(LocationManagerEventType evType, EventArgs args)
        {

            this.EventType = evType;
            this.Args = args;

        }//end ctor



        public LocationManagerEventType EventType
        {
            get;
            private set;
        }//end LocationManagerEventType EventType




        public EventArgs Args
        {
            get;
            private set;
        }//end Args

    }//end class LocationManagerEventArgs





    public class ReverseGeocodeCompletedEventArgs : EventArgs
    {

        public ReverseGeocodeCompletedEventArgs(CLPlacemark[] placemarks, NSError error)
        {

            this.Placemarks = placemarks;
            this.Error = error;

        }//end ctor



        public CLPlacemark[] Placemarks
        {
            get;
            private set;
        }//end CLPlacemark[] Placemarks



        public NSError Error
        {
            get;
            private set;
        }//end NSError Error

    }//end class ReverseGeocodeCompletedEventArgs
*/
}

