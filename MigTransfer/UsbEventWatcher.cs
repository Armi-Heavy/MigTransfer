using System;
using System.Management;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using MigTransfer;

public class UsbEventWatcher
{
    private readonly ManagementEventWatcher insertWatcher;
    private readonly ManagementEventWatcher removeWatcher;
    private readonly FlowLayoutPanel flowLayoutPanel;
    private readonly ExFatDriveDetector exFatDriveDetector;
    private readonly Form1 form;

    public UsbEventWatcher(FlowLayoutPanel flowLayoutPanel, Form1 form)
    {
        this.flowLayoutPanel = flowLayoutPanel;
        this.exFatDriveDetector = new ExFatDriveDetector();
        this.form = form;

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

    private void OnUsbInserted(object sender, EventArrivedEventArgs e)
    {
        UpdateExFatDrives();
    }

    private void OnUsbRemoved(object sender, EventArrivedEventArgs e)
    {
        UpdateExFatDrives();
        form.Invoke((MethodInvoker)(() => form.UncheckAllItems()));
    }

    private void UpdateExFatDrives()
    {
        flowLayoutPanel.Invoke((MethodInvoker)(() =>
        {
            flowLayoutPanel.Controls.Clear();
            foreach (var drive in exFatDriveDetector.GetExFatDrives())
            {
                var panel = exFatDriveDetector.CreateDrivePanel(drive, flowLayoutPanel.Width);
                panel.Click += (s, e) => form.SetActiveDrive(drive, panel);
                foreach (Control control in panel.Controls)
                {
                    control.Click += (s, e) => form.SetActiveDrive(drive, panel);
                }
                flowLayoutPanel.Controls.Add(panel);
            }
        }));
    }

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