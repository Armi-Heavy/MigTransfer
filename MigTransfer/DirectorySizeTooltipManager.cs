using System;
using System.IO;

namespace MigTransfer
{
    public class DirectorySizeTooltipManager
    {
        private readonly DriveSpaceManager driveSpaceManager;

        public DirectorySizeTooltipManager(DriveSpaceManager driveSpaceManager)
        {
            this.driveSpaceManager = driveSpaceManager;
        }

        /// <summary>
        /// Obtiene el peso del directorio en gigabytes.
        /// </summary>
        /// <param name="directoryPath">Ruta del directorio.</param>
        /// <returns>Peso del directorio en GB redondeado hacia el múltiplo superior más cercano de 2, 4, 8, 16, 32 o 64.</returns>
        public int GetDirectorySizeInGB(string directoryPath)
        {
            try
            {
                long directorySize = GetDirectorySize(directoryPath);
                double sizeInGB = directorySize / (1024.0 * 1024.0 * 1024.0);
                return RoundUpToNextThreshold(sizeInGB);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al calcular el tamaño del directorio: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Comprueba si el directorio cabe en el dispositivo seleccionado.
        /// </summary>
        /// <param name="directoryPath">Ruta del directorio.</param>
        /// <param name="driveInfo">Información de la unidad seleccionada.</param>
        /// <returns>True si cabe; False en caso contrario.</returns>
        public bool DoesDirectoryFit(string directoryPath, DriveInfo driveInfo)
        {
            long directorySize = GetDirectorySize(directoryPath);
            long availableSpace = driveInfo.AvailableFreeSpace;

            return directorySize <= availableSpace;
        }

        /// <summary>
        /// Calcula el tamaño total de un directorio.
        /// </summary>
        /// <param name="directoryPath">Ruta del directorio.</param>
        /// <returns>Tamaño del directorio en bytes.</returns>
        private long GetDirectorySize(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"El directorio '{directoryPath}' no existe.");
            }

            long size = 0;

            // Sumar el tamaño de los archivos en el directorio
            foreach (string file in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(file);
                size += fileInfo.Length;
            }

            return size;
        }

        /// <summary>
        /// Redondea hacia el siguiente múltiplo superior permitido.
        /// </summary>
        /// <param name="sizeInGB">Tamaño en GB.</param>
        /// <returns>Múltiplo de 2, 4, 8, 16, 32 o 64 GB más cercano.</returns>
        private int RoundUpToNextThreshold(double sizeInGB)
        {
            int[] thresholds = { 2, 4, 8, 16, 32, 64 };

            foreach (int threshold in thresholds)
            {
                if (sizeInGB <= threshold)
                {
                    return threshold;
                }
            }

            return thresholds[^1]; // Devolver el máximo si excede todos los umbrales
        }
    }
}
