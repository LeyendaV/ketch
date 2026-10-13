using System.Collections.Generic;
using System.Windows;

namespace Ketch
{
    public partial class SettingsWindow : Window
    {
        public SettingsModel Result { get; private set; }
        private readonly SettingsModel _working;

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

            var backgrounds = new List<(string name, string hex)>
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
            cbBackground.ItemsSource = backgrounds;
            cbBackground.DisplayMemberPath = "name";
            cbBackground.SelectedIndex = backgrounds.FindIndex(item => item.name == _working.Background);
            if (cbBackground.SelectedIndex < 0) cbBackground.SelectedIndex = 0;

            var fonts = new List<(string name, string hex)>
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
            cbFont.ItemsSource = fonts;
            cbFont.DisplayMemberPath = "name";
            cbFont.SelectedIndex = fonts.FindIndex(item => item.name == _working.Font);
            if (cbFont.SelectedIndex < 0) cbFont.SelectedIndex = 0;

            var opacities = new List<int> { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            cbOpacity.ItemsSource = opacities;
            cbOpacity.SelectedItem = _working.Opacity;

            var sizes = new List<string> { "Huge", "Big", "Medium", "Small" };
            cbSize.ItemsSource = sizes;
            cbSize.SelectedItem = _working.Size;

            var locations = new List<string> { "Bottom Right", "Bottom Left", "Top Left", "Top Right" };
            cbLocation.ItemsSource = locations;
            cbLocation.SelectedItem = _working.DisplayLocation;

            var formats = new List<string> { "12 hs.", "24 hs." };
            cbTimeFormat.ItemsSource = formats;
            cbTimeFormat.SelectedItem = _working.TimeFormat;

            chkSeconds.IsChecked = _working.ShowSeconds;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            var bg = ((string, string))cbBackground.SelectedItem;
            var ft = ((string, string))cbFont.SelectedItem;

            _working.Background = bg.Item1;
            _working.BackgroundHex = bg.Item2;
            _working.Font = ft.Item1;
            _working.FontHex = ft.Item2;
            _working.Opacity = (int)cbOpacity.SelectedItem;
            _working.Size = (string)cbSize.SelectedItem;
            _working.DisplayLocation = (string)cbLocation.SelectedItem;
            _working.TimeFormat = (string)cbTimeFormat.SelectedItem;
            _working.ShowSeconds = chkSeconds.IsChecked == true;

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
