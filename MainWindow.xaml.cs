using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Globalization;
using WF = System.Windows.Forms;
using WMedia = System.Windows.Media;
using W = System.Windows;

namespace Ketch
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private SettingsModel _settings;
        private TrayIconManager _tray;
        private string _settingsPath;

        public WMedia.Brush BackgroundBrush
        {
            get => RootBorder.Background;
            set => RootBorder.Background = value;
        }

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                var loaded = SettingsModel.LoadWithAutoPath();
                _settings = loaded.settings;
                _settingsPath = loaded.path;

                AllowsTransparency = true;

                var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "ketch_icon.ico");
                _tray = new TrayIconManager(OpenSettings, OpenAbout, ExitApp, iconPath);
                _tray.Initialize();

                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(250);
                _timer.Tick += (_, _) => UpdateClock();
                _timer.Start();
            }
            catch (Exception ex)
            {
                try
                {
                    var logPath = Path.Combine(AppContext.BaseDirectory, "ketch_error.log");
                    File.AppendAllText(logPath, ex.ToString());
                }
                catch { }

                System.Windows.MessageBox.Show("ketch failed to start. Please check ketch_error.log next to the executable or in %LOCALAPPDATA%\\ketch.", "ketch - Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MakeWindowClickThrough();
            ApplySettings();
            BuildClockGrid();
            UpdateClock();
        }

        private void MakeWindowClickThrough()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
            exStyle |= NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_TOOLWINDOW;
            NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE, exStyle);
        }

        public void ApplyNewSettings(SettingsModel s)
        {
            _settings = s;
            _settingsPath = _settings.Save(_settingsPath);
            ApplySettings();
            BuildClockGrid();
            UpdateClock();
        }

        public void ApplySettings()
        {
            if (_settings.Background == "Transparent")
            {
                BackgroundBrush = WMedia.Brushes.Transparent;
            }
            else
            {
                BackgroundBrush = new WMedia.SolidColorBrush((WMedia.Color)WMedia.ColorConverter.ConvertFromString(_settings.BackgroundHex));
            }

            Opacity = _settings.Opacity / 100.0;

            var screen = WF.Screen.PrimaryScreen ?? WF.Screen.AllScreens[0];
            var dpi = WMedia.VisualTreeHelper.GetDpi(this);
            var displayHeight = screen.Bounds.Height;

            double h = displayHeight * (_settings.Size switch
            {
                "Huge" => 0.50,
                "Big" => 0.35,
                "Medium" => 0.20,
                _ => 0.15
            });

            var heightDip = h / (dpi.PixelsPerInchY / 96.0);
            Width = heightDip;
            Height = heightDip;

            var margin = 50;
            var work = screen.WorkingArea;
            double left = 0;
            double top = 0;

            switch (_settings.DisplayLocation)
            {
                case "Bottom Right":
                    left = work.Right - Width - margin;
                    top = work.Bottom - Height - margin;
                    break;
                case "Bottom Left":
                    left = work.Left + margin;
                    top = work.Bottom - Height - margin;
                    break;
                case "Top Left":
                    left = work.Left + margin;
                    top = work.Top + margin;
                    break;
                case "Top Right":
                    left = work.Right - Width - margin;
                    top = work.Top + margin;
                    break;
            }

            Left = left;
            Top = top;

            BuildClockGrid();
        }

        private void BuildClockGrid()
        {
            ClockGrid.Children.Clear();
            ClockGrid.ColumnDefinitions.Clear();

            int cols = _settings.ShowSeconds ? 5 : 3;

            // Load font and choose a large base size for measurement. The outer Viewbox will scale uniformly to fit.
            var fontFamily = TryLoadFontFamilyFromAssets() ?? new WMedia.FontFamily("Segoe UI");
            double baseFontSize = 400;

            // Measure widest glyph among digits and colon so each column uses the same fixed width.
            double maxGlyphWidth = 0;
            var toMeasure = new[] { "0","1","2","3","4","5","6","7","8","9",":" };
            foreach (var ch in toMeasure)
            {
                var meas = new TextBlock
                {
                    Text = ch,
                    FontFamily = fontFamily,
                    FontSize = baseFontSize,
                    FontStretch = FontStretches.Normal,
                    FontStyle = FontStyles.Normal,
                    FontWeight = FontWeights.Normal
                };
                meas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if (meas.DesiredSize.Width > maxGlyphWidth) maxGlyphWidth = meas.DesiredSize.Width;
            }

            // Create inner grid with fixed pixel column widths (equal for every column).
            var inner = new Grid();
            for (int i = 0; i < cols; i++)
            {
                inner.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(maxGlyphWidth, GridUnitType.Pixel) });
            }

            for (int i = 0; i < cols; i++)
            {
                var tb = new TextBlock
                {
                    Text = string.Empty,
                    Foreground = new WMedia.SolidColorBrush((WMedia.Color)WMedia.ColorConverter.ConvertFromString(_settings.FontHex)),
                    FontSize = baseFontSize,
                    FontFamily = fontFamily,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = W.HorizontalAlignment.Center,
                    Width = maxGlyphWidth
                };

                // Preserve alignment intent per column
                if (_settings.ShowSeconds)
                {
                    if (i == 0) { tb.TextAlignment = TextAlignment.Right; tb.HorizontalAlignment = W.HorizontalAlignment.Right; }
                    if (i == 1) { tb.TextAlignment = TextAlignment.Center; tb.HorizontalAlignment = W.HorizontalAlignment.Center; }
                    if (i == 2) { tb.TextAlignment = TextAlignment.Left; tb.HorizontalAlignment = W.HorizontalAlignment.Left; }
                    if (i == 3) { tb.TextAlignment = TextAlignment.Center; tb.HorizontalAlignment = W.HorizontalAlignment.Center; }
                    if (i == 4) { tb.TextAlignment = TextAlignment.Left; tb.HorizontalAlignment = W.HorizontalAlignment.Left; }
                }
                else
                {
                    if (i == 0) { tb.TextAlignment = TextAlignment.Right; tb.HorizontalAlignment = W.HorizontalAlignment.Right; }
                    if (i == 1) { tb.TextAlignment = TextAlignment.Center; tb.HorizontalAlignment = W.HorizontalAlignment.Center; }
                    if (i == 2) { tb.TextAlignment = TextAlignment.Left; tb.HorizontalAlignment = W.HorizontalAlignment.Left; }
                }

                Grid.SetColumn(tb, i);
                inner.Children.Add(tb);
            }

            var outer = new Viewbox { Child = inner, Stretch = WMedia.Stretch.Uniform };
            ClockGrid.Children.Add(outer);
        }

        private WMedia.FontFamily? TryLoadFontFamilyFromAssets()
        {
            // 1) Try to load directly from the font file in the output Assets folder (site of origin). This does not require installing the font.
            try
            {
                var fontFile = Path.Combine(AppContext.BaseDirectory, "Assets", "ketch_font.ttf");
                if (File.Exists(fontFile))
                {
                    // Use GlyphTypeface to read the internal family name from the TTF
                    var glyph = new WMedia.GlyphTypeface(new Uri(fontFile));

                    // Pick a suitable family name (prefer current UI culture, then en-US, then first)
                    string familyName = null!;
                    if (glyph.FamilyNames.TryGetValue(CultureInfo.CurrentUICulture, out var name)) familyName = name;
                    else if (glyph.FamilyNames.TryGetValue(new CultureInfo("en-US"), out name)) familyName = name;
                    else familyName = glyph.FamilyNames.Values.FirstOrDefault() ?? "Poketch Clock Numeral";

                    // Create a FontFamily that points to the local font file directory and the internal family name
                    var folder = Path.GetDirectoryName(fontFile)!;
                    var folderUri = new Uri(folder.EndsWith("/") || folder.EndsWith("\\") ? folder : folder + Path.DirectorySeparatorChar);

                    try
                    {
                        return new WMedia.FontFamily(folderUri, $"./#{familyName}");
                    }
                    catch { }

                    // Last resort: try siteoforigin pack URI with the discovered family name
                    try
                    {
                        return new WMedia.FontFamily(new Uri("pack://siteoforigin:,,,/"), $"./Assets/#{familyName}");
                    }
                    catch { }
                }
            }
            catch { }

            // 2) Try to enumerate fonts from site-of-origin Assets folder
            try
            {
                var siteUri = new Uri("pack://siteoforigin:,,,/Assets/");
                var families = WMedia.Fonts.GetFontFamilies(siteUri, "./");
                foreach (var f in families)
                {
                    if (f.Source.IndexOf("Poketch", StringComparison.OrdinalIgnoreCase) >= 0 || f.Source.IndexOf("ketch", StringComparison.OrdinalIgnoreCase) >= 0)
                        return f;
                }
            }
            catch { }

            // 3) Try application resources (embedded resource)
            try
            {
                var appUri = new Uri("pack://application:,,,/Assets/");
                var families = WMedia.Fonts.GetFontFamilies(appUri, "./");
                foreach (var f in families)
                {
                    if (f.Source.IndexOf("Poketch", StringComparison.OrdinalIgnoreCase) >= 0 || f.Source.IndexOf("ketch", StringComparison.OrdinalIgnoreCase) >= 0)
                        return f;
                }
            }
            catch { }

            // Not found
            return null;
        }

        private void UpdateClock()
        {
            var now = DateTime.Now;
            string hour = _settings.TimeFormat == "12 hs." ? now.ToString("hh") : now.ToString("HH");
            string minute = now.ToString("mm");
            string second = now.ToString("ss");

            if (ClockGrid.Children.Count == 0) return;

            var inner = (Grid)((Viewbox)ClockGrid.Children[0]).Child;
            if (!_settings.ShowSeconds)
            {
                if (inner.Children.Count >= 3)
                {
                    ((TextBlock)inner.Children[0]).Text = hour;
                    ((TextBlock)inner.Children[1]).Text = ":";
                    ((TextBlock)inner.Children[2]).Text = minute;
                }
            }
            else
            {
                if (inner.Children.Count >= 5)
                {
                    ((TextBlock)inner.Children[0]).Text = hour;
                    ((TextBlock)inner.Children[1]).Text = ":";
                    ((TextBlock)inner.Children[2]).Text = minute;
                    ((TextBlock)inner.Children[3]).Text = ":";
                    ((TextBlock)inner.Children[4]).Text = second;
                }
            }
        }

        private WMedia.FontFamily? TryLoadFontFamilyFromAssets()
        {
            // 1) Try to load directly from the font file in the output Assets folder (site of origin). This does not require installing the font.
            try
            {
                var fontFile = Path.Combine(AppContext.BaseDirectory, "Assets", "ketch_font.ttf");
                if (File.Exists(fontFile))
                {
                    // Use GlyphTypeface to read the internal family name from the TTF
                    var glyph = new WMedia.GlyphTypeface(new Uri(fontFile));

                    // Pick a suitable family name (prefer current UI culture, then en-US, then first)
                    string familyName = null!;
                    if (glyph.FamilyNames.TryGetValue(CultureInfo.CurrentUICulture, out var name)) familyName = name;
                    else if (glyph.FamilyNames.TryGetValue(new CultureInfo("en-US"), out name)) familyName = name;
                    else familyName = glyph.FamilyNames.Values.FirstOrDefault() ?? "Poketch Clock Numeral";

                    // Create a FontFamily that points to the local font file directory and the internal family name
                    var folder = Path.GetDirectoryName(fontFile)!;
                    var folderUri = new Uri(folder.EndsWith("/") || folder.EndsWith("\\") ? folder : folder + Path.DirectorySeparatorChar);

                    try
                    {
                        return new WMedia.FontFamily(folderUri, $"./#{familyName}");
                    }
                    catch { }

                    // Last resort: try siteoforigin pack URI with the discovered family name
                    try
                    {
                        return new WMedia.FontFamily(new Uri("pack://siteoforigin:,,,/"), $"./Assets/#{familyName}");
                    }
                    catch { }
                }
            }
            catch { }

            // 2) Try to enumerate fonts from site-of-origin Assets folder
            try
            {
                var siteUri = new Uri("pack://siteoforigin:,,,/Assets/");
                var families = WMedia.Fonts.GetFontFamilies(siteUri, "./");
                foreach (var f in families)
                {
                    if (f.Source.IndexOf("Poketch", StringComparison.OrdinalIgnoreCase) >= 0 || f.Source.IndexOf("ketch", StringComparison.OrdinalIgnoreCase) >= 0)
                        return f;
                }
            }
            catch { }

            // 3) Try application resources (embedded resource)
            try
            {
                var appUri = new Uri("pack://application:,,,/Assets/");
                var families = WMedia.Fonts.GetFontFamilies(appUri, "./");
                foreach (var f in families)
                {
                    if (f.Source.IndexOf("Poketch", StringComparison.OrdinalIgnoreCase) >= 0 || f.Source.IndexOf("ketch", StringComparison.OrdinalIgnoreCase) >= 0)
                        return f;
                }
            }
            catch { }

            // Not found
            return null;
        }

        private void OpenSettings()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var window = new SettingsWindow(_settings);
                // Do not set Owner so the Settings window centers on the screen instead of following this window
                window.Show();
            });
        }

        private void OpenAbout()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var about = new AboutWindow();
                about.Owner = this;
                about.Show();
            });
        }

        private void ExitApp()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _tray.Dispose();
                System.Windows.Application.Current.Shutdown();
            });
        }
    }
}
