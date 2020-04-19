using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using RDRN_Shared;
using RDRNetwork.Utils;
using SharpDX;
using Xilium.CefGlue;

namespace RDRNetwork.Gui.Cef
{
    internal static class CEFManager
    {
        internal static string CefPath;

        internal static void InitializeCef()
        {
            CefPath = Path.Combine(Main.RDRNetworkPath, "cef");

            LogManager.CefLog("--> InitilizeCef: Start " + CefPath);
            CefRuntime.Load(CefPath);

            var args = new[]
            {
                "--off-screen-rendering-enabled",
                "--shared-texture-enabled",
                "--transparent-painting-enabled",
                "--enable-gpu",
                "--off-screen-frame-rate=60",
                "--multi-threaded-message-loop",
                        //"--single-process",
                /*
                "--disable-gpu",
                "--disable-software-rasterizer",
                "--disable-gpu-compositing",
                "--disable-gpu-vsync",
                "--enable-begin-frame-scheduling",*/

                "--enable-media-stream",
                "--enable-usermedia-screen-capturing",
                "--enable-gpu",
                //"--shared-texture-enabled",
                "--disable-gpu-shader-disk-cache",
                "--show-fps-counter"
            };

            var cefMainArgs = new CefMainArgs(args);
            var cefApp = new MainCefApp();

            if (CefRuntime.ExecuteProcess(cefMainArgs, cefApp, IntPtr.Zero) != -1)
            {
                LogManager.CefLog("Main: CefRuntime could not execute the secondary process.");
            }

            var cefSettings = new CefSettings()
            {
                LogSeverity = CefLogSeverity.Default,
                MultiThreadedMessageLoop = true,
                WindowlessRenderingEnabled = true,
                CachePath = CefPath,
                ResourcesDirPath = CefPath,
                LocalesDirPath = CefPath + "\\locales",
                BrowserSubprocessPath = CefPath + "\\RDRN_UI.exe",
                IgnoreCertificateErrors = true,
                LogFile = CefPath + "\\ceflog.log",
                Locale = CultureInfo.CurrentCulture.Name,
                //ExternalMessagePump = true,

            };
            
            // if (Main.PlayerSettings.CEFDevtool) cefSettings.RemoteDebuggingPort = 9222;
            cefSettings.RemoteDebuggingPort = 9222;
            CefRuntime.Initialize(cefMainArgs, cefSettings, cefApp, IntPtr.Zero);
            /*
            CefRuntime.RegisterSchemeHandlerFactory("http", null, new SecureSchemeFactory());
            CefRuntime.RegisterSchemeHandlerFactory("https", null, new SecureSchemeFactory());
            CefRuntime.RegisterSchemeHandlerFactory("ftp", null, new SecureSchemeFactory());
            CefRuntime.RegisterSchemeHandlerFactory("sftp", null, new SecureSchemeFactory());
            */
            LogManager.CefLog("--> InitilizeCef: End");
        }

        internal static void DisposeCef()
        {
            CefRuntime.Shutdown();
        }

        internal static void Dispose()
        {
            Cursor?.Dispose();
            Cursor = null;
        }

        internal static void SetMouseHidden(bool hidden)
        {
            if (Cursor == null)
                Cursor = ImageElement.FromFile(Main.RDRNetworkPath + "\\images\\cef\\cursor.png", new PointF());
            Cursor.Hidden = hidden;
        }

        internal static readonly List<Browser> Browsers = new List<Browser>();
        internal static ImageElement Cursor;
    }
}