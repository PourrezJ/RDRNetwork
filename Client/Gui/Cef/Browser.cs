using Microsoft.ClearScript.V8;
using RDRN_Shared;
using System;
using System.Drawing;
using Xilium.CefGlue;
using Xilium.CefGlue.Platform.Windows;

namespace RDRNetwork.Gui.Cef
{
    internal class Browser : IDisposable
    {
        internal MainCefClient _client;
        internal CefBrowser _browser;
        internal BrowserJavascriptCallback _callback;

        internal CefV8Context _mainContext;

        internal readonly bool _localMode;
        internal bool _hasFocused;

        public CefBrowserHost GetHost()
            => _browser.GetHost();

        private bool _headless = false;
        public bool Headless
        {
            get => _headless;
            set
            {
                _client.SetHidden(value);
                _headless = value;
            }
        }
        private Point _position;

        public Point Position
        {
            get => _position;
            set
            {
                _position = value;
                _client.SetPosition(value.X, value.Y);
            }
        }

        public PointF[] Pinned { get; set; }

        private Size _size;
        public Size Size
        {
            get => _size;
            set
            {
                _client.SetSize(value.Width, value.Height);
                _size = value;
            }
        }

        private V8ScriptEngine Father;

        public void Eval(string code)
        {
            if (!_localMode) return;
            _browser.GetMainFrame().ExecuteJavaScript(code, null, 0);
        }

        public void Call(string method, params object[] arguments)
        {
            if (!_localMode) 
                return;

            string callString = method + "(";
            if (arguments != null)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    string comma = ", ";
                    if (i == arguments.Length - 1)
                        comma = "";
                    if (arguments[i] is string)
                    {
                        var escaped = System.Web.HttpUtility.JavaScriptStringEncode(arguments[i].ToString(), true);
                        callString += escaped + comma;
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
            }
            callString += ");";

            _browser.GetMainFrame().ExecuteJavaScript(callString, null, 0);
        }

        internal Browser(V8ScriptEngine father, string url, Size browserSize, bool localMode)
        {
            try
            {
                Father = father;

                LogManager.CefLog("--> Browser: Start");
                var windowInfo = CefWindowInfo.Create();
                windowInfo.SetAsPopup(IntPtr.Zero, null);
                windowInfo.Width = browserSize.Width;
                windowInfo.Height = browserSize.Height;
                windowInfo.WindowlessRenderingEnabled = true;
                windowInfo.SharedTextureEnabled = false;

                windowInfo.Style = WindowStyle.WS_OVERLAPPEDWINDOW | WindowStyle.WS_CLIPCHILDREN;

                var browserSettings = new CefBrowserSettings()
                {
                    BackgroundColor = new CefColor(0, 0, 0, 0),
                    JavaScript = CefState.Enabled,
                    JavaScriptAccessClipboard = CefState.Disabled,
                    JavaScriptCloseWindows = CefState.Disabled,
                    JavaScriptDomPaste = CefState.Disabled,
                    //JavaScriptOpenWindows = CefState.Disabled,
                    LocalStorage = CefState.Disabled,
                    WindowlessFrameRate = 60

                };

                _client = new MainCefClient(this, browserSize.Width, browserSize.Height);

                Size = browserSize;
                _localMode = localMode;
                _callback = new BrowserJavascriptCallback(father, this);

                _client.OnCreated += (sender, args) =>
                {
                    _browser = sender as CefBrowser;
                    LogManager.CefLog("-> Browser created!");
                    GoToPage("https://www.twitch.tv/directory");
                    LogManager.CefLog("-> Browser created! 2");
                };
                LogManager.CefLog("--> Browser: Creating Browser");
                CefBrowserHost.CreateBrowser(windowInfo, _client, browserSettings, url);

                CEFManager.Browsers.Add(this);
            }
            catch (Exception e)
            {
                LogManager.CefLog(e, "CreateBrowser Error");
            }
           
            LogManager.CefLog("--> Browser: End");
        }

        internal void GoToPage(string page)
        {
            if (_browser != null)
            {
                LogManager.CefLog("Trying to load page " + page + "...");
                _browser.GetMainFrame().LoadUrl(page);
                LogManager.CefLog("Page loaded ...");
            }
        }

        internal void GoBack()
        {
            if (_browser != null && _browser.CanGoBack)
            {
                LogManager.CefLog("Trying to go back a page...");
                _browser.GoBack();
            }
        }

        internal void Close()
        {
            _client.Close();

            if (_browser == null) return;
            var host = _browser.GetHost();
            host.CloseBrowser(true);
            host.Dispose();
            _browser.Dispose();
        }

        internal string GetAddress()
        {
            if (_browser == null) 
                return null;
            return _browser.GetMainFrame().Url;
        }

        internal bool IsLoading() => _browser.IsLoading;

        internal bool IsInitialized() => _browser != null;

        public void Dispose() => _browser = null;
    }
}
