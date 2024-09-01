using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MigTransfer
{
    public class ImageLoader
    {
        public List<Image> LoadImages()
        {
            List<Image> images = new List<Image>();
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            //string switchFolderPath = Path.Combine(userFolderPath, "ownCloud", "Switch");
            string switchFolderPath = Path.Combine(userFolderPath, "Test", "Switch");

            if (Directory.Exists(switchFolderPath))
            {
                var directories = Directory.GetDirectories(switchFolderPath);

                foreach (var directory in directories)
                {
                    string imagePath = Path.Combine(directory, "Cover.jpg");
                    if (File.Exists(imagePath))
                    {
                        Image image = Image.FromFile(imagePath);
                        Image resizedImage = new Bitmap(image, new Size(256, 414));
                        images.Add(resizedImage);
                    }
                }
            }

            return images;
        }
    }
}