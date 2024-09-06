using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MigTransfer
{
    public class ImageLoader
    {
        public List<string> LoadImagePaths()
        {
            var switchFolderPath = GlobalSettings.SwitchFolderPath;

            if (!Directory.Exists(switchFolderPath))
                return new List<string>();

            try
            {
                return Directory.GetDirectories(switchFolderPath)
                                .Select(directory => Path.Combine(directory, "Cover.jpg"))
                                .Where(File.Exists)
                                .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las rutas de las imágenes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<string>();
            }
        }
    }
}