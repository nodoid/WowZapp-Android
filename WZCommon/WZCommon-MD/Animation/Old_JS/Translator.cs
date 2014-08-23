using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace LOLMessageDelivery.Classes.LOLAnimation
{
	public static class Translator
	{
		public static string MakePlaybackCode (Animation Anim, int ScreenWidth, int ScreenHeight, RenderPlatformTypes RenderPlatformType, string JavaScript = "")
		{
			if (Anim.ScreenObjects.Count > 0) {
				float XPixelsOnePercent = ScreenWidth / 100;
				float YPixelsOnePercent = ScreenHeight / 100;
			 	
				CultureInfo ci = new CultureInfo ("en-US", true);
				
				StringBuilder RenderingCode = new StringBuilder ();
				StringBuilder ResourcesData = new StringBuilder ();
				StringBuilder AnimationCode = new StringBuilder ();
				
				RenderingCode.Append ("<!DOCTYPE html>").AppendLine ();
				RenderingCode.Append ("<html>").AppendLine ();
				RenderingCode.Append ("<head>").AppendLine ();
				RenderingCode.Append ("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">").AppendLine ();
				RenderingCode.Append ("<style> body { margin: 0; overflow: hidden; } </style>").AppendLine ();
				RenderingCode.Append ("<script type=\"text/javascript\">").AppendLine ();
				
				if (RenderPlatformType != RenderPlatformTypes.iOS && JavaScript == "")
					RenderingCode.Append (GetJavaScript ()).AppendLine ();
				else
					RenderingCode.Append (JavaScript).AppendLine ();
				
				RenderingCode.Append ("</script>").AppendLine ();
				RenderingCode.Append ("<script type=\"text/paperscript\" canvas=\"canvas\">").AppendLine ();
				
				AnimationCode.Append ("function onFrame(event) {").AppendLine ();
				
				int IdCounter = 0;
				
				foreach (ScreenObject Item in Anim.ScreenObjects) {
					if (Item != null) {
						Item.Size.X = Item.Size.X * XPixelsOnePercent;
						Item.Size.Y = Item.Size.Y * YPixelsOnePercent;
						Item.InitialPosition.X = (Item.InitialPosition.X * XPixelsOnePercent) + (Item.Size.X / 2);
						Item.InitialPosition.Y = (Item.InitialPosition.Y * YPixelsOnePercent) + (Item.Size.Y / 2);
					
						switch (Item.ObjectType) {
						case ScreenObjectTypes.FreehandPath:
						
							RenderingCode.Append ("var segments_").Append (IdCounter).Append (" = ").Append (TranslatePath (Item.DrawnPath, XPixelsOnePercent, YPixelsOnePercent)).Append (";").AppendLine ();
							RenderingCode.Append ("var obj_").Append (IdCounter).Append (" = new Path(segments_").Append (IdCounter).Append (");").AppendLine ();
							RenderingCode.Append ("var obj_").Append (IdCounter).Append ("_rotation = 0;").AppendLine ();
							RenderingCode.Append ("var obj_").Append (IdCounter).Append ("_scaling = 0;").AppendLine ();
							RenderingCode.Append ("obj_").Append (IdCounter).Append (".strokeColor = '").Append (Item.DrawnPathColor).Append ("';").AppendLine ();
							RenderingCode.Append ("obj_").Append (IdCounter).Append (".strokeWidth = ").AppendFormat (ci, "{0}", Item.DrawnPathStroke).Append (";").AppendLine ();
							RenderingCode.Append ("obj_").Append (IdCounter).Append (".position = new Point(").AppendFormat (ci, "{0}", Item.InitialPosition.X).Append (",").AppendFormat (ci, "{0}", Item.InitialPosition.Y).Append (");").AppendLine ();
						
							if (Item.AppearsAt == 0) {
								RenderingCode.Append ("obj_").Append (IdCounter).Append (".opacity = 1;").AppendLine ();
							} else {
								RenderingCode.Append ("obj_").Append (IdCounter).Append (".opacity = 0;").AppendLine ();
								AnimationCode.Append ("if( Math.round(event.time) == ").AppendFormat (ci, "{0}", Item.AppearsAt).Append (") {").AppendLine ();
								AnimationCode.Append ("obj_").Append (IdCounter).Append (".opacity = 1;").AppendLine ();
								AnimationCode.Append ("}").AppendLine ();
							}
						
							RenderingCode.Append ("project.activeLayer.insertChild(").Append (Item.Layer).Append (", ").Append ("obj_").Append (IdCounter).Append (");").AppendLine ();
						
							break;
						
						case ScreenObjectTypes.FreehandDrawing:
						
							RenderingCode.Append ("var obj_").Append (IdCounter).Append (" = new Group();").AppendLine ();
						
							for (int i = 0; i < Item.DrawnPaths.Count; i++) {
								RenderingCode.Append ("var segments_").Append (IdCounter).Append ("_path_").Append (i).Append (" = ").Append (TranslatePath (Item.DrawnPaths [i], XPixelsOnePercent, YPixelsOnePercent)).Append (";").AppendLine ();
								RenderingCode.Append ("var obj_").Append (IdCounter).Append ("_path_").Append (i).Append (" = new Path(segments_").Append (IdCounter).Append ("_path_").Append (i).Append (");").AppendLine ();
								RenderingCode.Append ("var obj_").Append (IdCounter).Append ("_rotation = 0;").AppendLine ();
								RenderingCode.Append ("var obj_").Append (IdCounter).Append ("_scaling = 0;").AppendLine ();
								RenderingCode.Append ("obj_").Append (IdCounter).Append ("_path_").Append (i).Append (".strokeColor = '").Append (Item.DrawnPathColors [i]).Append ("';").AppendLine ();
								RenderingCode.Append ("obj_").Append (IdCounter).Append ("_path_").Append (i).Append (".strokeWidth = ").AppendFormat (ci, "{0}", Item.DrawnPathStrokes [i]).Append (";").AppendLine ();
								RenderingCode.Append ("obj_").Append (IdCounter).Append (".addChild(").Append ("obj_").Append (IdCounter).Append ("_path_").Append (i).Append (");");
							}
						
							if (Item.AppearsAt == 0) {
								RenderingCode.Append ("obj_").Append (IdCounter).Append (".opacity = 1;").AppendLine ();
							} else {
								RenderingCode.Append ("obj_").Append (IdCounter).Append (".opacity = 0;").AppendLine ();
								AnimationCode.Append ("if( Math.round(event.time) == ").AppendFormat (ci, "{0}", Item.AppearsAt).Append (") {").AppendLine ();
								AnimationCode.Append ("obj_").Append (IdCounter).Append (".opacity = 1;").AppendLine ();
								AnimationCode.Append ("}").AppendLine ();
							}
						
							RenderingCode.Append ("obj_").Append (IdCounter).Append (".position = new Point(").AppendFormat (ci, "{0}", Item.InitialPosition.X).Append (",").AppendFormat (ci, "{0}", Item.InitialPosition.Y).Append (");").AppendLine ();
						
							RenderingCode.Append ("project.activeLayer.insertChild(").Append (Item.Layer).Append (", ").Append ("obj_").Append (IdCounter).Append (");").AppendLine ();
						
							break;
						
						// NEEDS TO BE ANALYZED PROPERLY PRE-IMPLEMENTATION !!!
						case ScreenObjectTypes.BrushPath:
						
						
						
							break;
						
						// NEEDS TO BE ANALYZED PROPERLY PRE-IMPLEMENTATION !!!
						case ScreenObjectTypes.BrushDrawing:
						
						
							break;
						
						case ScreenObjectTypes.Image:
						
							RenderingCode.Append ("var obj_").Append (IdCounter).Append (" = new Raster('img_").Append (IdCounter).Append ("');").AppendLine ();
							RenderingCode.Append ("var obj_").Append (IdCounter).Append ("_rotation = 0;").AppendLine ();
							RenderingCode.Append ("var obj_").Append (IdCounter).Append ("_scaling = 0;").AppendLine ();
							RenderingCode.Append ("obj_").Append (IdCounter).Append (".scale((").AppendFormat (ci, "{0}", Item.Size.X).Append (" / ").Append ("obj_").Append (IdCounter).Append (".bounds.width),")
							.Append ("(").AppendFormat (ci, "{0}", Item.Size.Y).Append (" / ").Append ("obj_").Append (IdCounter).Append (".bounds.height));").AppendLine ();
							RenderingCode.Append ("obj_").Append (IdCounter).Append (".position = new Point(").AppendFormat (ci, "{0}", Item.InitialPosition.X).Append (",").AppendFormat (ci, "{0}", Item.InitialPosition.Y).Append (");").AppendLine ();
						
						
							if (Item.AppearsAt == 0) {
								RenderingCode.Append ("obj_").Append (IdCounter).Append (".opacity = 1;").AppendLine ();
							} else {
								RenderingCode.Append ("obj_").Append (IdCounter).Append (".opacity = 0;").AppendLine ();
								AnimationCode.Append ("if( Math.round(event.time) == ").AppendFormat (ci, "{0}", Item.AppearsAt).Append (") {").AppendLine ();
								AnimationCode.Append ("obj_").Append (IdCounter).Append (".opacity = 1;").AppendLine ();
								AnimationCode.Append ("}").AppendLine ();
							}
						
							RenderingCode.Append ("project.activeLayer.insertChild(").Append (Item.Layer).Append (", ").Append ("obj_").Append (IdCounter).Append (");").AppendLine ();
						
							string Base64Data = System.Convert.ToBase64String (Item.ObjectByteData, 0, Item.ObjectByteData.Length);
							ResourcesData.Append ("<img id=\"img_").Append (IdCounter).Append ("\" style=\"display:none;\" src=\"data:image/png;base64,").Append (Base64Data).Append ("\" />").AppendLine ();
						
							break;
						}
					
						if (Item.Transitions.Count > 0) {
							foreach (Transition step in Item.Transitions) {
								if (step.TransitionType == TransitionTypes.Move) {
									step.StartPosition.X = (step.StartPosition.X * XPixelsOnePercent) + (Item.Size.X / 2);
									step.StartPosition.Y = (step.StartPosition.Y * YPixelsOnePercent) + (Item.Size.Y / 2);
									step.EndPosition.X = (step.EndPosition.X * XPixelsOnePercent) + (Item.Size.X / 2);
									step.EndPosition.Y = (step.EndPosition.Y * YPixelsOnePercent) + (Item.Size.Y / 2);
								
									float EndsAt = step.StartsAt + step.Duration;
									float TimePercentile = step.Duration / 100;
									float XPercentile = (0 - (step.StartPosition.X - step.EndPosition.X)) / 100;
									float YPercentile = (0 - (step.StartPosition.Y - step.EndPosition.Y)) / 100;
								
									/*
                                 * Movement
                                 * -------------------------------------------------------------------------------------------------------
                                 * position_x = start_position_x + time_passed_percent * x_percentile
                                 * position_y = start_position_y + time_passed_percent * y_percentile
                                 * -------------------------------------------------------------------------------------------------------
                                 */
								
									AnimationCode.Append ("if( event.time >= ").AppendFormat (ci, "{0}", step.StartsAt).Append (" && event.time <= ").AppendFormat (ci, "{0}", EndsAt).Append (") {").AppendLine ();
									AnimationCode.Append ("var time_passed = (event.time - ").AppendFormat (ci, "{0}", step.StartsAt).Append (") / ").AppendFormat (ci, "{0}", TimePercentile).Append (";").AppendLine ();
									AnimationCode.Append ("obj_").Append (IdCounter).Append (".position = new Point( ").AppendFormat (ci, "{0}", step.StartPosition.X).Append (" + (time_passed * ").AppendFormat (ci, "{0}", XPercentile)
									.Append ("),").AppendFormat (ci, "{0}", step.StartPosition.Y).Append (" + (time_passed * ").AppendFormat (ci, "{0}", YPercentile).Append ("));").AppendLine ();
									AnimationCode.Append ("}").AppendLine ();
								}
							
								if (step.TransitionType == TransitionTypes.Scale) {
									step.StartSize.X = step.StartSize.X * XPixelsOnePercent;
									step.StartSize.Y = step.StartSize.Y * YPixelsOnePercent;
									step.EndSize.X = step.EndSize.X * XPixelsOnePercent;
									step.EndSize.Y = step.EndSize.Y * YPixelsOnePercent;
								
									float EndsAt = step.StartsAt + step.Duration;
									float TimePercentile = step.Duration / 100;
									float XPercentile = (0 - (step.StartSize.X - step.EndSize.X)) / 100;
									float YPercentile = (0 - (step.StartSize.Y - step.EndSize.Y)) / 100;
								
									/*
                                 * Scaling
                                 * -------------------------------------------------------------------------------------------------------
                                 * ratio_x = (object.bounds.width + (time_passed_percent * x_percentile)) / object.bounds.width
                                 * ratio_y = (object.bounds.height + (time_passed_percent * y_percentile)) / object.bounds.height
                                 * -------------------------------------------------------------------------------------------------------
                                 */
								
									AnimationCode.Append ("if( event.time >= ").AppendFormat (ci, "{0}", step.StartsAt).Append (" && event.time <= ").AppendFormat (ci, "{0}", EndsAt).Append (") {").AppendLine ();
									AnimationCode.Append ("var time_passed = (event.time - ").AppendFormat (ci, "{0}", step.StartsAt).Append (") / ").AppendFormat (ci, "{0}", TimePercentile).Append (";").AppendLine ();
									//AnimationCode.Append("obj_").Append(IdCounter).Append(".scale(((").Append("obj_").Append(IdCounter).Append(".bounds.width  + (time_passed * ").AppendFormat(ci, "{0}", XPercentile).Append(")) / ").Append("obj_").Append(IdCounter).Append(".bounds.width),")
									//                                                     .Append("((").Append("obj_").Append(IdCounter).Append(".bounds.height + (time_passed * ").AppendFormat(ci, "{0}", YPercentile).Append(")) / ").Append("obj_").Append(IdCounter).Append(".bounds.height));").AppendLine();
									AnimationCode.Append ("obj_").Append (IdCounter).Append (".scale(((").AppendFormat (ci, "{0}", step.StartSize.X).Append (" + (time_passed * ").AppendFormat (ci, "{0}", XPercentile).Append (")) / ").Append ("obj_").Append (IdCounter).Append (".bounds.width),")
									.Append ("((").AppendFormat (ci, "{0}", step.StartSize.Y).Append (" + (time_passed * ").AppendFormat (ci, "{0}", YPercentile).Append (")) / ").Append ("obj_").Append (IdCounter).Append (".bounds.height));").AppendLine ();
									AnimationCode.Append ("}").AppendLine ();
								}
							
								if (step.TransitionType == TransitionTypes.Rotate) {
									float EndsAt = step.StartsAt + step.Duration;
									float TimePercentile = step.Duration / 100;
									float RotationPercentile = (step.RotationsNum * 360) / 100;
								
									AnimationCode.Append ("if( event.time >= ").AppendFormat (ci, "{0}", step.StartsAt).Append (" && event.time <= ").AppendFormat (ci, "{0}", EndsAt).Append (") {").AppendLine ();
									AnimationCode.Append ("var time_passed = (event.time - ").AppendFormat (ci, "{0}", step.StartsAt).Append (") / ").AppendFormat (ci, "{0}", TimePercentile).Append (";").AppendLine ();
									AnimationCode.Append ("obj_").Append (IdCounter).Append (".rotate( (time_passed * ").AppendFormat (ci, "{0}", RotationPercentile).Append (") - obj_").Append (IdCounter).Append ("_rotation );").AppendLine ();
									AnimationCode.Append ("obj_").Append (IdCounter).Append ("_rotation = time_passed * ").AppendFormat (ci, "{0}", RotationPercentile).Append (";").AppendLine ();
									AnimationCode.Append ("}").AppendLine ();
								}
							
								if (step.TransitionType == TransitionTypes.Fade) {
									float EndsAt = step.StartsAt + step.Duration;
									float TimePercentile = step.Duration / 100;
									float OpacityPercentile = step.EndOpacity / 100;
								
									AnimationCode.Append ("if( event.time >= ").AppendFormat (ci, "{0}", step.StartsAt).Append (" && event.time <= ").AppendFormat (ci, "{0}", EndsAt).Append (") {").AppendLine ();
									AnimationCode.Append ("var time_passed = (event.time - ").AppendFormat (ci, "{0}", step.StartsAt).Append (") / ").AppendFormat (ci, "{0}", TimePercentile).Append (";").AppendLine ();
									AnimationCode.Append ("obj_").Append (IdCounter).Append (".opacity = time_passed * ").AppendFormat (ci, "{0}", OpacityPercentile).Append (";").AppendLine ();
									AnimationCode.Append ("}").AppendLine ();
								}
							}
						}
					
						IdCounter++;
					}
				}
				
				AnimationCode.Append ("}").AppendLine ();
				
				RenderingCode.Append (AnimationCode.ToString ()).AppendLine ();
				RenderingCode.Append ("</script>").AppendLine ();
				RenderingCode.Append ("</head>").AppendLine ();
				RenderingCode.Append ("<body>").AppendLine ();
				RenderingCode.Append (ResourcesData.ToString ());
				RenderingCode.Append ("<canvas id=\"canvas\" width=\"").Append (ScreenWidth).Append ("\" height=\"").Append (ScreenHeight).Append ("\"></canvas>").AppendLine ();
				RenderingCode.Append ("</body>").AppendLine ();
				RenderingCode.Append ("</html>").AppendLine ();
				
				return RenderingCode.ToString ();
			} else {
				// Y U NO Add ScreenObjects to your Animation ??
				return "ERROR: Nothing to animate...";
			}
		}
		
		public static string TranslatePath (List<PointF> path, float XPixels, float YPixels)
		{
			StringBuilder segments = new StringBuilder ();
			segments.Append ("[");
			
			for (int i = 0; i < path.Count; i++) {
				CultureInfo ci = new CultureInfo ("en-US", true);
				
				segments.Append ("new Point(").AppendFormat (ci, "{0}", path [i].X * XPixels).Append (",").AppendFormat (ci, "{0}", path [i].Y * YPixels).Append (")");
				
				if (i != (path.Count - 1)) {
					segments.Append (",");
				}
			}
			
			segments.Append ("]");
			
			return segments.ToString ();
		}
		
		public static string GetJavaScript ()
		{
			Assembly ass;
			StreamReader sr;
			try {
				ass = Assembly.GetExecutingAssembly ();
				sr = new StreamReader (ass.GetManifestResourceStream ("LOLAnimation.JavaScript.js"));
				return sr.ReadToEnd ();
			} catch {
				// error occured, do nothing and feel bad.
				return "";
			}
		}
	}
	
	public enum RenderPlatformTypes
	{
		iOS = 1,
		Android,
		Windows,
		OSX,
		XBox
	}
}
