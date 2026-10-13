using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ketch
{
    public class TrayIconManager : IDisposable
    {
        private readonly NotifyIcon _notify;
        private readonly Action _openSettings;
        private readonly Action _openAbout;
        private readonly Action _exit;
        private readonly string _iconPath;
        private ContextMenuStrip? _menu;

        public TrayIconManager(Action openSettings, Action openAbout, Action exit, string iconPath)
        {
            _openSettings = openSettings;
            _openAbout = openAbout;
            _exit = exit;
            _iconPath = iconPath;
            _notify = new NotifyIcon();
        }

        public void Initialize()
        {
            if (System.IO.File.Exists(_iconPath))
            {
                _notify.Icon = new Icon(_iconPath);
            }
            else
            {
                _notify.Icon = SystemIcons.Application;
            }

            // Set the tooltip text shown on hover
            _notify.Text = "ketch clock";

            // Create a persistent context menu and attach it to the NotifyIcon so that clicking outside closes it normally
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
            if (e.Button == MouseButtons.Left)
            {
                _openSettings?.Invoke();
            }
        }

        private void Notify_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Show the persistent context menu; Windows will handle closing it when the user clicks outside
                _menu?.Show(Cursor.Position);
            }
        }

        public void Dispose()
        {
            _notify.Visible = false;
            try { _menu?.Dispose(); } catch { }
            _notify.Dispose();
        }
    }
}
