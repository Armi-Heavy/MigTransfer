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
        copyQueue.Enqueue((sourceDirectory, destinationDirectory, progressBar, checkBox));
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
}
