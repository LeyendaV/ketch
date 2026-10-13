using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Ketch
{
    public partial class SettingsWindow : Window
    {
        public SettingsModel? Result { get; private set; }
        private readonly SettingsModel _working;
        private readonly List<(string name, string hex)> _backgrounds;
        private readonly List<(string name, string hex)> _fontColors;

        public SettingsWindow(SettingsModel current)
        {
            InitializeComponent();

            _working = new SettingsModel
            {
                Background = current.Background,
                BackgroundHex = current.BackgroundHex,
                Font = current.Font,
                FontHex = current.FontHex,
                Opacity = current.Opacity,
                DisplayLocation = current.DisplayLocation,
                Size = current.Size,
                TimeFormat = current.TimeFormat,
                ShowSeconds = current.ShowSeconds
            };

            _backgrounds = new List<(string name, string hex)>
            {
                ("Green", "#70B070"),
                ("Yellow", "#B8B070"),
                ("Orange", "#C09068"),
                ("Red", "#D87070"),
                ("Purple", "#A070B0"),
                ("Blue", "#8888F8"),
                ("Teal", "#58B8C0"),
                ("Grey", "#A0A0A0"),
                ("Transparent", "#00000000")
            };

            cbBackground.ItemsSource = _backgrounds.Select(b => b.name).ToList();
            cbBackground.SelectedIndex = Math.Max(0, _backgrounds.FindIndex(item => item.name == _working.Background));

            _fontColors = new List<(string name, string hex)>
            {
                ("Green", "#385030"),
                ("Yellow", "#485030"),
                ("Orange", "#504028"),
                ("Red", "#602020"),
                ("Purple", "#483050"),
                ("Blue", "#383868"),
                ("Teal", "#105860"),
                ("Grey", "#505050")
            };

            cbFont.ItemsSource = _fontColors.Select(f => f.name).ToList();
            cbFont.SelectedIndex = Math.Max(0, _fontColors.FindIndex(item => item.name == _working.Font));

            sliderOpacity.Value = _working.Opacity;
            tbOpacityValue.Text = _working.Opacity.ToString();
            sliderOpacity.ValueChanged += (s, e) => tbOpacityValue.Text = ((int)sliderOpacity.Value).ToString();

            // Size radio buttons
            switch (_working.Size)
            {
                case "Huge": rbSizeHuge.IsChecked = true; break;
                case "Big": rbSizeBig.IsChecked = true; break;
                case "Medium": rbSizeMedium.IsChecked = true; break;
                default: rbSizeSmall.IsChecked = true; break;
            }

            // Display location
            switch (_working.DisplayLocation)
            {
                case "Bottom Right": rbLocationBottomRight.IsChecked = true; break;
                case "Bottom Left": rbLocationBottomLeft.IsChecked = true; break;
                case "Top Left": rbLocationTopLeft.IsChecked = true; break;
                default: rbLocationTopRight.IsChecked = true; break;
            }

            // Time format
            if (_working.TimeFormat == "12 hs.") rbTime12.IsChecked = true; else rbTime24.IsChecked = true;

            chkSeconds.IsChecked = _working.ShowSeconds;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            var bgName = cbBackground.SelectedItem as string ?? _backgrounds[0].name;
            var bg = _backgrounds.First(b => b.name == bgName);

            var ftName = cbFont.SelectedItem as string ?? _fontColors[0].name;
            var ft = _fontColors.First(f => f.name == ftName);

            _working.Background = bg.name;
            _working.BackgroundHex = bg.hex;
            _working.Font = ft.name;
            _working.FontHex = ft.hex;
            _working.Opacity = (int)sliderOpacity.Value;

            if (rbSizeHuge.IsChecked == true) _working.Size = "Huge";
            else if (rbSizeBig.IsChecked == true) _working.Size = "Big";
            else if (rbSizeMedium.IsChecked == true) _working.Size = "Medium";
            else _working.Size = "Small";

            if (rbLocationBottomRight.IsChecked == true) _working.DisplayLocation = "Bottom Right";
            else if (rbLocationBottomLeft.IsChecked == true) _working.DisplayLocation = "Bottom Left";
            else if (rbLocationTopLeft.IsChecked == true) _working.DisplayLocation = "Top Left";
            else _working.DisplayLocation = "Top Right";

            _working.TimeFormat = (rbTime12.IsChecked == true) ? "12 hs." : "24 hs.";
            _working.ShowSeconds = chkSeconds.IsChecked == true;

            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "ketch_settings_debug.log");
                File.AppendAllText(path, $"{DateTime.UtcNow:o} - Settings OK: Background={_working.Background} ({_working.BackgroundHex}), Font={_working.Font} ({_working.FontHex}), Opacity={_working.Opacity}, Size={_working.Size}, Location={_working.DisplayLocation}, TimeFormat={_working.TimeFormat}, ShowSeconds={_working.ShowSeconds}\n");
            }
            catch { }

            Result = _working;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
