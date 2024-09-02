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
    public const uint SHGFI_LARGEICON = 0x000000000; // Large icon
    public const uint SHGFI_SMALLICON = 0x000000001; // Small icon

    private UsbEventWatcher usbEventWatcher;

    public event EventHandler DrivesChanged;

    public ExFatDriveDetector()
    {
        usbEventWatcher = new UsbEventWatcher();
        usbEventWatcher.UsbInserted += OnUsbChanged;
        usbEventWatcher.UsbRemoved += OnUsbChanged;
    }

    public List<DriveInfo> GetExFatDrives()
    {
        List<DriveInfo> exFatDrives = new List<DriveInfo>();
        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            if (drive.IsReady && drive.DriveFormat.Equals("exFAT", StringComparison.OrdinalIgnoreCase))
            {
                exFatDrives.Add(drive);
            }
        }
        return exFatDrives;
    }

    public Icon GetSystemIcon(string path)
    {
        SHFILEINFO shinfo = new SHFILEINFO();
        IntPtr hImgSmall = SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON);

        if (shinfo.hIcon != IntPtr.Zero)
        {
            return Icon.FromHandle(shinfo.hIcon);
        }
        return null;
    }

    public Panel CreateDrivePanel(DriveInfo drive, int panelWidth)
    {
        // Crear un panel para contener el icono, la barra de progreso y el nombre del dispositivo
        Panel panel = new Panel();
        panel.Width = panelWidth - 20;
        panel.Height = 90; // Aumentar la altura para acomodar la barra de progreso debajo del icono
        panel.Margin = new Padding(5);
        panel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // Crear el PictureBox para el icono del dispositivo
        PictureBox pictureBox = new PictureBox();
        Icon icon = GetSystemIcon(drive.Name);
        if (icon != null)
        {
            pictureBox.Image = icon.ToBitmap();
        }
        pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        pictureBox.Width = 50;
        pictureBox.Height = 50;
        pictureBox.Location = new Point(10, 10);
        pictureBox.Tag = drive;

        // Crear la barra de progreso personalizada para mostrar el tamaño usado
        CustomProgressBar progressBar = new CustomProgressBar();
        progressBar.Width = panel.Width - 20;
        progressBar.Height = 20;
        progressBar.Location = new Point(10, 70); // Colocar debajo del icono
        progressBar.Minimum = 0;
        progressBar.Maximum = (int)(drive.TotalSize / (1024 * 1024 * 1024)); // Convertir a GB
        int usedSpace = (int)((drive.TotalSize - drive.AvailableFreeSpace) / (1024 * 1024 * 1024)); // Convertir a GB
        progressBar.Value = Math.Max(progressBar.Minimum, Math.Min(usedSpace, progressBar.Maximum)); // Asegurar que el valor esté dentro del rango
        progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // Crear la etiqueta para mostrar el nombre del dispositivo
        Label label = new Label();
        label.Text = $"{drive.Name} ({drive.VolumeLabel})";
        label.Location = new Point(70, 10);
        label.AutoSize = true;
        label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // Crear la etiqueta para mostrar el tamaño usado y el tamaño total en GB
        Label sizeLabel = new Label();
        sizeLabel.Text = $"{usedSpace} GB / {progressBar.Maximum} GB";
        sizeLabel.Location = new Point(70, 30);
        sizeLabel.AutoSize = true;
        sizeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // Añadir los controles al panel
        panel.Controls.Add(pictureBox);
        panel.Controls.Add(progressBar);
        panel.Controls.Add(label);
        panel.Controls.Add(sizeLabel);

        // Añadir el evento Click a todos los controles del panel
        panel.Click += (s, e) => OnPanelClick(drive, panel);
        pictureBox.Click += (s, e) => OnPanelClick(drive, panel);
        progressBar.Click += (s, e) => OnPanelClick(drive, panel);
        label.Click += (s, e) => OnPanelClick(drive, panel);
        sizeLabel.Click += (s, e) => OnPanelClick(drive, panel);

        return panel;
    }

    private void OnPanelClick(DriveInfo drive, Panel panel)
    {
        DrivesChanged?.Invoke(drive, EventArgs.Empty);
    }

    private void OnUsbChanged(object sender, EventArgs e)
    {
        DrivesChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StopWatcher()
    {
        usbEventWatcher.Stop();
    }
}
