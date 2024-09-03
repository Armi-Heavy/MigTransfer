using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

public class FileCopyManager
{
    public event EventHandler CopyCompleted; // Evento para notificar cuando la copia se haya completado

    private Queue<(string sourceDirectory, string destinationDirectory, ProgressBar progressBar, CheckBox checkBox)> copyQueue = new Queue<(string, string, ProgressBar, CheckBox)>();
    private bool isCopying = false;

    public async void AddToCopyQueue(string sourceDirectory, string destinationDirectory, ProgressBar progressBar, CheckBox checkBox)
    {
        try
        {
            copyQueue.Enqueue((sourceDirectory, destinationDirectory, progressBar, checkBox));
            if (!isCopying)
            {
                await StartCopying();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al agregar a la cola de copia: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task StartCopying()
    {
        isCopying = true;
        while (copyQueue.Count > 0)
        {
            var (sourceDirectory, destinationDirectory, progressBar, checkBox) = copyQueue.Peek();
            await CopyFiles(sourceDirectory, destinationDirectory, progressBar, checkBox);
            copyQueue.Dequeue(); // Eliminar el elemento de la cola después de copiar
        }
        isCopying = false;
        CopyCompleted?.Invoke(this, EventArgs.Empty); // Notificar que la copia se ha completado
    }

    private async Task CopyFiles(string sourceDirectory, string destinationDirectory, ProgressBar progressBar, CheckBox checkBox)
    {
        string baseDirectoryName = Path.GetFileName(sourceDirectory);
        string baseDirectoryNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceDirectory);
        string userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string switchFolderPath = Path.Combine(userFolderPath, "ownCloud", "Switch", baseDirectoryName);

        string[] files = {
            $"{baseDirectoryNameWithoutExtension} (Card ID Set).bin",
            $"{baseDirectoryNameWithoutExtension} (Card UID).bin",
            $"{baseDirectoryNameWithoutExtension} (Certificate).bin",
            $"{baseDirectoryNameWithoutExtension} (Initial Data).bin",
            $"{baseDirectoryNameWithoutExtension}.xci"
        };

        long totalSize = 0;
        foreach (var file in files)
        {
            string filePath = Path.Combine(switchFolderPath, file);
            if (File.Exists(filePath))
            {
                totalSize += new FileInfo(filePath).Length;
            }
            else
            {
                MessageBox.Show($"El archivo '{filePath}' no existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                checkBox.Invoke((MethodInvoker)(() => checkBox.Checked = false)); // Desmarcar el checkbox
                return;
            }
        }

        string destinationFolder = Path.Combine(destinationDirectory, baseDirectoryName);
        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

        long copiedSize = 0;
        foreach (var file in files)
        {
            string sourceFilePath = Path.Combine(switchFolderPath, file);
            string destFile = Path.Combine(destinationFolder, Path.GetFileName(file));
            try
            {
                using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open))
                using (FileStream destStream = new FileStream(destFile, FileMode.Create))
                {
                    byte[] buffer = new byte[81920];
                    int bytesRead;
                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await destStream.WriteAsync(buffer, 0, bytesRead);
                        copiedSize += bytesRead;
                        progressBar.Invoke((MethodInvoker)(() => progressBar.Value = (int)((double)copiedSize / totalSize * 100)));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al copiar el archivo '{sourceFilePath}': {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                checkBox.Invoke((MethodInvoker)(() => checkBox.Checked = false)); // Desmarcar el checkbox
                return;
            }
        }
        progressBar.Invoke((MethodInvoker)(() => progressBar.Value = 100)); // Asegurar que la barra de progreso llegue al 100%
        CopyCompleted?.Invoke(this, EventArgs.Empty); // Notificar que la copia se ha completado
    }
}
