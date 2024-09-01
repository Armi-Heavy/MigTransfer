using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MigTransfer
{
    public partial class Form1 : Form
    {
        private ImageLoader imageLoader;
        private ExFatDriveDetector exFatDriveDetector;

        public Form1()
        {
            InitializeComponent();
            imageLoader = new ImageLoader();
            exFatDriveDetector = new ExFatDriveDetector();
            LoadImagesToFlowLayoutPanel();
            LoadExFatDrivesToFlowLayoutPanel();
            this.Resize += new EventHandler(Form1_Resize);
        }

        private void LoadImagesToFlowLayoutPanel()
        {
            List<Image> images = imageLoader.LoadImages();

            foreach (var image in images)
            {
                ImageItem imageItem = new ImageItem(image);
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
                flowLayoutPanel2.Controls.Add(panel);
            }
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
    }
}
