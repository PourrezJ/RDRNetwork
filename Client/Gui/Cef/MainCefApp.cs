using System;
using Xilium.CefGlue;

namespace RDRNetwork.Gui.Cef
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
            //commandLine.AppendSwitch("--transparent-painting-enabled");
            //commandLine.AppendSwitch("--disable-gpu");
            commandLine.AppendSwitch("--enable-gpu");
            commandLine.AppendSwitch("--multi-threaded-message-loop ");
            commandLine.AppendSwitch("--off-screen-frame-rate=60");
            commandLine.AppendSwitch("--shared-texture-enabled");
            commandLine.AppendSwitch("--disable-gpu-shader-disk-cache");
            commandLine.AppendSwitch("--use-angle", "d3d11");
        }
    }
}
