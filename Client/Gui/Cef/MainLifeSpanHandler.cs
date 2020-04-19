using RDRN_Shared;
using Xilium.CefGlue;

namespace RDRNetwork.Gui.Cef
{
    internal class MainLifeSpanHandler : CefLifeSpanHandler
    {
        private MainCefClient bClient;


        internal MainLifeSpanHandler(MainCefClient bc)
        {
            LogManager.CefLog("-> MainLifeSpanHandler");
            this.bClient = bc;
        }

        protected override void OnAfterCreated(CefBrowser browser)
        {
            LogManager.CefLog("-> OnAfterCreated");
            base.OnAfterCreated(browser);
            this.bClient.Created(browser);
        }

        protected override bool OnBeforePopup(CefBrowser browser, CefFrame frame, string targetUrl, string targetFrameName, CefWindowOpenDisposition targetDisposition, bool userGesture, CefPopupFeatures popupFeatures, CefWindowInfo windowInfo, ref CefClient client, CefBrowserSettings settings, ref CefDictionaryValue extraInfo, ref bool noJavascriptAccess)
        {
            LogManager.CefLog("-> OnBeforePopup");
            Browser father = CefUtil.GetBrowserFromCef(browser);
            father.GoToPage(targetUrl);
            return true;
        }
    }
}
