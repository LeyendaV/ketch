using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Ketch
{
    public class TrayIconManager : IDisposable
    {
        private readonly NotifyIcon _notify;
        private readonly Action _openSettings;
        private readonly Action _openAbout;
        private readonly Action _exit;
        private ContextMenuStrip? _menu;

        // The iconPath parameter is retained for source compatibility; the icon is now loaded
        // from the WPF Resource embedded in the single-file application.
        public TrayIconManager(Action openSettings, Action openAbout, Action exit, string iconPath)
        {
            _openSettings = openSettings;
            _openAbout = openAbout;
            _exit = exit;
            _notify = new NotifyIcon();
        }

        public void Initialize()
        {
            try
            {
                var resource = System.Windows.Application.GetResourceStream(
                    new Uri("pack://application:,,,/Assets/ketch_icon.ico", UriKind.Absolute));
                if (resource != null)
                {
                    using (resource.Stream)
                    using (var loadedIcon = new Icon(resource.Stream))
                    {
                        _notify.Icon = (Icon)loadedIcon.Clone();
                    }
                }
                else
                {
                    _notify.Icon = SystemIcons.Application;
                }
            }
            catch
            {
                _notify.Icon = SystemIcons.Application;
            }

            _notify.Text = "ketch clock";
            _menu = new ContextMenuStrip();
            var settingsItem = new ToolStripMenuItem("Settings");
            settingsItem.Click += (_, _) => _openSettings?.Invoke();

            var aboutItem = new ToolStripMenuItem("About");
            aboutItem.Click += (_, _) => _openAbout?.Invoke();

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (_, _) => _exit?.Invoke();

            _menu.Items.AddRange(new ToolStripItem[]
            {
                settingsItem,
                aboutItem,
                new ToolStripSeparator(),
                exitItem
            });

            _notify.ContextMenuStrip = _menu;
            _notify.Visible = true;
            _notify.MouseClick += Notify_MouseClick;
            _notify.MouseDoubleClick += Notify_MouseDoubleClick;
        }

        private void Notify_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) _openSettings?.Invoke();
        }

        private void Notify_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) _menu?.Show(Cursor.Position);
        }

        public void Dispose()
        {
            _notify.Visible = false;
            try { _menu?.Dispose(); } catch { }
            _notify.Dispose();
        }
    }
}
