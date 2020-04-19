using RDRN_Shared;
using RDRNetwork.Utils;
using Xilium.CefGlue;

namespace RDRNetwork.Gui.Cef
{
    internal class MainCefLoadHandler : CefLoadHandler
    {
        protected override void OnLoadStart(CefBrowser browser, CefFrame frame, CefTransitionType transitionType)
        {
            // A single CefBrowser instance can handle multiple requests
            //   for a single URL if there are frames (i.e. <FRAME>, <IFRAME>).
            //if (frame.IsMain)
            {
                LogManager.CefLog("-> Start: " + browser.GetMainFrame().Url);
            }
        }

        protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {
            //if (frame.IsMain)
            {
                LogManager.CefLog($"-> End: {browser.GetMainFrame().Url}, {httpStatusCode}");
            }
        }
    }
}
