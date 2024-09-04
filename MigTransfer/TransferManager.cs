using System;
using System.IO;
using System.Windows.Forms;

namespace MigTransfer
{
    public class TransferManager
    {
        private readonly FileCopyManager fileCopyManager;
        public bool IsClosing { get; private set; } = false;

        public TransferManager(FileCopyManager fileCopyManager)
        {
            this.fileCopyManager = fileCopyManager;
        }

        public void VerifyAndCloseApplication(Form form)
        {
            if (IsClosing) return;

            if (fileCopyManager.IsCopying)
            {
                var result = MessageBox.Show("Hay una transferencia en curso. Si cierras la aplicación, la transferencia no se completará. ¿Deseas continuar?", "Confirmación de cierre", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    CancelCurrentTransfer();
                    IsClosing = true;
                    form.Close();
                }
                else
                {
                    IsClosing = false;
                }
            }
            else
            {
                IsClosing = true;
                form.Close();
            }
        }

        private void CancelCurrentTransfer()
        {
            while (fileCopyManager.CopyQueue.Count > 0)
            {
                var (sourceDirectory, destinationDirectory, progressBar, checkBox) = fileCopyManager.CopyQueue.Dequeue();
                UpdateProgressBar(progressBar, 0);
                UpdateCheckBox(checkBox, false);

                // Eliminar los archivos y directorios copiados
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
            fileCopyManager.IsCopying = false;
        }

        private void UpdateProgressBar(ProgressBar progressBar, int value)
        {
            if (progressBar.IsHandleCreated)
            {
                progressBar.Invoke((MethodInvoker)(() => progressBar.Value = value));
            }
        }

        private void UpdateCheckBox(CheckBox checkBox, bool isChecked)
        {
            if (checkBox.IsHandleCreated)
            {
                checkBox.Invoke((MethodInvoker)(() => checkBox.Checked = isChecked));
            }
        }
    }
}
