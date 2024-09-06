using MigTransfer;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

public class FileCopyManager
{
    public event EventHandler? CopyCompleted;

    private readonly DriveSpaceManager driveSpaceManager;
    private readonly Panel activeDrivePanel;
    private readonly DriveInfo activeDrive;

    public FileCopyManager(DriveSpaceManager driveSpaceManager, Panel activeDrivePanel, DriveInfo activeDrive)
    {
        this.driveSpaceManager = driveSpaceManager;
        this.activeDrivePanel = activeDrivePanel;
        this.activeDrive = activeDrive;
    }

    public async void CopyFiles(string sourceDirectory, string destinationDirectory, ProgressBar progressBar, CheckBox checkBox)
    {
        var baseDirectoryName = Path.GetFileName(sourceDirectory);
        var baseDirectoryNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceDirectory);
        var switchFolderPath = GlobalSettings.SwitchFolderPath;
        var parentDirectoryPath = Path.Combine(switchFolderPath, baseDirectoryName);

        var files = new[]
        {
            $"{baseDirectoryNameWithoutExtension} (Card ID Set).bin",
            $"{baseDirectoryNameWithoutExtension} (Card UID).bin",
            $"{baseDirectoryNameWithoutExtension} (Certificate).bin",
            $"{baseDirectoryNameWithoutExtension} (Initial Data).bin",
            $"{baseDirectoryNameWithoutExtension}.xci"
        };

        long totalSize = files.Sum(file => new FileInfo(Path.Combine(parentDirectoryPath, file)).Length);
        var destinationFolder = Path.Combine(destinationDirectory, baseDirectoryName);

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

        long copiedSize = 0;
        foreach (var file in files)
        {
            var sourceFilePath = Path.Combine(parentDirectoryPath, file);
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
        driveSpaceManager.UpdateDrivePanel(activeDrive, activeDrivePanel); // Actualizar el texto del espacio disponible
        CopyCompleted?.Invoke(this, EventArgs.Empty);
    }
}