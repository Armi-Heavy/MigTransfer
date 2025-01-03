using MigTransfer;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public class FileCopyManager
{
    public event EventHandler? CopyCompleted;

    private readonly DriveSpaceManager driveSpaceManager;
    private readonly Panel activeDrivePanel;
    private readonly DriveInfo activeDrive;
    private CancellationTokenSource cancellationTokenSource;

    public FileCopyManager(DriveSpaceManager driveSpaceManager, Panel activeDrivePanel, DriveInfo activeDrive)
    {
        this.driveSpaceManager = driveSpaceManager;
        this.activeDrivePanel = activeDrivePanel;
        this.activeDrive = activeDrive;
        this.cancellationTokenSource = new CancellationTokenSource();
    }

    public async void CopyFiles(string sourceDirectory, string destinationDirectory, ProgressBar progressBar, CheckBox checkBox)
    {
        // Obtener el nombre del directorio padre a partir de sourceDirectory
        string? directoryName = Path.GetFileNameWithoutExtension(sourceDirectory);
        if (directoryName == null)
        {
            MessageBox.Show("Error al obtener el nombre del directorio a partir de la imagen.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Decodificar el nombre del directorio
        directoryName = Uri.UnescapeDataString(directoryName);

        // Construir la URL base para obtener los archivos desde el servidor
        var baseUrl = GlobalSettings.URL_base;

        // Archivos .bin a descargar desde el servidor
        var binFiles = new[] {
            $"{directoryName} (Card ID Set).bin",
            $"{directoryName} (Card UID).bin",
            $"{directoryName} (Certificate).bin",
            $"{directoryName} (Initial Data).bin"
        };

        // Inicializar la variable para el tamaño total de los archivos bin
        long totalSize = 0;

        // Descargar los archivos .bin y calcular el tamaño total
        foreach (var file in binFiles)
        {
            var fileUrl = $"{baseUrl}{directoryName}.xci/{file}";
            fileUrl = GlobalSettings.CleanUrl(fileUrl);

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetAsync(fileUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        // Si no se puede obtener Content-Length, no hacer nada
                        totalSize += response.Content.Headers.ContentLength.GetValueOrDefault();
                    }
                    else
                    {
                        MessageBox.Show($"Error al obtener el archivo '{fileUrl}'", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        checkBox.Invoke((MethodInvoker)(() => checkBox.Checked = false));
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al obtener la información del archivo '{fileUrl}': {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    checkBox.Invoke((MethodInvoker)(() => checkBox.Checked = false));
                    return;
                }
            }
        }

        // Crear el directorio de destino en el disco activo
        var destinationFolder = Path.Combine(destinationDirectory, directoryName);
        destinationFolder = Uri.UnescapeDataString(destinationFolder);
        destinationFolder += ".xci";

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

        long copiedSize = 0;

        // Descargar los archivos .bin
        foreach (var file in binFiles)
        {
            var sourceFileUrl = $"{baseUrl}{directoryName}.xci/{file}";
            sourceFileUrl = GlobalSettings.CleanUrl(sourceFileUrl);

            var decodedFileName = Uri.UnescapeDataString(Path.GetFileName(file));
            var destFile = Path.Combine(destinationFolder, decodedFileName);

            using (var httpClient = new HttpClient())
            {
                try
                {
                    using (var sourceStream = await httpClient.GetStreamAsync(sourceFileUrl))
                    using (var destStream = new FileStream(destFile, FileMode.Create))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await destStream.WriteAsync(buffer, 0, bytesRead);
                            copiedSize += bytesRead;

                            // Calcular el progreso sin depender de Content-Length
                            progressBar.Invoke((MethodInvoker)(() => progressBar.Value = (int)((double)copiedSize / totalSize * 100)));
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show($"La descarga del archivo '{sourceFileUrl}' fue cancelada.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    checkBox.Invoke((MethodInvoker)(() => checkBox.Checked = false));
                    return;
                }
            }
        }

        // Descargar el archivo .xci
        var xciFile = $"{directoryName}.xci";
        var xciFileUrl = $"{baseUrl}{directoryName}.xci/{xciFile}";
        xciFileUrl = GlobalSettings.CleanUrl(xciFileUrl);

        var decodedXciFileName = Uri.UnescapeDataString(xciFile);
        var destinationXciFile = Path.Combine(destinationFolder, decodedXciFileName);

        using (var httpClient = new HttpClient())
        {
            using (var sourceStream = await httpClient.GetStreamAsync(xciFileUrl))
            using (var destStream = new FileStream(destinationXciFile, FileMode.Create))
            {
                var buffer = new byte[8192];
                int bytesRead;
                long xciFileSize = 0;
                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await destStream.WriteAsync(buffer, 0, bytesRead);
                    xciFileSize += bytesRead;
                }

                progressBar.Invoke((MethodInvoker)(() => progressBar.Value = 100));
            }
        }

        // Actualizar el espacio disponible del disco
        driveSpaceManager.UpdateDrivePanel(activeDrive, activeDrivePanel);

        // Invocar el evento de finalización
        CopyCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void CancelCopy()
    {
        cancellationTokenSource.Cancel();
    }
}
