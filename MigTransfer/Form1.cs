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

            // Añadir el evento TextChanged para textBox1
            textBox1.TextChanged += TextBox1_TextChanged;
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
            // Ajustar el tamaño de flowLayoutPanel2 verticalmente
            flowLayoutPanel2.Height = this.ClientSize.Height - flowLayoutPanel3.Height - 20;

            // Ajustar el tamaño de flowLayoutPanel3 solo horizontalmente
            flowLayoutPanel3.Width = this.ClientSize.Width - flowLayoutPanel2.Width - 20;

            // Ajustar el tamaño de flowLayoutPanel1 horizontal y verticalmente
            flowLayoutPanel1.Width = this.ClientSize.Width - flowLayoutPanel2.Width - 20;
            flowLayoutPanel1.Height = this.ClientSize.Height - flowLayoutPanel3.Height - 20;

            // Asegurarse de que flowLayoutPanel1 y flowLayoutPanel2 se toquen por sus lados
            flowLayoutPanel1.Top = flowLayoutPanel3.Bottom + 10;
            flowLayoutPanel2.Top = flowLayoutPanel3.Bottom + 10;
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (copyQueueManager.HasPendingItems())
            {
                var result = MessageBox.Show("Hay transferencias en curso. ¿Estás seguro de que deseas cerrar la aplicación?", "Confirmar cierre", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    e.Cancel = true; // Cancela el cierre de la aplicación
                    return;
                }
                else
                {
                    copyQueueManager.CancelAllCopies(); // Cancela todas las copias en curso

                    // Esperar un momento para asegurarse de que las copias se han cancelado
                    await Task.Delay(1000000);

                    // Desmarcar los elementos y eliminar los archivos
                    foreach (var imageItem in flowLayoutPanel1.Controls.OfType<ImageItem>())
                    {
                        imageItem.Invoke((MethodInvoker)(() => imageItem.SetCheckBoxChecked(false)));
                    }
                }
            }

            exFatDriveDetector.StopWatcher();
            usbEventWatcher.Stop();
            Application.Exit(); // Asegurarse de que la aplicación se cierre completamente
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            string filterText = textBox1.Text.ToLower();
            foreach (var imageItem in flowLayoutPanel1.Controls.OfType<ImageItem>())
            {
                string directoryName = Path.GetFileName(Path.GetDirectoryName(imageItem.ImagePath)).ToLower();
                imageItem.Visible = directoryName.Contains(filterText);
            }
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