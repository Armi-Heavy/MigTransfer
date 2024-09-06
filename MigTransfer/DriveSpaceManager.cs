using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MigTransfer
{
    public class DriveSpaceManager
    {
        private readonly Form1 form;

        public DriveSpaceManager(Form1 form)
        {
            this.form = form;
        }

        public void UpdateDrivePanel(DriveInfo drive, Panel panel)
        {
            if (panel == null || panel.Controls == null)
            {
                return; // Salir si el panel o sus controles son nulos
            }

            var progressBar = panel.Controls.OfType<CustomProgressBar>().FirstOrDefault();
            var sizeLabel = panel.Controls.OfType<Label>().LastOrDefault();

            if (progressBar != null && sizeLabel != null)
            {
                progressBar.Maximum = (int)(drive.TotalSize / (1024 * 1024 * 1024));
                progressBar.Value = Math.Max(0, Math.Min((int)((drive.TotalSize - drive.AvailableFreeSpace) / (1024 * 1024 * 1024)), progressBar.Maximum));
                sizeLabel.Text = $"{(int)((drive.TotalSize - drive.AvailableFreeSpace) / (1024 * 1024 * 1024))} GB / {progressBar.Maximum} GB";
            }
        }
    }
}