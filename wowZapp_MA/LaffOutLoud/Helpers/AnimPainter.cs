using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Reflection;
using System.IO;

namespace wowZapp.Helpers
{
	public static class AnimPainter
	{
		public static string MakeMeAPainter (int ScreenWidth, int ScreenHeight)
		{
			StringBuilder RenderingCode = new StringBuilder ();
			RenderingCode.Append ("<!DOCTYPE html>").AppendLine ();
			RenderingCode.Append ("<html>").AppendLine ();
			RenderingCode.Append ("<head>").AppendLine ();
			RenderingCode.Append ("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">").AppendLine ();
			RenderingCode.Append ("<style> body { margin: 0; overflow: hidden; } </style>").AppendLine ();
			RenderingCode.Append ("<script type=\"text/javascript\">").AppendLine ();
			RenderingCode.Append (GetJavaScript ()).AppendLine ();
			RenderingCode.Append ("</script>").AppendLine ();
            RenderingCode.Append ("<script type=\"text/javascript\">").AppendLine ();
            RenderingCode.Append (GetPainterJavaScriptProxy()).AppendLine();
            RenderingCode.Append ("</script>").AppendLine ();
			RenderingCode.Append ("<script type=\"text/paperscript\" canvas=\"canvas\">").AppendLine ();
			RenderingCode.Append (GetPainterJavaScript ()).AppendLine ();
			RenderingCode.Append ("</script>").AppendLine ();
			RenderingCode.Append ("</head>").AppendLine ();
			RenderingCode.Append ("<body>").AppendLine ();
			RenderingCode.Append ("<canvas id=\"canvas\" width=\"").Append (ScreenWidth).Append ("\" height=\"").Append (ScreenHeight).Append ("\"></canvas>").AppendLine ();
			RenderingCode.Append ("<img id=\"brush_1").Append ("\" style=\"display:none;\" src=\"data:image/png;base64,").Append (System.Convert.ToBase64String (GrabMyBrushAsByte ("brush_1.png"))).Append ("\" />").AppendLine ();
			RenderingCode.Append ("<img id=\"brush_2").Append ("\" style=\"display:none;\" src=\"data:image/png;base64,").Append (System.Convert.ToBase64String (GrabMyBrushAsByte ("brush_2.png"))).Append ("\" />").AppendLine ();
			RenderingCode.Append ("<img id=\"brush_3").Append ("\" style=\"display:none;\" src=\"data:image/png;base64,").Append (System.Convert.ToBase64String (GrabMyBrushAsByte ("brush_3.png"))).Append ("\" />").AppendLine ();
			RenderingCode.Append ("<img id=\"brush_4").Append ("\" style=\"display:none;\" src=\"data:image/png;base64,").Append (System.Convert.ToBase64String (GrabMyBrushAsByte ("brush_4.png"))).Append ("\" />").AppendLine ();
			RenderingCode.Append ("<img id=\"brush_5").Append ("\" style=\"display:none;\" src=\"data:image/png;base64,").Append (System.Convert.ToBase64String (GrabMyBrushAsByte ("brush_5.png"))).Append ("\" />").AppendLine ();
			RenderingCode.Append ("</body>").AppendLine ();
			RenderingCode.Append ("</html>").AppendLine ();
            
			#if DEBUG
			string filename = Path.Combine (wowZapp.LaffOutOut.Singleton.ContentDirectory, "makemeapainter.html");
			using (TextWriter d = File.CreateText(filename)) {
				d.Write (RenderingCode.ToString ());
			}
			if (!System.IO.Directory.Exists (Android.OS.Environment.ExternalStorageDirectory + "/wz"))
				System.IO.Directory.CreateDirectory (Android.OS.Environment.ExternalStorageDirectory + "/wz");
			File.Copy (filename, 
				Android.OS.Environment.ExternalStorageDirectory + "/wz/makemeapainter.html", true);
			#endif
            
            
			return RenderingCode.ToString ();
		}

        public static string MakeMeAFreehandPainter(int ScreenWidth, int ScreenHeight)
        {
            StringBuilder RenderingCode = new StringBuilder();
            RenderingCode.Append("<!DOCTYPE html>").AppendLine();
            RenderingCode.Append("<html>").AppendLine();
            RenderingCode.Append("<head>").AppendLine();
            RenderingCode.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">").AppendLine();
            RenderingCode.Append("<style> body { margin: 0; overflow: hidden; } </style>").AppendLine();
            RenderingCode.Append("<script type=\"text/javascript\">").AppendLine();
            RenderingCode.Append(GetJavaScript()).AppendLine();
            RenderingCode.Append("</script>").AppendLine();
            RenderingCode.Append("<script type=\"text/javascript\">").AppendLine();
            RenderingCode.Append(GetPainterJavaScriptProxy()).AppendLine();
            RenderingCode.Append("</script>").AppendLine();
            RenderingCode.Append("<script type=\"text/paperscript\" canvas=\"canvas\">").AppendLine();
            RenderingCode.Append(GetFreehandJavaScript()).AppendLine();
            RenderingCode.Append("</script>").AppendLine();
            RenderingCode.Append("</head>").AppendLine();
            RenderingCode.Append("<body>").AppendLine();
            RenderingCode.Append("<canvas id=\"canvas\" width=\"").Append(ScreenWidth).Append("\" height=\"").Append(ScreenHeight).Append("\"></canvas>").AppendLine();
            RenderingCode.Append("</body>").AppendLine();
            RenderingCode.Append("</html>").AppendLine();

            return RenderingCode.ToString();
        }

		public static string GetJavaScript ()
		{
			Assembly ass;
			StreamReader sr;
			try {
				ass = Assembly.GetExecutingAssembly ();
				sr = new StreamReader (ass.GetManifestResourceStream ("wowZapp.animlib.JavaScript.js"));
				return sr.ReadToEnd ();
			} catch {
				// error occured, do nothing and feel bad.
				return "";
			}
		}

		public static string GetPainterJavaScript ()
		{
			Assembly ass;
			StreamReader sr;
			try {
				ass = Assembly.GetExecutingAssembly ();
				sr = new StreamReader (ass.GetManifestResourceStream ("wowZapp.Helpers.JavaScriptBrushDrawing.js"));
				return sr.ReadToEnd ();
			} catch {
				// error occured, do nothing and feel bad.
				return "";
			}
		}

        public static string GetFreehandJavaScript()
        {
            Assembly ass;
            StreamReader sr;
            try
            {
                ass = Assembly.GetExecutingAssembly();
                sr = new StreamReader(ass.GetManifestResourceStream("wowZapp.Helpers.JavaScriptFreehandDrawing.js"));
                return sr.ReadToEnd();
            }
            catch
            {
                // error occured, do nothing and feel bad.
                return "";
            }
        }

        public static string GetPainterJavaScriptProxy()
        {
            Assembly ass;
            StreamReader sr;
            try
            {
                ass = Assembly.GetExecutingAssembly();
                sr = new StreamReader(ass.GetManifestResourceStream("wowZapp.Helpers.JavaScriptDrawingProxy.js"));
                return sr.ReadToEnd();
            }
            catch
            {
                // error occured, do nothing and feel bad.
                return "";
            }
        }

		public static byte[] GrabMyBrushAsByte (string brushname)
		{
			using (MemoryStream memBrush = new MemoryStream()) {
				Stream brush = Application.Context.Assets.Open (brushname);
				brush.CopyTo (memBrush);
				brush.Dispose ();
				return memBrush.ToArray ();
			}
		}
	}

	class AnimPainterEvents : Java.Lang.Object
	{
		Context context;

		public AnimPainterEvents (Context context)
		{
			this.context = context;
		}

		public void BrushDrawingFinished (string DrawingData, string BrushSelection, string X_position, string Y_position, string X_size, string Y_size)
		{
			//this will be called with all the final data after you call javascript:WrapItUp(); on a WebView with JavaScriptBrushDrawing.js loaded
		}

        public void FreehandDrawingFinished (string DrawingData, string StrokeWidthData, string PathColorData, string X_position, string Y_position, string X_size, string Y_size)
        {
            //this will be called with all the final data after you call javascript:WrapItUp(); on a WebView with JavaScriptFreehandDrawing.js loaded
        }

        public void ImageDrawingFinished(string ImageBase64Data, string X_position, string Y_position, string X_size, string Y_size)
        {
            //this will be called with all the final data after you call javascript:WrapItUp(); on a WebView with JavaScriptImageDrawing.js loaded
        }

        public void CalloutDrawingFinished(string CalloutBase64Data, string TextData, string X_position, string Y_position, string X_size, string Y_size)
        {
            //this will be called with all the final data after you call javascript:WrapItUp(); on a WebView with JavaScriptCalloutDrawing.js loaded
        }
	}
}