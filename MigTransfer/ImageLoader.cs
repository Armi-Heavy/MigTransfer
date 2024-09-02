using System;
using System.Collections.Generic;
using System.IO;

namespace MigTransfer
{
    public class ImageLoader
    {
        public List<string> LoadImagePaths()
        {
            List<string> imagePaths = new List<string>();
            string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string switchFolderPath = Path.Combine(userFolderPath, "ownCloud", "Switch");

            if (Directory.Exists(switchFolderPath))
            {
                var directories = Directory.GetDirectories(switchFolderPath);

                foreach (var directory in directories)
                {
                    string imagePath = Path.Combine(directory, "Cover.jpg");
                    if (File.Exists(imagePath))
                    {
                        imagePaths.Add(imagePath);
                    }
                }
            }

            return imagePaths;
        }
    }
}
