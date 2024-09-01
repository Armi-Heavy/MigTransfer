using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace MigTransfer
{
    public class ImageItem : UserControl
    {
        private PictureBox pictureBox;
        private CheckBox checkBox;
        private Image originalImage;

        public ImageItem(Image image)
        {
            originalImage = image;
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
                AutoSize = true, // Asegurar que la casilla se ajuste a su contenido
                Visible = false // Inicialmente invisible
            };

            this.Controls.Add(pictureBox);
            this.Controls.Add(checkBox);

            // Asegurarse de que el CheckBox esté al frente
            checkBox.BringToFront();

            // Agregar eventos para mostrar/ocultar la casilla de verificación
            pictureBox.MouseEnter += (s, e) => checkBox.Visible = true;
            pictureBox.MouseLeave += (s, e) => { if (!checkBox.Checked) checkBox.Visible = false; };
            checkBox.MouseEnter += (s, e) => checkBox.Visible = true;
            checkBox.MouseLeave += (s, e) => { if (!checkBox.Checked) checkBox.Visible = false; };

            // Agregar evento para marcar la casilla al hacer clic en la imagen
            pictureBox.Click += (s, e) => checkBox.Checked = !checkBox.Checked;

            // Agregar evento para oscurecer la imagen cuando se marque la casilla
            checkBox.CheckedChanged += (s, e) =>
            {
                if (checkBox.Checked)
                {
                    pictureBox.Image = ChangeImageBrightness(originalImage, -0.5f);
                }
                else
                {
                    pictureBox.Image = originalImage;
                }
            };
        }

        private Image ChangeImageBrightness(Image image, float brightness)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                float[][] ptsArray = {
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {brightness, brightness, brightness, 0, 2}
                };

                ColorMatrix clrMatrix = new ColorMatrix(ptsArray);
                ImageAttributes imgAttributes = new ImageAttributes();
                imgAttributes.SetColorMatrix(clrMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imgAttributes);
            }
            return bmp;
        }
    }
}
