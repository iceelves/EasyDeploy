using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EasyDeploy.Helpers
{
    public class ConsoleWindow
    {
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole")]
        public static extern bool Show();

        [DllImport("kernel32.dll", EntryPoint = "FreeConsole")]
        public static extern bool Close();
    }
}
