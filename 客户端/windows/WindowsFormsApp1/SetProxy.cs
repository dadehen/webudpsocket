namespace WindowsFormsApp1
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;

    internal class SetProxy
    {
        [DllImport("wininet", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool InternetSetOption(int hInternet, int dmOption, IntPtr lpBuffer, int dwBufferLength);
        public static void Set(string proxy)
        {
            using (RegistryKey key = Registry.CurrentUser)
            {
                string name = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
                key.OpenSubKey(name, true).SetValue("AutoConfigURL", proxy);
                InternetSetOption(0, 0x27, IntPtr.Zero, 0);
                InternetSetOption(0, 0x25, IntPtr.Zero, 0);
                key.Flush();
            }
        }
    }
}

