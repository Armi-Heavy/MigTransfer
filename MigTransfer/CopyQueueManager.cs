using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

public class CopyQueueManager
{
    private Queue<(string sourceDirectory, string destinationDirectory, ProgressBar progressBar, CheckBox checkBox)> copyQueue = new();
    private readonly FileCopyManager fileCopyManager;
    private bool isCopying;

    public CopyQueueManager(FileCopyManager fileCopyManager)
    {
        this.fileCopyManager = fileCopyManager;
        this.fileCopyManager.CopyCompleted += OnCopyCompleted;
    }

    public void AddToCopyQueue(string sourceDirectory, string destinationDirectory, ProgressBar progressBar, CheckBox checkBox)
    {
        long requiredSpace = CalculateRequiredSpace(sourceDirectory);
        long totalRequiredSpace = requiredSpace + CalculateTotalSpaceInQueue();
        long availableSpace = GetAvailableSpace(destinationDirectory);

        if (totalRequiredSpace > availableSpace)
        {
            MessageBox.Show("No hay espacio suficiente en el disco activo para este juego.", "Espacio insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            checkBox.Checked = false;
            return;
        }

        copyQueue.Enqueue((sourceDirectory, destinationDirectory, progressBar, checkBox));
        ReorderQueue();
        if (!isCopying)
        {
            StartCopying();
        }
    }

    public void RemoveFromCopyQueue(CheckBox checkBox)
    {
        var item = copyQueue.FirstOrDefault(x => x.checkBox == checkBox);
        if (item != default)
        {
            copyQueue = new Queue<(string sourceDirectory, string destinationDirectory, ProgressBar progressBar, CheckBox checkBox)>(copyQueue.Where(x => x != item));
            ReorderQueue();
        }
    }

    private void StartCopying()
    {
        isCopying = true;
        ProcessNextInQueue();
    }

    private void ProcessNextInQueue()
    {
        if (copyQueue.Count > 0)
        {
            var (sourceDirectory, destinationDirectory, progressBar, checkBox) = copyQueue.Peek();
            fileCopyManager.CopyFiles(sourceDirectory, destinationDirectory, progressBar, checkBox);
        }
        else
        {
            isCopying = false;
        }
    }

    private void OnCopyCompleted(object sender, EventArgs e)
    {
        if (copyQueue.Count > 0)
        {
            var (_, _, _, checkBox) = copyQueue.Dequeue();
            checkBox.Invoke((MethodInvoker)(() =>
            {
                checkBox.Enabled = true;
            }));
        }
        ProcessNextInQueue();
    }

    public int GetQueueIndex(CheckBox checkBox)
    {
        var list = copyQueue.ToList();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].checkBox == checkBox)
            {
                return i; // La posición en la cola es el índice
            }
        }
        return -1; // No encontrado
    }

    public bool HasPendingItems()
    {
        return copyQueue.Count > 0;
    }

    public void CancelAllCopies()
    {
        fileCopyManager.CancelCopy();
        while (copyQueue.Count > 0)
        {
            var (_, _, _, checkBox) = copyQueue.Dequeue();
            checkBox.Invoke((MethodInvoker)(() =>
            {
                checkBox.Checked = false;
                checkBox.Enabled = true;
            }));
        }
    }

    private void ReorderQueue()
    {
        var orderedQueue = copyQueue.OrderBy(item => Path.GetDirectoryName(item.sourceDirectory)).ThenBy(item => item.sourceDirectory).ToList();
        copyQueue = new Queue<(string sourceDirectory, string destinationDirectory, ProgressBar progressBar, CheckBox checkBox)>(orderedQueue);
    }

    private long CalculateRequiredSpace(string sourceDirectory)
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
        return totalSize;
    }

    private long CalculateTotalSpaceInQueue()
    {
        long totalSize = 0;
        foreach (var item in copyQueue)
        {
            totalSize += CalculateRequiredSpace(item.sourceDirectory);
        }
        return totalSize;
    }

    private long GetAvailableSpace(string destinationDirectory)
    {
        var driveInfo = new DriveInfo(Path.GetPathRoot(destinationDirectory));
        return driveInfo.AvailableFreeSpace;
    }
}
