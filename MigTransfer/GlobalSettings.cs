using System;
using System.IO;

public static class GlobalSettings
{
    public static readonly string SwitchFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ownCloud", "Switch");
}