using System;
using System.Management;

public class UsbEventWatcher
{
    private ManagementEventWatcher insertWatcher;
    private ManagementEventWatcher removeWatcher;

    public event EventHandler UsbInserted;
    public event EventHandler UsbRemoved;

    public UsbEventWatcher()
    {
        // Configurar el watcher para detectar la inserción de unidades
        insertWatcher = new ManagementEventWatcher();
        insertWatcher.Query = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
        insertWatcher.EventArrived += new EventArrivedEventHandler(OnUsbInserted);
        insertWatcher.Start();

        // Configurar el watcher para detectar la eliminación de unidades
        removeWatcher = new ManagementEventWatcher();
        removeWatcher.Query = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
        removeWatcher.EventArrived += new EventArrivedEventHandler(OnUsbRemoved);
        removeWatcher.Start();
    }

    private void OnUsbInserted(object sender, EventArrivedEventArgs e)
    {
        UsbInserted?.Invoke(this, EventArgs.Empty);
    }

    private void OnUsbRemoved(object sender, EventArrivedEventArgs e)
    {
        UsbRemoved?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {
        insertWatcher.Stop();
        removeWatcher.Stop();
    }
}
