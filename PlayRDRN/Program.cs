using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RDRNetworkLauncher
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                new MainBehaviour().Start(args);
            }
            catch (Exception ex)
            {
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\logs")) 
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\logs");

                File.AppendAllText(Directory.GetCurrentDirectory() + "\\logs\\launcher.log", "LAUNCHER EXCEPTION AT " + DateTime.Now + "\r\n" + ex.ToString() + "\r\n\r\n");

                MessageBox.Show(ex.ToString(), "FATAL ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
