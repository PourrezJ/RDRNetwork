using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using NAudio.Wave;
using RDRNetwork.API;
using RDRNetwork.Javascript;
using RDRNetwork.Utils;
using RDRN_Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Windows.Forms;

namespace RDRNetwork.Gui.Cef
{
    internal static class JavascriptHook
    {
        internal static void Init()
        {
            ScriptEngines = new List<ClientsideScriptWrapper>();
            ThreadJumper = new List<Action>();
            //TextElements = new List<UIResText>();
            Exported = new ExpandoObject();
        }



        internal static PointF MousePosition { get; set; }
        internal static bool MouseClick { get; set; }

       // internal static List<UIResText> TextElements { get; set; }

        internal static List<ClientsideScriptWrapper> ScriptEngines;

        internal static List<Action> ThreadJumper;

        internal static WaveOutEvent AudioDevice { get; set; }
        internal static WaveStream AudioReader { get; set; }

        internal static ExpandoObject Exported { get; set; }

        internal static void InvokeServerEvent(string eventName, string resource, object[] arguments)
        {
            ThreadJumper.Add(() =>
            {
                lock (ScriptEngines)
                    for (int i = 0; i < ScriptEngines.Count; i++)
                    {
                        try
                        {
                            if (resource != "*" && ScriptEngines[i].ResourceParent != resource) continue;
                            ScriptEngines[i].Engine.Script.API.invokeServerEvent(eventName, arguments);
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }
            });
        }

        internal static void InvokeMessageEvent(string msg)
        {
            if (msg == null) return;
            ThreadJumper.Add(() =>
            {
                if (msg.StartsWith("/"))
                {
                    lock (ScriptEngines)
                    {
                        for (var index = ScriptEngines.Count - 1; index >= 0; index--)
                        {
                            ScriptEngines[index].Engine.Script.API.invokeChatCommand(msg);
                        }
                    }
                }
                else
                {
                    lock (ScriptEngines)
                    {
                        for (var index = 0; index < ScriptEngines.Count; index++)
                        {
                            ScriptEngines[index].Engine.Script.API.invokeChatMessage(msg);
                        }
                    }
                }
            });
        }

        internal static void InvokeCustomEvent(Action<dynamic> func)
        {
            ThreadJumper.Add(() =>
            {
                lock (ScriptEngines)
                {
                    for (var index = ScriptEngines.Count - 1; index >= 0; index--)
                    {
                        func(ScriptEngines[index].Engine.Script.API);
                    }
                }
            });
        }

        internal static void InvokeStreamInEvent(LocalHandle handle, int type)
        {
            ThreadJumper.Add(() =>
            {
                lock (ScriptEngines)
                {
                    for (var index = 0; index < ScriptEngines.Count; index++)
                    {
                        ScriptEngines[index].Engine.Script.API.invokeEntityStreamIn(handle, type);
                    }
                }
            });
        }

        internal static void InvokeStreamOutEvent(LocalHandle handle, int type)
        {
            ThreadJumper.Add(() =>
            {
                lock (ScriptEngines)
                {
                    for (var index = 0; index < ScriptEngines.Count; index++)
                    {
                        ScriptEngines[index].Engine.Script.API.invokeEntityStreamOut(handle, type);
                    }
                }
            });
        }

        internal static void InvokeDataChangeEvent(LocalHandle handle, string key, object oldValue)
        {
            ThreadJumper.Add(() =>
            {
                lock (ScriptEngines)
                {
                    for (var index = 0; index < ScriptEngines.Count; index++)
                    {
                        ScriptEngines[index].Engine.Script.API.invokeEntityDataChange(handle, key, oldValue);
                    }
                }
            });
        }

        internal static void InvokeCustomDataReceived(string resource, string data)
        {
            ThreadJumper.Add(() =>
            {
                lock (ScriptEngines)
                {
                    foreach (var res in ScriptEngines.Where(en => en.ResourceParent == resource))
                    {
                        res.Engine.Script.API.invokeCustomDataReceived(data);
                    }
                }
            });
        }

        internal static void OnTick()
        {
            var tmpList = new List<Action>(ThreadJumper);
            ThreadJumper.Clear();
            try
            {
                for (var i = 0; i < tmpList.Count; i++)
                {
                    tmpList[i].Invoke();
                }

                lock (ScriptEngines)
                {
                    for (var i = 0; i < ScriptEngines.Count; i++)
                    {
                        ScriptEngines[i].Engine.Script.API.invokeUpdate();
                        ScriptEngines[i].Engine.Script.API.processCoroutines();
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        internal static void OnKeyDown(object sender, KeyEventArgs e)
        {
           // if (Main.Chat == null || Main.Chat.IsFocused || Main.MainMenu.Visible) return;

            lock (ScriptEngines)
            {
                for (var i = 0; i < ScriptEngines.Count; i++)
                {
                    //try
                    //{
                    ScriptEngines[i].Engine.Script.API.invokeKeyDown(sender, e);
                    //}
                    //catch (ScriptEngineException ex)
                    //{
                    //    LogException(ex);
                    //}
                }
            }
        }

        internal static void OnKeyUp(object sender, KeyEventArgs e)
        {
            //if (Main.Chat == null || Main.Chat.IsFocused) return;

            lock (ScriptEngines)
            {
                for (var i = 0; i < ScriptEngines.Count; i++)
                {
                    //try
                    //{
                    ScriptEngines[i].Engine.Script.API.invokeKeyUp(sender, e);
                    //}
                    //catch (ScriptEngineException ex)
                    //{
                    //    LogException(ex);
                    //}
                }
            }
        }

        internal static void StartScripts(ScriptCollection sc)
        {
            var localSc = new List<ClientsideScript>(sc.ClientsideScripts);

            ThreadJumper.Add(() =>
            {
                var scripts = localSc.Select(StartScript).ToList();

                var exportedDict = Exported as IDictionary<string, object>;

                foreach (var group in scripts.GroupBy(css => css.ResourceParent))
                {
                    dynamic thisRes = new ExpandoObject();
                    var thisResDict = (IDictionary<string, object>)thisRes;

                    foreach (var compiledResources in group)
                    {
                        thisResDict.Add(compiledResources.Filename, compiledResources.Engine.Script);
                    }

                    foreach (var wrapper in group)
                    {
                        wrapper.Engine.AddHostObject("resource", thisRes);
                    }

                    exportedDict.Add(group.Key, thisRes);
                }

                for (var index = scripts.Count - 1; index >= 0; index--)
                {
                    var cr = scripts[index];
                    cr.Engine.AddHostObject("exported", Exported);

                    cr.Engine.Script.API.invokeResourceStart();
                }
            });
        }

        internal static ClientsideScriptWrapper StartScript(ClientsideScript script)
        {
            ClientsideScriptWrapper csWrapper;
            var scriptEngine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            //scriptEngine.AddHostObject("host", new HostFunctions()); // Disable an exploit where you could get reflection
            scriptEngine.AddHostObject("API", new ScriptContext(scriptEngine));
            scriptEngine.AddHostType("Enumerable", typeof(Enumerable));
            scriptEngine.AddHostType("List", typeof(List<>));
            scriptEngine.AddHostType("Dictionary", typeof(Dictionary<,>));
            scriptEngine.AddHostType("String", typeof(string));
            scriptEngine.AddHostType("Int32", typeof(int));
            scriptEngine.AddHostType("Bool", typeof(bool));
            scriptEngine.AddHostType("Double", typeof(double));
            scriptEngine.AddHostType("Float", typeof(float));
            scriptEngine.AddHostType("KeyEventArgs", typeof(KeyEventArgs));
            scriptEngine.AddHostType("CancelEventArgs", typeof(CancelEventArgs));
            scriptEngine.AddHostType("Keys", typeof(Keys));
            scriptEngine.AddHostType("Point", typeof(Point));
            scriptEngine.AddHostType("PointF", typeof(PointF));
            scriptEngine.AddHostType("Size", typeof(Size));
            scriptEngine.AddHostType("Size2", typeof(SharpDX.Size2));
            scriptEngine.AddHostType("Vector3", typeof(Vector3));
            scriptEngine.AddHostType("Matrix4", typeof(Matrix));
            //scriptEngine.AddHostType("menuControl", typeof(UIMenu.MenuControls));
           // scriptEngine.AddHostType("BadgeStyle", typeof(UIMenuItem.BadgeStyle));
            scriptEngine.AllowReflection = false;

            try
            {
                scriptEngine.Execute(script.Script);
                scriptEngine.Script.API.ParentResourceName = script.ResourceParent;
            }
            catch (ScriptEngineException ex)
            {
                LogException(ex);
            }
            finally
            {
                csWrapper = new ClientsideScriptWrapper(scriptEngine, script.ResourceParent, script.Filename);
                lock (ScriptEngines) ScriptEngines.Add(csWrapper);
            }
            return csWrapper;
        }

        internal static void StopAllScripts()
        {
            lock (ScriptEngines)
            {
                foreach (var t in ScriptEngines)
                {
                    t.Engine.Script.API.isDisposing = true;
                }
            }

            lock (ScriptEngines)
            {
                foreach (var t in ScriptEngines)
                {
                    t.Engine.Interrupt();
                    t.Engine.Script.API.invokeResourceStop();
                    t.Engine.Dispose();
                }
                ScriptEngines.Clear();
            }

            AudioDevice?.Stop();
            AudioDevice?.Dispose();
            AudioReader?.Dispose();
            AudioDevice = null;
            AudioReader = null;
            Exported = new ExpandoObject();

            lock (CEFManager.Browsers)
            {
                foreach (var t in CEFManager.Browsers)
                {
                    t.Close();
                    t.Dispose();
                }

                CEFManager.Browsers.Clear();
            }
        }

        internal static void StopScript(string resourceName)
        {
            lock (ScriptEngines)
            {
                for (int i = 0; i < ScriptEngines.Count; i++)
                {
                    if (ScriptEngines[i].ResourceParent != resourceName) continue;
                    ScriptEngines[i].Engine.Script.API.isDisposing = true;
                }
            }

            var dict = Exported as IDictionary<string, object>;
            dict.Remove(resourceName);

            ThreadJumper.Add(delegate
            {
                lock (ScriptEngines)
                {
                    for (int i = 0; i < ScriptEngines.Count; i++)
                    {
                        if (ScriptEngines[i].ResourceParent != resourceName) continue;
                        ScriptEngines[i].Engine.Script.API.invokeResourceStop();
                        ScriptEngines[i].Engine.Dispose();
                        ScriptEngines.RemoveAt(i);
                    }
                }

            });
        }


        private static void LogException(Exception ex)
        {
            Func<string, int, string[]> splitter = (string input, int everyN) =>
            {
                var list = new List<string>();
                for (var i = 0; i < input.Length; i += everyN)
                {
                    list.Add(input.Substring(i, Math.Min(everyN, input.Length - i)));
                }
                return list.ToArray();
            };

            //Util.Util.SafeNotify("~r~~h~Clientside Javascript Error~h~~w~");

            var count = splitter(ex.Message, 99).Length;
            for (var index = 0; index < count; index++)
            {
                //Util.Util.SafeNotify(splitter(ex.Message, 99)[index]);
            }

            LogManager.LogException(ex, "CLIENTSIDE SCRIPT ERROR");
        }
    }
}
