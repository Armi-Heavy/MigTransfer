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
}