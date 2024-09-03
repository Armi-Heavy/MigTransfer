using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class ExFatDriveDetector
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

    public const uint SHGFI_ICON = 0x000000100;
    public const uint SHGFI_SMALLICON = 0x000000001;

    public event EventHandler? DrivesChanged;

    public List<DriveInfo> GetExFatDrives() =>
        DriveInfo.GetDrives()
                 .Where(drive => drive.IsReady && drive.DriveFormat.Equals("exFAT", StringComparison.OrdinalIgnoreCase))
                 .ToList();

    public Icon? GetSystemIcon(string path)
    {
        var shinfo = new SHFILEINFO();
        var hImgSmall = SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON);

        if (shinfo.hIcon != IntPtr.Zero)
        {
            var icon = Icon.FromHandle(shinfo.hIcon);
            var clonedIcon = (Icon)icon.Clone();
            DestroyIcon(icon.Handle);
            return clonedIcon;
        }
        return null;
    }

    public Panel CreateDrivePanel(DriveInfo drive, int panelWidth)
    {
        var panel = new Panel
        {
            Width = panelWidth - 20,
            Height = 90,
            Margin = new Padding(5),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var pictureBox = new PictureBox
        {
            Image = GetSystemIcon(drive.Name)?.ToBitmap(),
            SizeMode = PictureBoxSizeMode.StretchImage,
            Width = 50,
            Height = 50,
            Location = new Point(10, 10),
            Tag = drive
        };

        var progressBar = new CustomProgressBar
        {
            Width = panel.Width - 20,
            Height = 20,
            Location = new Point(10, 70),
            Minimum = 0,
            Maximum = (int)(drive.TotalSize / (1024 * 1024 * 1024)),
            Value = Math.Max(0, Math.Min((int)((drive.TotalSize - drive.AvailableFreeSpace) / (1024 * 1024 * 1024)), (int)(drive.TotalSize / (1024 * 1024 * 1024)))),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var label = new Label
        {
            Text = $"{drive.Name} ({drive.VolumeLabel})",
            Location = new Point(70, 10),
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var sizeLabel = new Label
        {
            Text = $"{(int)((drive.TotalSize - drive.AvailableFreeSpace) / (1024 * 1024 * 1024))} GB / {(int)(drive.TotalSize / (1024 * 1024 * 1024))} GB",
            Location = new Point(70, 30),
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        panel.Controls.AddRange(new Control[] { pictureBox, progressBar, label, sizeLabel });

        foreach (Control control in panel.Controls)
        {
            control.Click += (s, e) => DrivesChanged?.Invoke(drive, EventArgs.Empty);
        }

        return panel;
    }

    public void StopWatcher() { }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    extern static bool DestroyIcon(IntPtr handle);
}
