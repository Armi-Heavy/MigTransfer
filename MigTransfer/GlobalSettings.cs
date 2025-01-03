using System;
using System.IO;

public static class GlobalSettings
{
    public static readonly string SwitchFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ownCloud", "Switch");

    // FTP configuration
    //public static readonly string FtpServer = "armiche.tplinkdns.com";
    //public static readonly string FtpUsername = "nintendo";
    //public static readonly string FtpPassword = "switch";
    //public static readonly string FtpPort = "21";
    //public static readonly string FtpDirectory = "/media/Switch/Juegos/";
    //public static readonly string FtpDirectory = "/media/Switch/Cartuchos/";

    //HTTP Configuration
    public static readonly string URL_base = "http://armiche.tplinkdns.com:9000/";

    // Función para limpiar la URL
    public static string CleanUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return url;

        // Primero decodificamos la URL para asegurarnos de que cualquier carácter codificado esté en su forma original.
        string decodedUrl = Uri.UnescapeDataString(url);

        // Reemplazar espacios por %20 y &amp por &
        string cleanedUrl = decodedUrl.Replace(" ", "%20").Replace("&amp;", "&");

        return cleanedUrl;
    }
}