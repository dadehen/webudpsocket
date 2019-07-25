using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class SetProxy
    {
        [DllImport(@"wininet",
       SetLastError = true,
       CharSet = CharSet.Auto,
       EntryPoint = "InternetSetOption",
       CallingConvention = CallingConvention.StdCall)]
        public static extern bool InternetSetOption
       (
       int hInternet,
       int dmOption,
       IntPtr lpBuffer,
       int dwBufferLength
       );

        public static void Set(string proxy)
        { 
            using (RegistryKey regKey = Registry.CurrentUser)
            {
                string SubKeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings";
                RegistryKey optionKey = regKey.OpenSubKey(SubKeyPath, true);             //更改健值，设置代理
                //optionKey.SetValue("ProxyEnable", 1);
                //optionKey.SetValue("ProxyServer", proxy);
                optionKey.SetValue("AutoConfigURL", proxy); 
                //激活代理设置【用于即使IE没有关闭也能更新当前打开的IE中的代理设置。】   
                InternetSetOption(0, 39, IntPtr.Zero, 0);
                InternetSetOption(0, 37, IntPtr.Zero, 0);
                regKey.Flush(); //刷新注册表  
                //ShowProxyInfo();
            }
        }
    }
}
