using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using Memory;
using RDRNetworkShared;

namespace RDRNetworkLauncher
{
    public class MainBehaviour
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);


        public static string RDRNFolder = Directory.GetCurrentDirectory() + "\\";

        public void Start(params string[] args)
        {
            #region Create splash screen
            SplashScreenThread splashScreen = new SplashScreenThread();
            #endregion

            System.Threading.Thread.Sleep(5500);

            var playerSetings = new PlayerSettings();

            #region Create settings.xml if it does not exist
            if (!File.Exists(RDRNFolder + "settings.xml") || string.IsNullOrWhiteSpace(File.ReadAllText(RDRNFolder + "settings.xml")))
            {
                System.Threading.Thread.Sleep(200);
                var ser = new XmlSerializer(typeof(PlayerSettings));
                using (var stream = File.OpenWrite(RDRNFolder + "settings.xml"))
                {
                    ser.Serialize(stream, playerSetings);
                }
            }
            #endregion

            #region Read settings.xml
            PlayerSettings settings = null;
            settings = PlayerSettings.ReadSettings(RDRNFolder + "settings.xml");
            #endregion

            splashScreen.SetPercent(10);
            splashScreen.SetPercent(15);

            #region Check if RDR2 or RDR2Launcher is running
            if (Process.GetProcessesByName("RDR2").Any() || Process.GetProcessesByName("RDR2Launcher").Any())
            {
                MessageBox.Show(splashScreen.SplashScreen, "RDR2 or the RDR:Network is already running. Please close them before starting RDR:Network.");
                return;
            }
            #endregion

            #region Check for dependencies
            if (!Environment.Is64BitOperatingSystem)
            {
                MessageBox.Show(splashScreen.SplashScreen, "RDR:Network does not work on 32bit machines.", "Incompatible");
                return;
            }
            #endregion

            #region Check CEF version
            if (!Directory.Exists(RDRNFolder + "cef") /*|| !File.Exists(RDRNFolder + "cef\\libcef.dll")*/)
            {
                MessageBox.Show(splashScreen.SplashScreen, "CEF directory or one of the core CEF components is missing from the directory, please reinstall.");
                return;
            }
            #endregion

            #region Check for Client Dependencies

            var shvVersion = new ParseableVersion(0, 0, 0, 0);
            if (File.Exists(RDRNFolder + "bin" + "\\" + "depsver.txt"))
            {
                shvVersion = ParseableVersion.Parse(File.ReadAllText(RDRNFolder + "bin" + "\\" + "depsver.txt"));
            }

            splashScreen.SetPercent(30);

            #endregion

            #region Check for new client version

            ParseableVersion fileVersion = new ParseableVersion(0, 0, 0, 0);
            if (File.Exists(RDRNFolder + "bin" + "\\" + "scripts" + "\\" + "ResuMP.Client.dll"))
            {
                fileVersion = ParseableVersion.Parse(FileVersionInfo.GetVersionInfo(RDRNFolder + "bin" + "\\" + "scripts" + "\\" + "ResuMP.Client.dll").FileVersion);
            }

            splashScreen.SetPercent(30);

            #endregion

            splashScreen.SetPercent(35);

            #region Check GamePath directory
            if (string.IsNullOrWhiteSpace(settings.GamePath) || !File.Exists(settings.GamePath + "\\" + "RDR2.exe"))
            {
                var diag = new OpenFileDialog();

                diag.DefaultExt = ".exe";
                diag.RestoreDirectory = true;
                diag.CheckFileExists = true;
                diag.CheckPathExists = true;
                diag.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    settings.GamePath = Path.GetDirectoryName(diag.FileName);
                    try
                    {
                        PlayerSettings.SaveSettings(RDRNFolder + "settings.xml", settings);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show(splashScreen.SplashScreen, "Insufficient permissions, Please run as Admin to avoid permission issues. (2)", "Unauthorized access");
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            #endregion

            splashScreen.SetPercent(45);

            #region Check required folders and clean up

            #endregion

            #region Patching Game Settings
            /*
            var mySettings = GameSettings.LoadGameSettings();
            if (mySettings.Video != null)
            {
                if (mySettings.Video.PauseOnFocusLoss != null)
                {
                    mySettings.Video.PauseOnFocusLoss.Value = 0;
                    mySettings.Graphics.DX_Version.Value = 2;
                }
            }
            else
            {
                mySettings.Video = new GameSettings.Video();
                mySettings.Video.PauseOnFocusLoss = new GameSettings.PauseOnFocusLoss();
                mySettings.Video.PauseOnFocusLoss.Value = 0;
                mySettings.Graphics.DX_Version = new GameSettings.DX_Version();
                mySettings.Graphics.DX_Version.Value = 2;
                mySettings.Video.Windowed = new GameSettings.Windowed();
                mySettings.Video.Windowed.Value = 2;
            }
            try
            {
                GameSettings.SaveSettings(mySettings);
            }
            catch
            {
                MessageBox.Show(splashScreen.SplashScreen, "Insufficient permissions, Please run as Admin to avoid permission issues.(8)", "Unauthorized access");
                return;
            }*/
            #endregion

            splashScreen.SetPercent(90);

            #region Copy over the savegame
            /*
            foreach (var file in Directory.GetFiles(Profiles, "pc_settings.bin", SearchOption.AllDirectories))
            {
                try
                {
                    if (File.Exists((Path.GetDirectoryName(file) + "\\" + "SGTA50000")))
                        MoveFile(Path.GetDirectoryName(file) + "\\" + "SGTA50000", Path.GetDirectoryName(file) + "\\" + "SGTA50000.bak");

                    if (File.Exists(RDRNFolder + "savegame" + "\\" + "SGTA50000"))
                        File.Copy(RDRNFolder + "savegame" + "\\" + "SGTA50000", Path.GetDirectoryName(file) + "\\" + "SGTA50000");
                }
                catch (Exception e)
                {
                    //MessageBox.Show(splashScreen.SplashScreen, "Insufficient permissions, Please run as Admin to avoid permission issues. (4)", "Unauthorized access");
                    MessageBox.Show(splashScreen.SplashScreen, e.ToString(), "Unauthorized access");
                    return;
                }
            }*/
            #endregion

            splashScreen.SetPercent(95);

            #region Launch the Game

            if (Directory.GetFiles(settings.GamePath, "*.wow").Length == 0)
            {
                BinaryReader br = new BinaryReader(new MemoryStream(File.ReadAllBytes(settings.GamePath + "\\" + "RDR2.exe")));
                br.BaseStream.Position = 0x01500000;
                byte[] array = br.ReadBytes(0x35F757);
                string value = BitConverter.ToString(array).Replace("-", string.Empty);

                if (value.Contains("737465616D")) 
                    Process.Start("steam://run/1174180"); 
                else 
                    Process.Start(settings.GamePath + "\\" + "RDR2.exe", "-ignorepipelinecache");   
            }
            else
            {
                Process.Start(settings.GamePath + "\\" + "RDR2.exe");
            }
            #endregion

            splashScreen.SetPercent(100);
            try
            {
                #region Wait for the Game to launch
                Process rdr2Process = null;


                while (FindWindow("sgaWindow", "Red Dead Redemption 2") == IntPtr.Zero)
                {
                    Thread.Sleep(100);
                }

                //Thread.Sleep(100);

                if (Process.GetProcessesByName("RDR2").Length > 0)
                    rdr2Process = Process.GetProcessesByName("RDR2").FirstOrDefault();

               // Thread.Sleep(100);
                #endregion

                splashScreen.SetPercent(100);
                splashScreen.Stop();

                #region Inject into RDR2
                //Thread.Sleep(25000);
                InjectOurselves(rdr2Process);
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            #region Restore save game
            /*
            foreach (var file in Directory.GetFiles(Profiles, "pc_settings.bin", SearchOption.AllDirectories))
            {
                try
                {
                    if (File.Exists((Path.GetDirectoryName(file) + "\\" + "SGTA50000")))
                        File.Delete(Path.GetDirectoryName(file) + "\\" + "SGTA50000");

                    if (File.Exists((Path.GetDirectoryName(file) + "\\" + "SGTA50000.bak")))
                        MoveFile(Path.GetDirectoryName(file) + "\\" + "SGTA50000.bak", Path.GetDirectoryName(file) + "\\" + "SGTA50000");
                }
                catch (Exception)
                {
                    MessageBox.Show(splashScreen.SplashScreen, "Insufficient permissions, Please run as Admin to avoid permission issues. (5)", "Unauthorized access");
                    return;
                }
            }*/
            #endregion
    
        }

        public static void InjectOurselves(Process rdr2)
        {
            Mem m = new Mem();

            try
            {
                m.OpenProcess("RDR2");

                m.InjectDLL(RDRNFolder + "bin" + "\\ScriptHookRDR2.dll");
                Thread.Sleep(100);
                //m.InjectDLL(RDRNFolder + "cef" + "\\libcef.dll");
                //m.InjectDLL(RDRNFolder + "bin" + "\\" + "sharpdx_direct3d11_1_effects_x64.dll");
                m.InjectDLL(RDRNFolder + "bin" + "\\RDRN_Module.dll");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region Dir and Files utils

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                NoReadonly(file);
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, true);
        }

        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                NoReadonly(dest);
                File.Copy(file, dest, true);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }

        public static void NoReadonly(string path)
        {
            if (File.Exists(path))
                new FileInfo(path).IsReadOnly = false;
        }

        public static IntPtr FindWindow(string windowName)
        {
            var hWnd = FindWindow(windowName, null);
            return hWnd;
        }
        #endregion

    }

    public class SplashScreenThread
    {
        private Thread _thread;

        private delegate void CloseForm();
        private delegate void SetPercentDel(int newPercent);

        public SplashScreen SplashScreen;

        public SplashScreenThread()
        {
            _thread = new Thread(Show);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void SetPercent(int newPercent)
        {
            while (SplashScreen == null) Thread.Sleep(10);
            if (SplashScreen.InvokeRequired)
            {
                SplashScreen.Invoke(new SetPercentDel(SetPercent), newPercent);

            }
            else
            {
                SplashScreen.progressBar1.Value = newPercent;
            }
        }

        public void Stop()
        {
            if (SplashScreen.InvokeRequired)
                SplashScreen.Invoke(new CloseForm(Stop));
            else
                SplashScreen.Close();
        }

        public void Show()
        {
            SplashScreen = new SplashScreen();
            SplashScreen.ShowDialog();
        }
    }
}
