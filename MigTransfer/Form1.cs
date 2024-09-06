using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MigTransfer;

namespace MigTransfer
{
    public partial class Form1 : Form
    {
        private readonly ImageLoader imageLoader = new ImageLoader();
        private readonly ExFatDriveDetector exFatDriveDetector = new ExFatDriveDetector();
        private readonly DriveSpaceManager driveSpaceManager;
        private DriveInfo activeDrive;
        public Panel activeDrivePanel; // Cambiar a public o internal
        private FileCopyManager fileCopyManager;
        private DirectoryComparer directoryComparer;
        private readonly UsbEventWatcher usbEventWatcher;
        private readonly CopyQueueManager copyQueueManager;

        public event EventHandler? DriveSpaceUpdated;

        public Form1()
        {
            InitializeComponent();
            driveSpaceManager = new DriveSpaceManager(this);
            fileCopyManager = new FileCopyManager(driveSpaceManager, activeDrivePanel, GetActiveDrive());
            copyQueueManager = new CopyQueueManager(fileCopyManager);
            directoryComparer = new DirectoryComparer(this);
            usbEventWatcher = new UsbEventWatcher(flowLayoutPanel2, this);
            LoadImagesToFlowLayoutPanel();
            LoadExFatDrivesToFlowLayoutPanel();
            this.Resize += (s, e) => AdjustFlowLayoutPanelSizes();
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
        }

        public DriveInfo GetActiveDrive()
        {
            return activeDrive;
        }

        private void LoadImagesToFlowLayoutPanel()
        {
            var imagePaths = imageLoader.LoadImagePaths();
            foreach (var imagePath in imagePaths)
            {
                try
                {
                    flowLayoutPanel1.Controls.Add(new ImageItem(imagePath, this, copyQueueManager));
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

        public void SetActiveDrive(DriveInfo drive, Panel panel)
        {
            activeDrive = drive;
            activeDrivePanel?.Invoke((MethodInvoker)(() => activeDrivePanel.BorderStyle = BorderStyle.None));
            activeDrivePanel = panel;
            activeDrivePanel.BorderStyle = BorderStyle.FixedSingle;
            CompareAndMarkCheckBoxes();
        }

        public void UncheckAllItems()
        {
            foreach (var imageItem in flowLayoutPanel1.Controls.OfType<ImageItem>())
            {
                imageItem.SetCheckBoxChecked(false);
            }
        }

        private void AdjustFlowLayoutPanelSizes()
        {
            flowLayoutPanel1.Width = this.ClientSize.Width - flowLayoutPanel2.Width - 20;
            flowLayoutPanel1.Height = flowLayoutPanel2.Height = this.ClientSize.Height - 20;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            exFatDriveDetector.StopWatcher();
            usbEventWatcher.Stop();
            Application.Exit(); // Asegurarse de que la aplicación se cierre completamente
        }

        public void CompareAndMarkCheckBoxes()
        {
            directoryComparer.CompareAndMarkCheckBoxes();
        }

        public void UpdateDrivePanel(DriveInfo drive, Panel panel)
        {
            driveSpaceManager.UpdateDrivePanel(drive, panel);
        }

        public void OnDriveSpaceUpdated()
        {
            DriveSpaceUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}