using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace MigTransfer
{
    public class ImageItem : UserControl
    {
        private PictureBox pictureBox;
        private CheckBox checkBox;
        private ProgressBar progressBar; // Añadir esta línea
        private Image originalImage;
        private string imagePath; // Añadir esta línea
        private Form1 form; // Añadir esta línea

        public ImageItem(Image image, string imagePath, Form1 form) // Modificar el constructor
        {
            this.imagePath = imagePath; // Añadir esta línea
            this.form = form; // Añadir esta línea
            originalImage = image;
            InitializeComponents(image);
        }

        private void InitializeComponents(Image image)
        {
            this.Size = new Size(256, 414);

            pictureBox = new PictureBox
            {
                Image = image,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Dock = DockStyle.Fill,
                Margin = new Padding(10)
            };

            checkBox = new CheckBox
            {
                Location = new Point(5, 5),
                AutoSize = true,
                Visible = false
            };

            progressBar = new ProgressBar // Añadir esta sección
            {
                Location = new Point(10, this.Height - 30),
                Width = this.Width - 20,
                Height = 20,
                Visible = false
            };

            this.Controls.Add(pictureBox);
            this.Controls.Add(checkBox);
            this.Controls.Add(progressBar); // Añadir esta línea

            checkBox.BringToFront();

            pictureBox.MouseEnter += (s, e) => checkBox.Visible = true;
            pictureBox.MouseLeave += (s, e) => { if (!checkBox.Checked) checkBox.Visible = false; };
            checkBox.MouseEnter += (s, e) => checkBox.Visible = true;
            checkBox.MouseLeave += (s, e) => { if (!checkBox.Checked) checkBox.Visible = false; };

            pictureBox.Click += (s, e) => checkBox.Checked = !checkBox.Checked;

            checkBox.CheckedChanged += (s, e) =>
            {
                if (checkBox.Checked)
                {
                    DriveInfo activeDrive = form.GetActiveDrive(); // Obtener el disco activo
                    if (activeDrive == null)
                    {
                        MessageBox.Show("Por favor, seleccione un dispositivo donde copiar los archivos antes de continuar.", "Dispositivo no seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        checkBox.Checked = false; // Desmarcar el checkbox
                        return;
                    }

                    pictureBox.Image = ChangeImageBrightness(originalImage, -0.5f);
                    progressBar.Visible = true; // Mostrar la barra de progreso
                    progressBar.BringToFront(); // Traer la barra de progreso al frente

                    string directoryName = Path.GetFileName(Path.GetDirectoryName(imagePath));
                    string destinationDirectory = Path.Combine(activeDrive.RootDirectory.FullName, directoryName);

                    FileCopyManager fileCopyManager = new FileCopyManager();
                    fileCopyManager.AddToCopyQueue(directoryName, destinationDirectory, progressBar, checkBox);
                }
                else
                {
                    pictureBox.Image = originalImage;
                    progressBar.Visible = false; // Ocultar la barra de progreso
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
