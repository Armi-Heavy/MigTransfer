using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public class FileCopyManager
{
    public event EventHandler? CopyCompleted;

    private readonly Queue<(string sourceDirectory, string destinationDirectory, ProgressBar progressBar, CheckBox checkBox)> copyQueue = new();
    private bool isCopying;

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
            copyQueue.Dequeue();
        }
        isCopying = false;
        CopyCompleted?.Invoke(this, EventArgs.Empty);
    }

    private async Task CopyFiles(string sourceDirectory, string destinationDirectory, ProgressBar progressBar, CheckBox checkBox)
    {
        var baseDirectoryName = Path.GetFileName(sourceDirectory);
        var baseDirectoryNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceDirectory);
        var switchFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ownCloud", "Switch", baseDirectoryName);

        var files = new[]
        {
            $"{baseDirectoryNameWithoutExtension} (Card ID Set).bin",
            $"{baseDirectoryNameWithoutExtension} (Card UID).bin",
            $"{baseDirectoryNameWithoutExtension} (Certificate).bin",
            $"{baseDirectoryNameWithoutExtension} (Initial Data).bin",
            $"{baseDirectoryNameWithoutExtension}.xci"
        };

        long totalSize = files.Sum(file => new FileInfo(Path.Combine(switchFolderPath, file)).Length);
        var destinationFolder = Path.Combine(destinationDirectory, baseDirectoryName);

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

        long copiedSize = 0;
        foreach (var file in files)
        {
            var sourceFilePath = Path.Combine(switchFolderPath, file);
            var destFile = Path.Combine(destinationFolder, Path.GetFileName(file));
            try
            {
                using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open))
                using (var destStream = new FileStream(destFile, FileMode.Create))
                {
                    var buffer = new byte[81920];
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
                checkBox.Invoke((MethodInvoker)(() => checkBox.Checked = false));
                return;
            }
        }
        progressBar.Invoke((MethodInvoker)(() => progressBar.Value = 100));
        CopyCompleted?.Invoke(this, EventArgs.Empty);
    }
}
