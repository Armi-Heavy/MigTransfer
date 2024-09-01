using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MigTransfer
{
    public partial class Form1 : Form
    {
        private ImageLoader imageLoader;

        public Form1()
        {
            InitializeComponent();
            imageLoader = new ImageLoader();
            LoadImagesToFlowLayoutPanel();
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
