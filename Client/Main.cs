using RDR2;
using RDR2.Native;
using RDRN_Shared;
using RDRNetwork.Gui.Cef;
using RDRNetwork.Gui.DirectXHook;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDRNetwork
{
    internal class Main : Script
    {
        internal static string RDRNetworkPath = System.IO.Directory.GetParent(ScriptDomain.CurrentDir).FullName;

        internal static DxHook DxHook;
        internal static Size Screen
        {
            get
            {
                int w, h;
                unsafe { Function.Call(Hash.GET_SCREEN_RESOLUTION, &w, &h); }
                return new Size(w, h);
            }
        }

        public override void OnInit()
        {
            Console.WriteLine(RDRNetworkPath);

            LogManager.Init(Path.Combine(RDRNetworkPath, "logs"));

            LogManager.DebugLog("Starting RDRNetwork.");

            ActionsCache = new List<Action>();
            /*
            DxHook = new DxHook();
            CEFManager.InitializeCef();

            var newBrowser = new Browser(new Microsoft.ClearScript.V8.V8ScriptEngine(), "http://google.fr", Screen, false);
            newBrowser.Position = new Point(Screen.Width /3 , Screen.Height / 3);
            Task.Run(async () =>
            {
                await Task.Delay(500);
                Main.Do(() =>
                {
                    Console.WriteLine("GoToPage");
                    newBrowser.GoToPage("https://www.youtube.com/watch?v=ufQl2NCzf6E");
                    newBrowser.GetHost().SetFocus(true);
                    newBrowser.GetHost().SendFocusEvent(true);
                    CefController.ShowCursor = true;
                });
            });*/


            /*
            DateTime now = DateTime.Now;
            for (int a = 0; a < 100000; a++)
            {
                Function.Call(Hash.DRAW_RECT, 0.1f, 0.2f, 0.1f, 0.1f, 255, 0, 0, 255);
                Function.Call(Hash.PLAYER_ID);
            }

            Console.WriteLine("Native perfs {0}", (DateTime.Now - now).TotalMilliseconds);
            */
            LogManager.DebugLog("End RDRNetwork.");
        }

        public override void OnKeyDown(KeyEventArgs args)
        {
            CefController.OnKeyDown(args);
            base.OnKeyDown(args);
        }

        public override void OnKeyUp(KeyEventArgs args)
        {
            CefController.OnKeyUp(args);
            base.OnKeyUp(args);
        }

        public override void OnTick()
        {
            if (ActionsCache != null)
            {
                if (ActionsCache.Count > 0)
                {
                    lock (ActionsCache)
                    {
                        var last = ActionsCache.Last();
                        if (last != null)
                        {
                            ActionsCache.Remove(last);
                            last.Invoke();
                        }
                    }
                }
            }

            CefController.OnTick();
            CleanupGame.OnTick();

            base.OnTick();
        }

        static List<Action> ActionsCache;

        public static void Do(Action action)
        {
            lock (ActionsCache)
            {
                ActionsCache.Add(action);
            }
        }
    }
}
