using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MigTransfer
{
    public class NotificationManager
    {
        public static void ShowNotification(string imagePath, string directoryPath)
        {
            // Obtener el nombre del directorio padre truncado hasta antes del carácter '['
            string directoryName = Path.GetFileName(directoryPath);
            int index = directoryName.IndexOf('[');
            if (index > 0)
            {
                directoryName = directoryName.Substring(0, index);
            }

            // Crear el icono a partir de la imagen
            Icon icon = CreateIconFromImage(imagePath);

            // Crear la notificación
            NotifyIcon notifyIcon = new NotifyIcon
            {
                Icon = icon,
                Visible = true,
                BalloonTipTitle = "Copia completada",
                BalloonTipText = $"Directorio: {directoryName}"
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

        private static Icon CreateIconFromImage(string imagePath)
        {
            using (Bitmap bitmap = new Bitmap(imagePath))
            {
                IntPtr hIcon = bitmap.GetHicon();
                return Icon.FromHandle(hIcon);
            }
        }
    }
}
