using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MigTransfer
{
    public partial class Form1 : Form
    {
        private readonly ImageLoader imageLoader = new ImageLoader();
        private readonly ExFatDriveDetector exFatDriveDetector = new ExFatDriveDetector();
        private DriveInfo activeDrive;
        private Panel activeDrivePanel;

        public Form1()
        {
            InitializeComponent();
            LoadImagesToFlowLayoutPanel();
            LoadExFatDrivesToFlowLayoutPanel();
            this.Resize += (s, e) => AdjustFlowLayoutPanelSizes();
        }

        private void LoadImagesToFlowLayoutPanel()
        {
            var imagePaths = imageLoader.LoadImagePaths();
            foreach (var imagePath in imagePaths)
            {
                try
                {
                    flowLayoutPanel1.Controls.Add(new ImageItem(imagePath, this));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar la imagen: {imagePath}\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadExFatDrivesToFlowLayoutPanel()
        {
            flowLayoutPanel2.Controls.Clear();
            foreach (var drive in exFatDriveDetector.GetExFatDrives())
            {
                var panel = exFatDriveDetector.CreateDrivePanel(drive, flowLayoutPanel2.Width);
                panel.Click += (s, e) => SetActiveDrive(drive, panel);
                foreach (Control control in panel.Controls)
                {
                    control.Click += (s, e) => SetActiveDrive(drive, panel);
                }
                flowLayoutPanel2.Controls.Add(panel);
            }
        }

        private void SetActiveDrive(DriveInfo drive, Panel panel)
        {
            activeDrive = drive;
            activeDrivePanel?.Invoke((MethodInvoker)(() => activeDrivePanel.BorderStyle = BorderStyle.None));
            activeDrivePanel = panel;
            activeDrivePanel.BorderStyle = BorderStyle.FixedSingle;
            CompareAndMarkCheckBoxes();
        }

        private void AdjustFlowLayoutPanelSizes()
        {
            flowLayoutPanel1.Width = this.ClientSize.Width - flowLayoutPanel2.Width - 20;
            flowLayoutPanel1.Height = flowLayoutPanel2.Height = this.ClientSize.Height - 20;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            exFatDriveDetector.StopWatcher();
            base.OnFormClosing(e);
        }

        public DriveInfo GetActiveDrive() => activeDrive;

        public void CompareAndMarkCheckBoxes()
        {
            if (activeDrive == null)
            {
                MessageBox.Show("Por favor, seleccione un dispositivo activo.", "Dispositivo no seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var activeDriveFiles = Directory.GetFiles(activeDrive.RootDirectory.FullName, "*.*", SearchOption.AllDirectories);
            var switchFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ownCloud", "Switch");

            foreach (ImageItem imageItem in flowLayoutPanel1.Controls.OfType<ImageItem>())
            {
                var directoryName = Path.GetFileName(Path.GetDirectoryName(imageItem.ImagePath));
                var switchFiles = Directory.GetFiles(Path.Combine(switchFolderPath, directoryName), "*.*", SearchOption.TopDirectoryOnly);

                foreach (var switchFile in switchFiles)
                {
                    if (activeDriveFiles.Any(activeDriveFile => Path.GetFileName(activeDriveFile).Equals(Path.GetFileName(switchFile), StringComparison.OrdinalIgnoreCase)))
                    {
                        imageItem.SetCheckBoxChecked(true, true);
                        break;
                    }
                }
            }
        }
    }
}
