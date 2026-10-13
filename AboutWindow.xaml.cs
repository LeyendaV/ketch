using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace Ketch
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            // Close when the window loses activation (click outside).
            this.Deactivated += (s, e) => this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
