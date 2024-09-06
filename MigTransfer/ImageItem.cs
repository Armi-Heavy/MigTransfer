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
        private PictureBox pictureBox = null!;
        private CheckBox checkBox = null!;
        private ProgressBar progressBar = null!;
        private Image? originalImage; // Permitir valores NULL
        private string imagePath;
        private Form1 form;
        private DriveSpaceManager driveSpaceManager;
        private bool fromComparison = false;
        private readonly CopyQueueManager copyQueueManager;

        public string ImagePath => imagePath; // Propiedad para acceder a imagePath

        public ImageItem(string imagePath, Form1 form, CopyQueueManager copyQueueManager)
        {
            this.imagePath = imagePath;
            this.form = form;
            this.copyQueueManager = copyQueueManager;
            this.driveSpaceManager = new DriveSpaceManager(form);
            InitializeComponents();
            _ = LoadImageAsync(imagePath); // Ignorar advertencia de método asincrónico sin await
        }

        private void InitializeComponents()
        {
            this.Size = new Size(256, 414);

            pictureBox = new PictureBox
            {
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

            pictureBox.MouseEnter += (s, e) => checkBox.Visible = false;
            pictureBox.MouseLeave += (s, e) => { if (!checkBox.Checked) checkBox.Visible = false; };
            checkBox.MouseEnter += (s, e) => checkBox.Visible = false;
            checkBox.MouseLeave += (s, e) => { if (!checkBox.Checked) checkBox.Visible = false; };

            pictureBox.Click += (s, e) => ToggleCheckBox();

            checkBox.CheckedChanged += async (s, e) =>
            {
                if (checkBox.Checked)
                {
                    if (fromComparison) return;

                    DriveInfo? activeDrive = form.GetActiveDrive();
                    if (activeDrive == null)
                    {
                        MessageBox.Show("Por favor, seleccione un dispositivo donde copiar los archivos antes de continuar.", "Dispositivo no seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        checkBox.Checked = false;
                        return;
                    }

                    pictureBox.Image = ChangeImageBrightness(originalImage, -0.5f);
                    progressBar.Visible = true;
                    progressBar.BringToFront();

                    string? directoryName = Path.GetFileName(Path.GetDirectoryName(imagePath));
                    if (directoryName == null)
                    {
                        MessageBox.Show("Error al obtener el nombre del directorio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        checkBox.Checked = false;
                        return;
                    }

                    string destinationDirectory = activeDrive.RootDirectory.FullName;

                    copyQueueManager.AddToCopyQueue(directoryName, destinationDirectory, progressBar, checkBox);

                    // Bloquear el CheckBox solo si su índice en la cola es 0
                    if (copyQueueManager.GetQueueIndex(checkBox) == 0)
                    {
                        checkBox.Enabled = false;
                        pictureBox.Enabled = false;
                    }
                }
                else
                {
                    // Permitir desmarcar si no es el primero en la cola
                    if (copyQueueManager.GetQueueIndex(checkBox) == 0)
                    {
                        checkBox.Checked = true; // No permitir desmarcar si es el primero en la cola
                        return;
                    }

                    pictureBox.Image = originalImage;
                    progressBar.Visible = false;

                    DriveInfo? activeDrive = form.GetActiveDrive();
                    if (activeDrive != null)
                    {
                        string? directoryName = Path.GetFileName(Path.GetDirectoryName(imagePath));
                        if (directoryName == null)
                        {
                            MessageBox.Show("Error al obtener el nombre del directorio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

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

                        // Actualizar la barra de progreso y el texto del tamaño actual
                        driveSpaceManager.UpdateDrivePanel(activeDrive, form.activeDrivePanel);
                    }

                    // Restablecer fromComparison después de desmarcar
                    fromComparison = false;

                    // Eliminar de la cola
                    copyQueueManager.RemoveFromCopyQueue(checkBox);
                }
            };
        }

        private void ToggleCheckBox()
        {
            checkBox.Checked = !checkBox.Checked;
        }

        private async Task LoadImageAsync(string imagePath)
        {
            try
            {
                using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    originalImage = await Task.Run(() => Image.FromStream(stream)); // Usar await Task.Run para ejecutar en segundo plano
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
                if (progressBar.InvokeRequired)
                {
                    progressBar.Invoke((MethodInvoker)(() => progressBar.Visible = false));
                }
                else
                {
                    progressBar.Visible = false;
                }
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

            Bitmap bmp = new Bitmap(image.Width, image.Height);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                float[][] ptsArray = {
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {brightness, brightness, brightness, 0, 1}
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