using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
                // Obtener el nombre del directorio desde la ruta de la imagen, que ahora proviene de ImageUrl
                var directoryName = Path.GetFileName(Path.GetDirectoryName(imageItem.ImageUrl));

                // Decodificar la URL correctamente para que no tenga codificación innecesaria
                directoryName = Uri.UnescapeDataString(directoryName); // Decodifica la URL

                // Eliminar "cover.jpg" si está presente al final del nombre del directorio
                if (directoryName.EndsWith("cover.jpg", StringComparison.OrdinalIgnoreCase))
                {
                    directoryName = directoryName.Substring(0, directoryName.Length - "cover.jpg".Length);
                }

                // Formar la ruta local para el directorio correspondiente al servidor
                string localDirectoryPath = Path.Combine(form.GetActiveDrive().RootDirectory.FullName, directoryName);

                // Verificar si el directorio existe en el disco externo
                bool directoryExistsOnDrive = Directory.Exists(localDirectoryPath);

                if (directoryExistsOnDrive)
                {
                    // Si el directorio existe en el disco externo, marcar el checkbox
                    imageItem.SetCheckBoxChecked(true, true);
                }
                else
                {
                    // Si el directorio no existe en el disco externo, proceder con la comparación en el servidor HTTP
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
}