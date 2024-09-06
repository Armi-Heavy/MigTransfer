using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MigTransfer
{
    public static class NotificationManager
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

            // Crear la notificación con el icono por defecto
            NotifyIcon notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // Icono por defecto
                Visible = true,
                BalloonTipTitle = "Juego Añadido correctamente",
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

        public static void ShowNotification(string message)
        {
            var notification = new Form
            {
                Size = new Size(300, 100),
                StartPosition = FormStartPosition.Manual,
                Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - 310, Screen.PrimaryScreen.WorkingArea.Height - 110),
                FormBorderStyle = FormBorderStyle.None,
                TopMost = true,
                BackColor = Color.White
            };

            var label = new Label
            {
                Text = message,
                AutoSize = true,
                Location = new Point(10, 40)
            };

            notification.Controls.Add(label);

            notification.Show();

            var timer = new System.Windows.Forms.Timer { Interval = 3000 };
            timer.Tick += (s, e) =>
            {
                notification.Close();
                timer.Stop();
            };
            timer.Start();
        }
    }
}