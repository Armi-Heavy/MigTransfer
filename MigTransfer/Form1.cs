using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MigTransfer
{
    public partial class Form1 : Form
    {
        private ImageLoader imageLoader;
        private ExFatDriveDetector exFatDriveDetector;
        private DriveInfo activeDrive;
        private Panel activeDrivePanel;

        public Form1()
        {
            InitializeComponent();
            imageLoader = new ImageLoader();
            exFatDriveDetector = new ExFatDriveDetector();
            // Eliminar la suscripción al evento DrivesChanged
            LoadImagesToFlowLayoutPanel();
            LoadExFatDrivesToFlowLayoutPanel();
            this.Resize += new EventHandler(Form1_Resize);
        }


        private void LoadImagesToFlowLayoutPanel()
        {
            List<string> imagePaths = imageLoader.LoadImagePaths();

            foreach (var imagePath in imagePaths)
            {
                try
                {
                    ImageItem imageItem = new ImageItem(imagePath, this);
                    flowLayoutPanel1.Controls.Add(imageItem);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar la imagen: {imagePath}\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadExFatDrivesToFlowLayoutPanel()
        {
            var exFatDrives = exFatDriveDetector.GetExFatDrives();
            flowLayoutPanel2.Controls.Clear();
            foreach (var drive in exFatDrives)
            {
                Panel panel = exFatDriveDetector.CreateDrivePanel(drive, flowLayoutPanel2.Width);
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
            if (activeDrivePanel != null)
            {
                activeDrivePanel.BorderStyle = BorderStyle.None;
            }
            activeDrivePanel = panel;
            activeDrivePanel.BorderStyle = BorderStyle.FixedSingle;

            CompareAndMarkCheckBoxes(); // Llamar al método de comparación después de seleccionar el disco activo
        }

        private void OnDrivesChanged(object sender, EventArgs e)
        {
            LoadExFatDrivesToFlowLayoutPanel();
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            AdjustFlowLayoutPanelSizes();
            flowLayoutPanel1.PerformLayout();
        }

        private void AdjustFlowLayoutPanelSizes()
        {
            flowLayoutPanel1.Width = this.ClientSize.Width - flowLayoutPanel2.Width - 20;
            flowLayoutPanel1.Height = this.ClientSize.Height - 20;
            flowLayoutPanel2.Height = this.ClientSize.Height - 20;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            exFatDriveDetector.StopWatcher();
            base.OnFormClosing(e);
        }

        public DriveInfo GetActiveDrive()
        {
            return activeDrive;
        }

        public void CompareAndMarkCheckBoxes()
        {
            if (activeDrive == null)
            {
                MessageBox.Show("Por favor, seleccione un dispositivo activo.", "Dispositivo no seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string[] activeDriveFiles = Directory.GetFiles(activeDrive.RootDirectory.FullName, "*.*", SearchOption.AllDirectories);
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string switchFolderPath = Path.Combine(userFolderPath, "ownCloud", "Switch");

            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is ImageItem imageItem)
                {
                    string directoryName = Path.GetFileName(Path.GetDirectoryName(imageItem.ImagePath));
                    string[] switchFiles = Directory.GetFiles(Path.Combine(switchFolderPath, directoryName), "*.*", SearchOption.TopDirectoryOnly);

                    foreach (string switchFile in switchFiles)
                    {
                        string switchFileName = Path.GetFileName(switchFile);
                        foreach (string activeDriveFile in activeDriveFiles)
                        {
                            if (Path.GetFileName(activeDriveFile).Equals(switchFileName, StringComparison.OrdinalIgnoreCase))
                            {
                                imageItem.SetCheckBoxChecked(true, true); // Marcar el checkbox y mostrar la barra de progreso al 100%
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
