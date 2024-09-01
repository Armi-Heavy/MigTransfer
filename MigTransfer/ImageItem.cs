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
            originalImage = image ?? throw new ArgumentNullException(nameof(image));
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(256, 414); // Establecer el tamaño del UserControl
            this.BorderStyle = BorderStyle.FixedSingle; // Agregar un borde para visualizar mejor

            pictureBox = new PictureBox
            {
                Image = originalImage,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Dock = DockStyle.Fill, // Ajustar el PictureBox al tamaño del UserControl
                Margin = new Padding(10)
            };

            checkBox = new CheckBox
            {
                Location = new Point(5, 5),
                Visible = false
            };

            this.Controls.Add(pictureBox);
            this.Controls.Add(checkBox);

            pictureBox.MouseEnter += PictureBox_MouseEnter;
            pictureBox.MouseLeave += PictureBox_MouseLeave;
            checkBox.CheckedChanged += CheckBox_CheckedChanged;
        }

        private void PictureBox_MouseEnter(object? sender, EventArgs e)
        {
            checkBox.Visible = true;
        }

        private void PictureBox_MouseLeave(object? sender, EventArgs e)
        {
            if (!checkBox.Checked)
            {
                checkBox.Visible = false;
            }
        }

        private void CheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (checkBox.Checked)
            {
                pictureBox.Image = ChangeImageOpacity(originalImage, 0.5f);
            }
            else
            {
                pictureBox.Image = ChangeImageOpacity(originalImage, 1.0f);
            }
        }

        private Image ChangeImageOpacity(Image image, float opacity)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = opacity;
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return bmp;
        }
    }
}
