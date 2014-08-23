// Created by Dimitris Tavlikos, dimitris@tavlikos.com, http://software.tavlikos.com
using System;
namespace LOLApp_Common
{

	public enum MediaPickType
	{
		StillImage,
		Video
	}//end enum MediaPickType




	public enum UserType
	{
		None,
		NewUser,
		ExistingUser
	}//end enum UserType





	public enum AlertType
	{
		None,
		FirstNameEmpty,
		LastNameEmpty,
		EmailAddressEmpty,
		EmailAddressInvalid,
		PasswordEmpty,
		AccountExists,
		BothFieldsEmpty,
		PasswordMismatch,
		//
		// Common communication error
		//
		CommunicationError,
		//
		// User did not allow LOL access to social network
		//
		UserCancelledAuth,
		//
		// CheckForAccessToken returned false
		//
		AccessTokenProblem,
		BlockContact,
		DeleteContact,
		FirstAndLastNameTwoChars,
		NoResults,
		EmptyField,
		NothingSelected,
		ReplyScope,
		SenderNotContact,
		NoPollQuestion,
		NumberOfPhotoPollImages,
		NumberOfPollOptions,
		ContactAlreadyAdded,
		MessageNotSent,
		//
		// Animations
		//
		ConfirmImageCrop,
		ApplyLayerChangesForTransition,
		AnimationError,
		CantUndo,
		//
		// Video
		//
		NoVideoStored
	}//end enum AlertType





	public enum ConnectServiceErrors
	{
		Unknown = -1,
		None = 0,
		AuthenticationTokenInvalid = 1,
		DeviceIDMissing = 2,
		PasswordsDontMatch = 3,
		AccountInformationInvalid = 4,
		DuplicateEmailAddress = 6,
		AccountIDNull = 7,
		AccountIdDoesNotMatchDatabase = 8,
		DeviceSaveFailed = 15,
		AuthenticationTokenDoesNotMatchAccountID = 28,
		AuthenticationTokenNotFound = 29,
		AuthenticationTokenLastUsedDateNotSet = 30,
		AuthenticationTokenExpired = 31,
		AuthenticationTokenNotLoggedIn = 32,
		AuthenticationTokenLoadFromDatarowFailed = 33,
		AuthenticationTokenLoadFailed = 34,
		AuthenticationTokenDeleteFailed = 35,
		AuthenticationTokenSaveFailed = 36,
		InvalidUserInformation = 52,
		NullEmailAddress = 53,
		ResetPasswordFailed = 54,
		UnableToResetPassword = 54,
		InvalidResetCode = 55,
		ResetCodeExpired = 59,
		ResetPasswordFailed_NoMatchingEmailAddress = 61,
		InvalidEmailAddress = 62,
		DeviceNotRegisteredWithAnAccount = 63,
		UnknownUserContactRequested = 76,
		DuplicateContact = 77,
		AccountIDInvalid = 78,
		OAuthTokenInvalid = 79,
		OAuthIDInvalid = 80,
		UserGetSpecificFailed = 81,
		AuthenticationTokenLoggedOut = 82,
		Unexpected = 100,
		CouldNotIdentifyRow = 101,
		ContentPackIdMissing = 102,
		ContentPackItemNotFound = 103,
		ContentPackItemIdMissing = 104,
		SearchCriteriaMissing = 105,
		ContactAccountIDMissing = 106,
		ItemSizeNone = 107,
		ContactIDEmpty = 108,
		ContactNotFound = 109,
		MessageHasNoSteps = 110,
		MessageHasNoAddressee = 111
	}


    


	public enum MessageEditType
	{
		Create,
		Preview,
		Read
	}//end enum MessageEditType



	public enum PollingScreenType
	{
		Preview,
		Vote,
		Results
	}//end enum PollingScreenType



	public enum PollKind
	{
		TextPoll,
		PhotoPoll
	}//end enum PollKind





	public enum BubbleOrientation
	{
		Left,
		Right
	}//end enum BubbleOrientation




	public enum PlaybackButtonStatus
	{

		Play = 10,
		Pause,
		Stop

	}//end enum PlaybackButtonStatus



	public enum MenuType
	{
		ColorPicker,
		BrushPicker,
		AnimationContent,
		AnimationItemType
	}//end enum MenuType



	public enum BrushType
	{

		Normal,
		Spray

	}//end enum BrushType



	public enum DrawingLayerType
	{
		Drawing,
		Image,
		Callout,
		Stamp,
		Comix
	}//end enum DrawingLayerType



	public enum LayerInfoType
	{

		Graphics,
		Audio

	}//end enum LayerInfoType



	public enum ImageManipulationMode
	{

		ScaleMove,
		Crop

	}//end enum ImageManipulationMode



	public enum ImageManipulationActionMode
	{
		Idle,
		Scaling,
		Moving,
		Cropping

	}//end enum ImageManipulationActionMode




	public enum GraphicsManipulationMode
	{
		ScaleMove,
		Rotate,
		Crop,
		Text
	}//end enum GraphicsManipulationMode



	public enum GraphicsManipulationActionMode
	{

		Idle,
		Scaling,
		Moving,
		Rotating,
		Cropping

	}//end enum GraphicsManipulationActionMode





	public enum CanvasControlPoint
	{
		/// <summary>
		/// Point is outside graphics object
		/// </summary>
		None,
		/// <summary>
		/// Point is inside graphics object, but not in any of the corner control points.
		/// </summary>
		ObjectBox,
		/// <summary>
		/// Point is inside the top-left control point
		/// </summary>
		TopLeft,
		/// <summary>
		/// Point is inside the top-right control point
		/// </summary>
		TopRight,
		/// <summary>
		/// Point is inside the bottom-left control point
		/// </summary>
		BottomLeft,
		/// <summary>
		/// Point is inside the bottom-right control point
		/// </summary>
		BottomRight
	}//end enum CanvasControlPoint



	public enum AnimationMenuSize
	{
		Compact,
		Full
	}//end enum AnimationMenuSize



	public enum AnimationItemType
	{

		Callout,
		Transition,
		Stamp

	}//end enum AnimationItemType
	


	public enum AnimationGridRowItemType
	{

		LayerThumb,
		TransitionIcon,
		AddButton,
		RemoveButton,
		AudioThumb

	}//end enum AnimationGridRowItemType



	public enum LayerHandlerMode
	{

		NewLayer,
		EditLayer

	}//end enum LayerHandlerMode



	public enum TransitionScreenType
	{
		List,
		Settings
	}//end enum TransitionScreenType




	public enum LocationManagerEventType
	{
		
		AuthorizationChanged,
		Failed,
		UpdatedHeading,
		UpdatedLocation,
		LocationsUpdated,
		LocationUpdatePaused,
		LocationUpdateResumed,
		ReverseGeocodeCompleted
		
	}//end enum LocationManagerEventType
	
	public enum PersonDescriptors
	{
		Unknown,
		Male,
		Female,
		Alien,
		Monster
	}
}

