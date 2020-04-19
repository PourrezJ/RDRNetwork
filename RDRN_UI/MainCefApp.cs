using Xilium.CefGlue;

namespace RDRN_UI
{
    internal class MainCefApp : CefApp
    {
        private WebKitInjector _injector;

        public MainCefApp()
        {
            _injector = new WebKitInjector();
        }

        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            return _injector;
        }

        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            commandLine.AppendSwitch("--enable-media-stream");
            commandLine.AppendSwitch("--enable-usermedia-screen-capturing");
            commandLine.AppendSwitch("--off-screen-rendering-enabled");
            commandLine.AppendSwitch("--transparent-painting-enabled");
            commandLine.AppendSwitch("--disable-gpu");
        }
    }
}
