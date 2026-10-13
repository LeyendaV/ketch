--- a/MainWindow.xaml.cs
+++ b/MainWindow.xaml.cs
@@
         private static WMedia.FontFamily GetClockFontFamily()
         {
             try
             {
-                return new WMedia.FontFamily(new Uri("pack://application:,,,/"), "./Assets/#Poketch Clock Numeral");
+                // Use site-of-origin pack URI so the font bundled as Content (copied to output) is discoverable at runtime.
+                return new WMedia.FontFamily(new Uri("pack://siteoforigin:,,,/"), "./Assets/#Poketch Clock Numeral");
             }
             catch
             {
                 return new WMedia.FontFamily("Segoe UI");
             }
         }
