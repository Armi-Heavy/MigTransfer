using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

public class FileCopyManager
{
    private Queue<string> copyQueue = new Queue<string>();
    private bool isCopying = false;

    public async void AddToCopyQueue(string sourceDirectory, string destinationDirectory, ProgressBar progressBar, CheckBox checkBox)
    {
        copyQueue.Enqueue(sourceDirectory);
        if (!isCopying)
        {
            await StartCopying(destinationDirectory, progressBar, checkBox);
        }
    }

    private async Task StartCopying(string destinationDirectory, ProgressBar progressBar, CheckBox checkBox)
    {
        isCopying = true;
        while (copyQueue.Count > 0)
        {
            string sourceDirectory = copyQueue.Dequeue();
            await CopyFiles(sourceDirectory, destinationDirectory, progressBar, checkBox);
        }
        isCopying = false;
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
        Directory.CreateDirectory(destinationFolder);

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
    }
}
