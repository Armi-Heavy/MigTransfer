using System;
using System.Drawing;
using System.Runtime.InteropServices;

public static class SystemIconsHelper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

    public const uint SHGFI_ICON = 0x000000100;
    public const uint SHGFI_LARGEICON = 0x000000000; // Large icon
    public const uint SHGFI_SMALLICON = 0x000000001; // Small icon

    public static Icon GetSystemIcon(string path)
    {
        SHFILEINFO shinfo = new SHFILEINFO();
        IntPtr hImgSmall = SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON);

        if (shinfo.hIcon != IntPtr.Zero)
        {
            return Icon.FromHandle(shinfo.hIcon);
        }
        return null;
    }
}
