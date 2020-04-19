using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RDRNetwork.Gui.Cef
{
    internal class BrowserJavascriptCallback
    {
        private V8ScriptEngine _parent;
        private Browser _wrapper;
        public BrowserJavascriptCallback(V8ScriptEngine parent, Browser wrapper)
        {
            _parent = parent;
            _wrapper = wrapper;
        }

        public BrowserJavascriptCallback() { }

        public object Call(string functionName, params object[] arguments)
        {
            if (!_wrapper._localMode) return null;

            object objToReturn = null;
            bool hasValue = false;

            lock (JavascriptHook.ThreadJumper)
                JavascriptHook.ThreadJumper.Add(() =>
                {
                    try
                    {
                        string callString = functionName + "(";

                        if (arguments != null)
                            for (int i = 0; i < arguments.Length; i++)
                            {
                                string comma = ", ";

                                if (i == arguments.Length - 1)
                                    comma = "";

                                if (arguments[i] is string)
                                {
                                    callString += System.Web.HttpUtility.JavaScriptStringEncode(arguments[i].ToString(), true) + comma;
                                }
                                else if (arguments[i] is bool)
                                {
                                    callString += arguments[i].ToString().ToLower() + comma;
                                }
                                else
                                {
                                    callString += arguments[i] + comma;
                                }
                            }

                        callString += ");";

                        objToReturn = _parent.Evaluate(callString);
                    }
                    finally
                    {
                        hasValue = true;
                    }
                });

            while (!hasValue) Thread.Sleep(10);

            return objToReturn;
        }

        public object Eval(string code)
        {
            if (!_wrapper._localMode) return null;
            // TODO: reinstate

            object objToReturn = null;
            bool hasValue = false;

            lock (JavascriptHook.ThreadJumper)
                JavascriptHook.ThreadJumper.Add(() =>
                {
                    try
                    {
                        objToReturn = _parent.Evaluate(code);
                    }
                    finally
                    {
                        hasValue = true;
                    }
                });

            while (!hasValue) Thread.Sleep(10);

            return objToReturn;
        }
        public void addEventHandler(string eventName, Action<object[]> action)
        {
            if (!_wrapper._localMode) return;
            _eventHandlers.Add(new Tuple<string, Action<object[]>>(eventName, action));
        }

        internal void TriggerEvent(string eventName, params object[] arguments)
        {
            foreach (var handler in _eventHandlers)
            {
                if (handler.Item1 == eventName)
                    handler.Item2.Invoke(arguments);
            }
        }

        private List<Tuple<string, Action<object[]>>> _eventHandlers = new List<Tuple<string, Action<object[]>>>();
    }
}
