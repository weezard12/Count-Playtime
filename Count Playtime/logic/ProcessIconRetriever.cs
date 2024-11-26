using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Count_Playtime.logic
{
    internal class ProcessIconRetriever
    {
        // Struct for SHFILEINFO to hold icon data
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        // Flags for SHGetFileInfo
        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x000000000; // Large icon
        private const uint SHGFI_SMALLICON = 0x000000001; // Small icon

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// Retrieves the icon of the specified process.
        /// </summary>
        /// <param name="process">The Process object to retrieve the icon for.</param>
        /// <returns>The icon as an Image object, or null if retrieval fails.</returns>
        public static Image? GetProcessIcon(Process process)
        {
            try
            {
                // Get the main module's file path
                string processPath = process.MainModule?.FileName;
                if (string.IsNullOrEmpty(processPath))
                    return null;

                SHFILEINFO shinfo = new SHFILEINFO();

                // Get large icon
                IntPtr iconHandle = SHGetFileInfo(
                    processPath,
                    0,
                    ref shinfo,
                    (uint)Marshal.SizeOf(shinfo),
                    SHGFI_ICON | SHGFI_LARGEICON);

                if (iconHandle == IntPtr.Zero)
                    return null;

                // Convert the icon handle to a .NET Icon object
                Icon processIcon = Icon.FromHandle(shinfo.hIcon);

                // Clone the icon to avoid memory issues
                Image iconImage = (Image)processIcon.ToBitmap().Clone();

                // Clean up native resources
                DestroyIcon(shinfo.hIcon);

                return iconImage;
            }
            catch
            {
                // Handle exceptions gracefully, e.g., if access to process resources is denied
                return null;
            }
        }
    }
}
