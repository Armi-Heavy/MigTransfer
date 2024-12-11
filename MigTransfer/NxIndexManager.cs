using System;
using System.IO;
using System.Windows.Forms;

public static class NxIndexManager
{
    public static void RemoveNxIndex(string rootDirectory)
    {
        try
        {
            string nxIndexFilePath = Path.Combine(rootDirectory, ".nxindex");

            // Verificar si el archivo existe
            if (File.Exists(nxIndexFilePath))
            {
                // Eliminar el archivo
                File.Delete(nxIndexFilePath);
                Console.WriteLine($"Archivo .nxindex eliminado de {rootDirectory}");
            }
            else
            {
                Console.WriteLine($"El archivo .nxindex no existe en {rootDirectory}");
            }
        }
        catch (Exception ex)
        {
            // Si hay un error al eliminar el archivo, mostrar un mensaje de error
            MessageBox.Show($"Error al eliminar .nxindex: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
