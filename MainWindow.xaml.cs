using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ketch
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private SettingsModel _settings;
        private TrayIconManager _tray;
        private string _settingsPath;

        public System.Windows.Media.Brush BackgroundBrush
        {
            get => RootBorder.Background;
            set => RootBorder.Background = value;
        }

        public MainWindow()
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
                BackgroundBrush = Brushes.Transparent;
                AllowsTransparency = true;
            }
            else
            {
                BackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.BackgroundHex));
                AllowsTransparency = true;
            }

            Opacity = _settings.Opacity / 100.0;

            var screen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
            var dpi = VisualTreeHelper.GetDpi(this);
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

            var hourAlign = _settings.ShowSeconds ? HorizontalAlignment.Right : HorizontalAlignment.Right;
            var minuteAlign = _settings.ShowSeconds ? HorizontalAlignment.Left : HorizontalAlignment.Left;

            for (int i = 0; i < cols; i++)
            {
                var vb = new Viewbox { Stretch = Stretch.Uniform, StretchDirection = StretchDirection.Both };
                var tb = new TextBlock
                {
                    Text = string.Empty,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.FontHex)),
                    FontSize = 72,
                    FontFamily = GetClockFontFamily(),
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center
                };

                if (_settings.ShowSeconds)
                {
                    if (i == 0) tb.HorizontalAlignment = HorizontalAlignment.Right;
                    if (i == 1) tb.HorizontalAlignment = HorizontalAlignment.Center;
                    if (i == 2) tb.HorizontalAlignment = HorizontalAlignment.Left;
                    if (i == 3) tb.HorizontalAlignment = HorizontalAlignment.Center;
                    if (i == 4) tb.HorizontalAlignment = HorizontalAlignment.Left;
                }
                else
                {
                    if (i == 0) tb.HorizontalAlignment = HorizontalAlignment.Right;
                    if (i == 1) tb.HorizontalAlignment = HorizontalAlignment.Center;
                    if (i == 2) tb.HorizontalAlignment = HorizontalAlignment.Left;
                }

                vb.Child = tb;
                Grid.SetColumn(vb, i);
                ClockGrid.Children.Add(vb);
            }
        }

        private static FontFamily GetClockFontFamily()
        {
            try
            {
                return new FontFamily(new Uri("pack://application:,,,/"), "./Assets/#Poketch Clock Numeral");
            }
            catch
            {
                return new FontFamily("Segoe UI");
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                var window = new SettingsWindow(_settings);
                if (window.ShowDialog() == true)
                {
                    _settings = window.Result;
                    _settingsPath = _settings.Save(_settingsPath);
                    ApplySettings();
                }
            });
        }

        private void OpenAbout()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var about = new AboutWindow();
                about.Owner = this;
                about.ShowDialog();
            });
        }

        private void ExitApp()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _tray.Dispose();
                Application.Current.Shutdown();
            });
        }
    }
}
