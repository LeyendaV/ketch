using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
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
                // Log and show a friendly message, then exit
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

        public void ApplySettings()
        {
            if (_settings.Background == "Transparent")
            {
                BackgroundBrush = WMedia.Brushes.Transparent;
                AllowsTransparency = true;
            }
            else
            {
                BackgroundBrush = new WMedia.SolidColorBrush((WMedia.Color)WMedia.ColorConverter.ConvertFromString(_settings.BackgroundHex));
                AllowsTransparency = true;
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

            for (int i = 0; i < cols; i++)
            {
                ClockGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            for (int i = 0; i < cols; i++)
            {
                var vb = new Viewbox { Stretch = WMedia.Stretch.Uniform, StretchDirection = StretchDirection.Both };
                var tb = new TextBlock
                {
                    Text = string.Empty,
                    Foreground = new WMedia.SolidColorBrush((WMedia.Color)WMedia.ColorConverter.ConvertFromString(_settings.FontHex)),
                    FontSize = 72,
                    FontFamily = GetClockFontFamily(),
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = W.HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center
                };

                if (_settings.ShowSeconds)
                {
                    if (i == 0) tb.HorizontalAlignment = W.HorizontalAlignment.Right;
                    if (i == 1) tb.HorizontalAlignment = W.HorizontalAlignment.Center;
                    if (i == 2) tb.HorizontalAlignment = W.HorizontalAlignment.Left;
                    if (i == 3) tb.HorizontalAlignment = W.HorizontalAlignment.Center;
                    if (i == 4) tb.HorizontalAlignment = W.HorizontalAlignment.Left;
                }
                else
                {
                    if (i == 0) tb.HorizontalAlignment = W.HorizontalAlignment.Right;
                    if (i == 1) tb.HorizontalAlignment = W.HorizontalAlignment.Center;
                    if (i == 2) tb.HorizontalAlignment = W.HorizontalAlignment.Left;
                }

                vb.Child = tb;
                Grid.SetColumn(vb, i);
                ClockGrid.Children.Add(vb);
            }
        }

        private static WMedia.FontFamily GetClockFontFamily()
        {
            try
            {
                return new WMedia.FontFamily(new Uri("pack://application:,,,/"), "./Assets/#Poketch Clock Numeral");
            }
            catch
            {
                return new WMedia.FontFamily("Segoe UI");
            }
        }

        private void UpdateClock()
        {
            var now = DateTime.Now;
            string hour = _settings.TimeFormat == "12 hs." ? now.ToString("hh") : now.ToString("HH");
            string minute = now.ToString("mm");
            string second = now.ToString("ss");

            if (!_settings.ShowSeconds)
            {
                if (ClockGrid.Children.Count >= 3)
                {
                    ((TextBlock)((Viewbox)ClockGrid.Children[0]).Child).Text = hour;
                    ((TextBlock)((Viewbox)ClockGrid.Children[1]).Child).Text = ":";
                    ((TextBlock)((Viewbox)ClockGrid.Children[2]).Child).Text = minute;
                }
            }
            else
            {
                if (ClockGrid.Children.Count >= 5)
                {
                    ((TextBlock)((Viewbox)ClockGrid.Children[0]).Child).Text = hour;
                    ((TextBlock)((Viewbox)ClockGrid.Children[1]).Child).Text = ":";
                    ((TextBlock)((Viewbox)ClockGrid.Children[2]).Child).Text = minute;
                    ((TextBlock)((Viewbox)ClockGrid.Children[3]).Child).Text = ":";
                    ((TextBlock)((Viewbox)ClockGrid.Children[4]).Child).Text = second;
                }
            }
        }

        private void OpenSettings()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var window = new SettingsWindow(_settings);
                if (window.ShowDialog() == true)
                {
                    _settings = window.Result!;
                    _settingsPath = _settings.Save(_settingsPath);
                    ApplySettings();
                }
            });
        }

        private void OpenAbout()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var about = new AboutWindow();
                about.Owner = this;
                about.ShowDialog();
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
