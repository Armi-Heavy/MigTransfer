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
        private ProgressBar progressBar;
        private Image originalImage;
        private string imagePath;
        private Form1 form;
        private bool fromComparison = false;

        public string ImagePath => imagePath; // Propiedad para acceder a imagePath

        public ImageItem(Image image, string imagePath, Form1 form)
        {
            this.imagePath = imagePath;
            this.form = form;
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

            progressBar = new ProgressBar
            {
                Location = new Point(10, this.Height - 30),
                Width = this.Width - 20,
                Height = 20,
                Visible = false
            };

            this.Controls.Add(pictureBox);
            this.Controls.Add(checkBox);
            this.Controls.Add(progressBar);

            checkBox.BringToFront();

            pictureBox.MouseEnter += (s, e) => checkBox.Visible = true;
            pictureBox.MouseLeave += (s, e) => { if (!checkBox.Checked) checkBox.Visible = false; };
            checkBox.MouseEnter += (s, e) => checkBox.Visible = true;
            checkBox.MouseLeave += (s, e) => { if (!checkBox.Checked) checkBox.Visible = false; };

            pictureBox.Click += (s, e) => checkBox.Checked = !checkBox.Checked;

            checkBox.CheckedChanged += async (s, e) =>
            {
                if (checkBox.Checked)
                {
                    if (fromComparison) return;

                    DriveInfo activeDrive = form.GetActiveDrive();
                    if (activeDrive == null)
                    {
                        MessageBox.Show("Por favor, seleccione un dispositivo donde copiar los archivos antes de continuar.", "Dispositivo no seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        checkBox.Checked = false;
                        return;
                    }

                    pictureBox.Image = ChangeImageBrightness(originalImage, -0.5f);
                    progressBar.Visible = true;
                    progressBar.BringToFront();

                    string directoryName = Path.GetFileName(Path.GetDirectoryName(imagePath));
                    string destinationDirectory = activeDrive.RootDirectory.FullName;

                    FileCopyManager fileCopyManager = new FileCopyManager();
                    fileCopyManager.AddToCopyQueue(directoryName, destinationDirectory, progressBar, checkBox);
                }
                else
                {
                    pictureBox.Image = originalImage;
                    progressBar.Visible = false;

                    DriveInfo activeDrive = form.GetActiveDrive();
                    if (activeDrive != null)
                    {
                        string directoryName = Path.GetFileName(Path.GetDirectoryName(imagePath));
                        string destinationDirectory = Path.Combine(activeDrive.RootDirectory.FullName, directoryName);

                        if (Directory.Exists(destinationDirectory))
                        {
                            try
                            {
                                Directory.Delete(destinationDirectory, true);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error al borrar el directorio '{destinationDirectory}': {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            };
        }

        public void SetCheckBoxChecked(bool isChecked, bool fromComparison = false)
        {
            this.fromComparison = fromComparison;
            checkBox.CheckedChanged -= CheckBox_CheckedChanged;
            checkBox.Checked = isChecked;
            if (isChecked && fromComparison)
            {
                progressBar.Visible = true;
                progressBar.Value = 100;
                progressBar.BringToFront();
                pictureBox.Image = ChangeImageBrightness(originalImage, -0.5f);
            }
            checkBox.CheckedChanged += CheckBox_CheckedChanged;
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Implementar la lógica de CheckedChanged aquí si es necesario
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
