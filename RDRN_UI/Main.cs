using System;
using System.Runtime.InteropServices;
using Xilium.CefGlue;

namespace RDRN_UI
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Starting RDRN UI Process");

            CefRuntime.Load();

            var cefMainArgs = new CefMainArgs(args);
            var cefApp = new MainCefApp();

            if (CefRuntime.ExecuteProcess(cefMainArgs, cefApp, IntPtr.Zero) != -1)
            {
                Console.WriteLine("CefRuntime could not execute the secondary process.");
            }
        }
    }
}
