using System;
using System.Drawing;
using System.Windows.Forms;

namespace MigTransfer
{
    public class ImageItem : UserControl
    {
        private PictureBox pictureBox;
        private CheckBox checkBox;

        public ImageItem(Image image)
        {
            InitializeComponents(image);
        }

        private void InitializeComponents(Image image)
        {
            this.Size = new Size(256, 414); // Establecer el tamaño del UserControl

            pictureBox = new PictureBox
            {
                Image = image,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Dock = DockStyle.Fill, // Ajustar el PictureBox al tamaño del UserControl
                Margin = new Padding(10)
            };

            checkBox = new CheckBox
            {
                Location = new Point(5, 5),
                BackColor = Color.Transparent, // Asegurar que el fondo sea transparente
                AutoSize = true // Asegurar que la casilla se ajuste a su contenido
            };

            this.Controls.Add(pictureBox);
            this.Controls.Add(checkBox);

            // Asegurarse de que el CheckBox esté al frente
            checkBox.BringToFront();
        }
    }
}
