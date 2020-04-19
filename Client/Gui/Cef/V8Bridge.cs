using RDRN_Shared;
using RDRNetwork.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace RDRNetwork.Gui.Cef
{
    internal class V8Bridge : CefV8Handler
    {
        private CefBrowser _browser;

        public V8Bridge(CefBrowser browser)
        {
            _browser = browser;
        }

        protected override bool Execute(string name, CefV8Value obj, CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
        {
            Browser father = null;

            LogManager.CefLog("-> Entering JS Execute. Func: " + name + " arg len: " + arguments.Length);

            father = CefUtil.GetBrowserFromCef(_browser);

            if (father == null)
            {
                LogManager.SimpleLog("cef", "NO FATHER FOUND FOR BROWSER " + _browser.Identifier);
                returnValue = CefV8Value.CreateNull();
                exception = "NO FATHER WAS FOUND.";
                return false;
            }
            LogManager.CefLog("-> Father was found!");
            try
            {
                switch (name)
                {
                    case "resourceCall":
                        {
                            LogManager.CefLog("-> Entering resourceCall...");

                            List<object> args = new List<object>();

                            for (int i = 1; i < arguments.Length; i++)
                            {
                                args.Add(arguments[i].GetValue());
                            }

                            LogManager.CefLog("-> Executing callback...");

                            object output = father._callback.Call(arguments[0].GetStringValue(), args.ToArray());

                            LogManager.CefLog("-> Callback executed!");

                            returnValue = V8Helper.CreateValue(output);
                            exception = null;
                            return true;
                        }
                    case "resourceEval":
                        {
                            LogManager.CefLog("-> Entering resource eval");
                            object output = father._callback.Eval(arguments[0].GetStringValue());
                            LogManager.CefLog("-> callback executed!");

                            returnValue = V8Helper.CreateValue(output);
                            exception = null;
                            return true;
                        }
                }
            }
            catch (Exception ex)
            {
                LogManager.CefLog(ex, "EXECUTE JS FUNCTION");
            }

            returnValue = CefV8Value.CreateNull();
            exception = "";
            return false;
        }
    }
}
