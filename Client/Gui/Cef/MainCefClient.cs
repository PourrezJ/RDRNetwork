using RDRN_Shared;
using System;
using System.Drawing;
using Xilium.CefGlue;

namespace RDRNetwork.Gui.Cef
{
    internal class MainCefClient : CefClient
    {
        private readonly MainCefLoadHandler _loadHandler;
        private readonly MainCefRenderHandler _renderHandler;
        private readonly MainLifeSpanHandler _lifeSpanHandler;
        private readonly ContextMenuRemover _contextMenuHandler;

        public event EventHandler OnCreated;

        internal PointF Position;

        private byte[] sPixelBuffer;
        private byte[] sPopupPixelBufer;
        private CefRectangle _popupSize;
        private bool _popupShow;


        public MainCefClient(Browser browser, int windowWidth, int windowHeight)
        {
            _renderHandler = new MainCefRenderHandler(browser, windowWidth, windowHeight);
            _loadHandler = new MainCefLoadHandler();
            _lifeSpanHandler = new MainLifeSpanHandler(this);
            _contextMenuHandler = new ContextMenuRemover();
            LogManager.CefLog("-> MainCefClient");
        }

        public void SetPosition(int x, int y)
        {
            Position = new PointF(x, y);
            _renderHandler.SetPosition(x, y);
        }

        public void SetSize(int w, int h)
        {
            _renderHandler.SetSize(w, h);
        }

        public void SetHidden(bool hidden)
        {
           _renderHandler.SetHidden(hidden);
        }

        public void Close()
        {
            _renderHandler.Dispose();
        }

        public void Created(CefBrowser bs)
        {
            OnCreated?.Invoke(bs, EventArgs.Empty);
        }

        protected override CefContextMenuHandler GetContextMenuHandler()
        {
            LogManager.CefLog("-> _contextMenuHandler");
            return _contextMenuHandler;
        }
        
        protected override CefRenderHandler GetRenderHandler()
        {
            //LogManager.CefLog("-> _renderHandler");
            return _renderHandler;
        }

        protected override CefLoadHandler GetLoadHandler()
        {
            LogManager.CefLog("-> _loadHandler");
            return _loadHandler;
        }
        
        protected override CefLifeSpanHandler GetLifeSpanHandler()
        {
            LogManager.CefLog("-> _lifeSpanHandler");
            return _lifeSpanHandler;
        }
    }
}
