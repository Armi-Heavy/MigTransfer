using System;
using System.Management;
using System.Windows.Forms;
using System.IO;
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
            // Crear watchers para insertar y quitar dispositivos USB
            insertWatcher = CreateWatcher("__InstanceCreationEvent", OnUsbInserted);
            removeWatcher = CreateWatcher("__InstanceDeletionEvent", OnUsbRemoved);
        }
        catch (Exception ex)
        {
            // Si hay un error al crear los watchers, lo mostramos pero sin interrumpir la app
            Console.WriteLine($"Error al configurar los watchers USB: {ex.Message}");
        }
    }

    // Crear un watcher para detectar inserción o eliminación
    private ManagementEventWatcher CreateWatcher(string eventType, EventArrivedEventHandler handler)
    {
        var watcher = new ManagementEventWatcher(new WqlEventQuery($"SELECT * FROM {eventType} WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'"));
        watcher.EventArrived += handler;
        watcher.Start();
        return watcher;
    }

    // Evento cuando se inserta un USB
    private void OnUsbInserted(object sender, EventArrivedEventArgs e)
    {
        // Actualizar las unidades ExFAT disponibles
        UpdateExFatDrives();
    }

    // Evento cuando se quita un USB
    private void OnUsbRemoved(object sender, EventArrivedEventArgs e)
    {
        // Actualizar las unidades ExFAT disponibles sin mostrar mensaje
        UpdateExFatDrives();
    }

    // Actualiza la lista de unidades ExFAT en la interfaz
    private void UpdateExFatDrives()
    {
        flowLayoutPanel.Invoke((MethodInvoker)(() =>
        {
            // Limpiar el flujo actual de dispositivos
            flowLayoutPanel.Controls.Clear();

            // Obtener todas las unidades ExFAT disponibles
            var drives = exFatDriveDetector.GetExFatDrives();

            // Verificar si hay unidades ExFAT disponibles
            if (drives.Any())
            {
                // Si hay unidades, agregarlas al flujo
                foreach (var drive in drives)
                {
                    var panel = exFatDriveDetector.CreateDrivePanel(drive, flowLayoutPanel.Width);
                    panel.Click += (s, e) => form.SetActiveDrive(drive, panel);
                    foreach (Control control in panel.Controls)
                    {
                        control.Click += (s, e) => form.SetActiveDrive(drive, panel);
                    }
                    flowLayoutPanel.Controls.Add(panel);
                }
            }
        }));
    }

    // Detener los watchers
    public void Stop()
    {
        try
        {
            insertWatcher.Stop();
            removeWatcher.Stop();
        }
        catch (Exception ex)
        {
            // Si ocurre un error al detener, lo mostramos solo en consola
            Console.WriteLine($"Error al detener los watchers USB: {ex.Message}");
        }
    }
}
