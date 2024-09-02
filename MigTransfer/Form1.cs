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
        private DriveInfo activeDrive; // Añadir esta línea
        private Panel activeDrivePanel; // Añadir esta línea

        public Form1()
        {
            InitializeComponent();
            imageLoader = new ImageLoader();
            exFatDriveDetector = new ExFatDriveDetector();
            exFatDriveDetector.DrivesChanged += OnDrivesChanged;
            LoadImagesToFlowLayoutPanel();
            LoadExFatDrivesToFlowLayoutPanel();
            this.Resize += new EventHandler(Form1_Resize);
        }

        private void LoadImagesToFlowLayoutPanel()
        {
            List<string> imagePaths = imageLoader.LoadImagePaths();

            foreach (var imagePath in imagePaths)
            {
                Image image = Image.FromFile(imagePath);
                ImageItem imageItem = new ImageItem(image, imagePath, this); // Pasar `this` para acceder a `activeDrive`
                flowLayoutPanel1.Controls.Add(imageItem);
            }
        }

        private void LoadExFatDrivesToFlowLayoutPanel()
        {
            var exFatDrives = exFatDriveDetector.GetExFatDrives();
            flowLayoutPanel2.Controls.Clear();
            foreach (var drive in exFatDrives)
            {
                Panel panel = exFatDriveDetector.CreateDrivePanel(drive, flowLayoutPanel2.Width);
                panel.Click += (s, e) => SetActiveDrive(drive, panel); // Modificar esta línea
                flowLayoutPanel2.Controls.Add(panel);
            }
        }

        private void SetActiveDrive(DriveInfo drive, Panel panel) // Modificar esta línea
        {
            activeDrive = drive;
            if (activeDrivePanel != null)
            {
                activeDrivePanel.BorderStyle = BorderStyle.None; // Quitar el borde del panel anterior
            }
            activeDrivePanel = panel;
            activeDrivePanel.BorderStyle = BorderStyle.FixedSingle; // Añadir borde negro al panel activo
        }

        private void OnDrivesChanged(object sender, EventArgs e)
        {
            // Volver a cargar las unidades EXfat cuando se detecta un cambio
            LoadExFatDrivesToFlowLayoutPanel();
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            AdjustFlowLayoutPanelSizes();
            flowLayoutPanel1.PerformLayout(); // Forzar la reorganización
        }

        private void AdjustFlowLayoutPanelSizes()
        {
            flowLayoutPanel1.Width = this.ClientSize.Width - flowLayoutPanel2.Width - 20; // Ajuste de margen
            flowLayoutPanel1.Height = this.ClientSize.Height - 20; // Ajuste de margen
            flowLayoutPanel2.Height = this.ClientSize.Height - 20; // Ajuste de margen
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Detener el watcher cuando se cierra el formulario
            exFatDriveDetector.StopWatcher();
            base.OnFormClosing(e);
        }

        public DriveInfo GetActiveDrive() // Añadir este método para acceder a `activeDrive`
        {
            return activeDrive;
        }
    }
}
