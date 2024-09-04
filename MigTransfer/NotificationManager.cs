using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MigTransfer
{
    public class NotificationManager
    {
        public static void ShowNotification(string imagePath, string directoryPath, Image itemImage)
        {
            // Obtener el nombre del directorio padre truncado hasta antes del carácter '['
            string directoryName = Path.GetFileName(directoryPath);
            int index = directoryName.IndexOf('[');
            if (index > 0)
            {
                directoryName = directoryName.Substring(0, index);
            }

            // Crear el icono a partir de la imagen del ítem
            Icon icon = CreateIconFromImage(itemImage);

            // Crear la notificación
            NotifyIcon notifyIcon = new NotifyIcon
            {
                Icon = icon,
                Visible = true,
                BalloonTipTitle = "Copia completada",
                BalloonTipText = $"Juego: {directoryName}"
            };

            // Mostrar la notificación
            notifyIcon.ShowBalloonTip(3000);

            // Ocultar el icono después de un tiempo
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 5000 };
            timer.Tick += (s, e) =>
            {
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private static Icon CreateIconFromImage(Image image)
        {
            int iconSize = 64; // Tamaño del icono (puedes ajustarlo según sea necesario)
            Bitmap squareBitmap = new Bitmap(iconSize, iconSize);
            using (Graphics g = Graphics.FromImage(squareBitmap))
            {
                g.Clear(Color.Transparent); // Fondo transparente

                // Calcular el tamaño y la posición de la imagen para mantener la relación de aspecto
                int width, height;
                if (image.Width > image.Height)
                {
                    width = iconSize;
                    height = (int)(image.Height * (iconSize / (float)image.Width));
                }
                else
                {
                    height = iconSize;
                    width = (int)(image.Width * (iconSize / (float)image.Height));
                }

                int x = (iconSize - width) / 2;
                int y = (iconSize - height) / 2;

                g.DrawImage(image, x, y, width, height);
            }

            IntPtr hIcon = squareBitmap.GetHicon();
            return Icon.FromHandle(hIcon);
        }
    }
}
