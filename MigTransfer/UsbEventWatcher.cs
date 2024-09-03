using System;
using System.Management;
using System.Windows.Forms;

public class UsbEventWatcher
{
    private readonly ManagementEventWatcher insertWatcher;
    private readonly ManagementEventWatcher removeWatcher;

    public event EventHandler? UsbInserted;
    public event EventHandler? UsbRemoved;

    public UsbEventWatcher()
    {
        try
        {
            insertWatcher = CreateWatcher("__InstanceCreationEvent", OnUsbInserted);
            removeWatcher = CreateWatcher("__InstanceDeletionEvent", OnUsbRemoved);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al configurar los watchers USB: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private ManagementEventWatcher CreateWatcher(string eventType, EventArrivedEventHandler handler)
    {
        var watcher = new ManagementEventWatcher(new WqlEventQuery($"SELECT * FROM {eventType} WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'"));
        watcher.EventArrived += handler;
        watcher.Start();
        return watcher;
    }

    private void OnUsbInserted(object sender, EventArrivedEventArgs e) => UsbInserted?.Invoke(this, EventArgs.Empty);
    private void OnUsbRemoved(object sender, EventArrivedEventArgs e) => UsbRemoved?.Invoke(this, EventArgs.Empty);

    public void Stop()
    {
        try
        {
            insertWatcher.Stop();
            removeWatcher.Stop();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al detener los watchers USB: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
