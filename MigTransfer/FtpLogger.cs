using System;
using System.IO;

public static class FtpLogger
{
    private static readonly string logFilePath = Path.Combine(Application.StartupPath, "ftplog.txt");

    // Función para registrar información en el log
    public static void Log(string message)
    {
        try
        {
            string logMessage = $"{DateTime.Now}: {message}";
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
        catch (Exception ex)
        {
            // Si no se puede escribir en el archivo, podemos registrar el error en la consola o mostrarlo.
            Console.WriteLine($"Error escribiendo en el log: {ex.Message}");
        }
    }
}
