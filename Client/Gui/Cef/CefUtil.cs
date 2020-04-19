using System.Collections.Generic;
using Xilium.CefGlue;

namespace RDRNetwork.Gui.Cef
{
    internal static class CefUtil
    {
        internal static Dictionary<int, Browser> _cachedReferences = new Dictionary<int, Browser>();

        internal static Browser GetBrowserFromCef(CefBrowser browser)
        {
            Browser father = null;

            if (browser == null) return null;

            lock (CEFManager.Browsers)
            {
                if (_cachedReferences.ContainsKey(browser.Identifier))
                    return _cachedReferences[browser.Identifier];

                for (var index = CEFManager.Browsers.Count - 1; index >= 0; index--)
                {
                    var b = CEFManager.Browsers[index];
                    if (b?._browser == null || b._browser.Identifier != browser.Identifier) continue;
                    father = b;
                    _cachedReferences.Add(browser.Identifier, b);
                    break;
                }
            }
            return father;
        }
    }
}
