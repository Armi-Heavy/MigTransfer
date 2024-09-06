using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MigTransfer
{
    public class DirectoryComparer
    {
        private readonly Form1 form;

        public DirectoryComparer(Form1 form)
        {
            this.form = form;
        }

        public void CompareAndMarkCheckBoxes()
        {
            if (form.GetActiveDrive() == null)
            {
                MessageBox.Show("Por favor, seleccione un dispositivo activo.", "Dispositivo no seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var activeDriveFiles = Directory.GetFiles(form.GetActiveDrive().RootDirectory.FullName, "*.*", SearchOption.AllDirectories);
            var switchFolderPath = GlobalSettings.SwitchFolderPath;

            foreach (ImageItem imageItem in form.Controls.OfType<FlowLayoutPanel>().FirstOrDefault(p => p.Name == "flowLayoutPanel1").Controls.OfType<ImageItem>())
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