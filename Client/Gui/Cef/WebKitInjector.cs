using RDRN_Shared;
using Xilium.CefGlue;

namespace RDRNetwork.Gui.Cef
{
    internal class WebKitInjector : CefRenderProcessHandler
    {
        protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            LogManager.CefLog("-> OnContextCreated!");
            if (frame.IsMain)
            {
                LogManager.CefLog("-> Setting main context!");

                Browser father = CefUtil.GetBrowserFromCef(browser);
                if (father != null)
                {
                    father._mainContext = context;
                    LogManager.CefLog("-> Main context set!");
                }
            }

            CefV8Value global = context.GetGlobal();

            CefV8Value func = CefV8Value.CreateFunction("resourceCall", new V8Bridge(browser));
            global.SetValue("resourceCall", func);

            CefV8Value func2 = CefV8Value.CreateFunction("resourceEval", new V8Bridge(browser));
            global.SetValue("resourceEval", func2);

            base.OnContextCreated(browser, frame, context);
        }
        /*
        protected override bool OnBeforeNavigation(CefBrowser browser, CefFrame frame, CefRequest request, CefNavigationType navigation_type, bool isRedirect)
        {
            if ((request.TransitionType & CefTransitionType.ForwardBackFlag) != 0 || navigation_type == CefNavigationType.BackForward)
            {
                return true;
            }

            return base.OnBeforeNavigation(browser, frame, request, navigation_type, isRedirect);
        }
        */
        protected override void OnRenderThreadCreated(CefListValue extraInfo)
        {
            LogManager.CefLog("-> OnRenderThreadCreated!");
            base.OnRenderThreadCreated(extraInfo);
        }

        protected override void OnWebKitInitialized()
        {
            LogManager.CefLog("-> OnWebKitInitialized!");
            base.OnWebKitInitialized();
        }

        protected override void OnBrowserCreated(CefBrowser browser, CefDictionaryValue extraInfo)
        {
            LogManager.CefLog("-> OnBrowserCreated!");
            base.OnBrowserCreated(browser, extraInfo);
        }

        protected override void OnBrowserDestroyed(CefBrowser browser)
        {
            LogManager.CefLog("-> OnBrowserDestroyed!");
            base.OnBrowserDestroyed(browser);
        }

        protected override CefLoadHandler GetLoadHandler()
        {
            LogManager.CefLog("-> GetLoadHandler!");
            return base.GetLoadHandler();
        }

        protected override void OnContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            LogManager.CefLog("-> OnContextReleased!");
            base.OnContextReleased(browser, frame, context);
        }

        protected override void OnUncaughtException(CefBrowser browser, CefFrame frame, CefV8Context context, CefV8Exception exception, CefV8StackTrace stackTrace)
        {
            LogManager.CefLog("-> OnUncaughtException!");
            base.OnUncaughtException(browser, frame, context, exception, stackTrace);
        }

        protected override void OnFocusedNodeChanged(CefBrowser browser, CefFrame frame, CefDomNode node)
        {
            LogManager.CefLog("-> OnFocusedNodeChanged!");
            base.OnFocusedNodeChanged(browser, frame, node);
        }

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefFrame frame, CefProcessId sourceProcess, CefProcessMessage message)
        {
            LogManager.CefLog("-> OnProcessMessageReceived!");
            return base.OnProcessMessageReceived(browser, frame, sourceProcess, message);
        }
    }
}
