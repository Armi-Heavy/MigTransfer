using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MigTransfer
{
    public class ImageItem : UserControl
    {
        private readonly PictureBox pictureBox = new PictureBox { SizeMode = PictureBoxSizeMode.StretchImage, Dock = DockStyle.Fill, Margin = new Padding(10) };
        private readonly CheckBox checkBox = new CheckBox { Location = new Point(5, 5), AutoSize = true, Visible = false };
        private readonly ProgressBar progressBar = new ProgressBar { Location = new Point(10, 384), Width = 236, Height = 20, Visible = false };
        private readonly string imagePath;
        private readonly Form1 form;
        private Image? originalImage;
        private bool fromComparison;
        private bool isCopying;

        public string ImagePath => imagePath;

        public ImageItem(string imagePath, Form1 form)
        {
            this.imagePath = imagePath;
            this.form = form;
            InitializeComponents();
            _ = LoadImageAsync(imagePath);
        }

        private void InitializeComponents()
        {
            this.Size = new Size(256, 414);
            this.Controls.AddRange(new Control[] { pictureBox, checkBox, progressBar });
            checkBox.BringToFront();

            pictureBox.MouseEnter += (s, e) => checkBox.Visible = true;
            pictureBox.MouseLeave += (s, e) => { if (!checkBox.Checked) checkBox.Visible = false; };
            checkBox.MouseEnter += (s, e) => checkBox.Visible = true;
            checkBox.MouseLeave += (s, e) => { if (!checkBox.Checked) checkBox.Visible = false; };
            pictureBox.Click += (s, e) => ToggleCheckBox();

            checkBox.CheckedChanged += async (s, e) =>
            {
                if (checkBox.Checked)
                {
                    if (fromComparison) return;

                    var activeDrive = form.GetActiveDrive();
                    if (activeDrive == null)
                    {
                        MessageBox.Show("Por favor, seleccione un dispositivo donde copiar los archivos antes de continuar.", "Dispositivo no seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        checkBox.Checked = false;
                        return;
                    }

                    pictureBox.Image = ChangeImageBrightness(originalImage, -0.5f);
                    progressBar.Visible = true;
                    progressBar.BringToFront();
                    checkBox.Enabled = pictureBox.Enabled = false;
                    isCopying = true;

                    var directoryName = Path.GetFileName(Path.GetDirectoryName(imagePath));
                    if (directoryName == null)
                    {
                        MessageBox.Show("Error al obtener el nombre del directorio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        checkBox.Checked = false;
                        return;
                    }

                    var destinationDirectory = activeDrive.RootDirectory.FullName;
                    var fileCopyManager = new FileCopyManager();
                    fileCopyManager.CopyCompleted += OnCopyCompleted;
                    await Task.Run(() => fileCopyManager.AddToCopyQueue(directoryName, destinationDirectory, progressBar, checkBox));
                }
                else
                {
                    if (isCopying)
                    {
                        checkBox.Checked = true;
                        return;
                    }

                    pictureBox.Image = originalImage;
                    progressBar.Visible = false;

                    var activeDrive = form.GetActiveDrive();
                    if (activeDrive != null)
                    {
                        var directoryName = Path.GetFileName(Path.GetDirectoryName(imagePath));
                        if (directoryName == null)
                        {
                            MessageBox.Show("Error al obtener el nombre del directorio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        var destinationDirectory = Path.Combine(activeDrive.RootDirectory.FullName, directoryName);
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

        private void ToggleCheckBox()
        {
            if (!isCopying)
            {
                checkBox.Checked = !checkBox.Checked;
            }
        }

        private async Task LoadImageAsync(string imagePath)
        {
            try
            {
                using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    originalImage = await Task.Run(() => Image.FromStream(stream));
                }
                pictureBox.Image = originalImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la imagen: {imagePath}\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            else if (!isChecked)
            {
                progressBar.Visible = false;
                pictureBox.Image = originalImage;
            }
            checkBox.CheckedChanged += CheckBox_CheckedChanged;
        }

        private void CheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            // Implementar la lógica de CheckedChanged aquí si es necesario
        }

        private Image ChangeImageBrightness(Image? image, float brightness)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            var bmp = new Bitmap(image.Width, image.Height);
            using (var gfx = Graphics.FromImage(bmp))
            {
                var clrMatrix = new ColorMatrix(new float[][]
                {
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {brightness, brightness, brightness, 0, 1}
                });

                var imgAttributes = new ImageAttributes();
                imgAttributes.SetColorMatrix(clrMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imgAttributes);
            }
            return bmp;
        }

        private void OnCopyCompleted(object? sender, EventArgs e)
        {
            checkBox.Invoke((MethodInvoker)(() => checkBox.Enabled = true));
            pictureBox.Invoke((MethodInvoker)(() => pictureBox.Enabled = true));
            isCopying = false;
        }
    }
}
