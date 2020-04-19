using RDRN_Shared;
using RDRNetwork.Gui.DirectXHook;
using RDRNetwork.Utils;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security;
using Xilium.CefGlue;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;

namespace RDRNetwork.Gui.Cef
{
    internal class MainCefRenderHandler : CefRenderHandler
    {
        private int windowHeight;
        private int windowWidth;

        private ImageElement imageElement;

        private Browser browser;

        //public Bitmap LastBitmap;
        //public readonly object BitmapLock = new object();


        public MainCefRenderHandler(Browser browser, int windowWidth, int windowHeight)
        {
            this.browser = browser;
            this.windowWidth = windowWidth;
            this.windowHeight = windowHeight;

            imageElement = new ImageElement();

            LogManager.CefLog("-> Instantiated Renderer");
        }

        public void SetHidden(bool hidden)
        {
            imageElement.Hidden = hidden;
        }

        public void SetSize(int width, int height)
        {
            windowHeight = height;
            windowWidth = width;
        }

        public void SetPosition(int x, int y)
        {
            imageElement.Position = new Point(x, y);
        }

        public void Dispose()
        {
            imageElement?.Dispose();
            imageElement = null;
        }

        protected override void OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo)
        {

        }

        protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect)
        {
            return true;
        }

        protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY)
        {
            screenX = viewX;
            screenY = viewY;
            return true;
        }

        protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo)
        {
            return true;
        }

        protected override void OnPopupSize(CefBrowser browser, CefRectangle rect)
        {
        }
        /*
        protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height)
        {
            if (this.browser == null)
                return;


            if (imageElement != null)
            {
                lock (imageElement)
                {
                    imageElement.SetBitmap(new System.Drawing.Bitmap(width, height, width * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, buffer), this.browser.Position);
                }
            }
        }*/
        
        [SecurityCritical]
        protected override unsafe void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height)
        {
            if (this.browser == null)
                return;

            if (imageElement == null)
                return;

            lock (imageElement)
            {
                int stride = width * sizeof(int);
                var length = height * stride;

                using (var tempStream = new SharpDX.DataStream(height * stride, true, true))
                {
                    for (int y = 0; y < height; y++)
                    {
                        int offset = stride * y;
                        for (int x = 0; x < width; x++)
                        {
                            // Not optimized 
                            byte B = Marshal.ReadByte(buffer, offset++);
                            byte G = Marshal.ReadByte(buffer, offset++);
                            byte R = Marshal.ReadByte(buffer, offset++);
                            byte A = Marshal.ReadByte(buffer, offset++);
                            int rgba = R | (G << 8) | (B << 16) | (A << 24);
                            tempStream.Write(rgba);
                        }
                    }

                    tempStream.Position = 0;
                    imageElement.Position = this.browser.Position;
                    imageElement.Height = height;
                    imageElement.Width = width;

                    if (imageElement.D2D1Bitmap != null)
                        imageElement.D2D1Bitmap.Dispose();

                    imageElement.D2D1Bitmap = new SharpDX.Direct2D1.Bitmap(
                                                DxHook.CurrentRenderTarget,
                                                new SharpDX.Size2(width, height),
                                                tempStream,
                                                stride,
                                                new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied)));
                }
            } 
        }

        protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y)
        {

        }

        protected override void OnImeCompositionRangeChanged(CefBrowser browser, CefRange selectedRange, CefRectangle[] characterBounds)
        {

        }

        protected override CefAccessibilityHandler GetAccessibilityHandler()
        {
            return null;
        }

        protected override void GetViewRect(CefBrowser browser, out CefRectangle rect)
        {
            //Console.WriteLine("GetViewRect");
            rect = new CefRectangle(0, 0, windowWidth, windowHeight);
        }

        protected override void OnAcceleratedPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr sharedHandle)
        {
            Console.WriteLine("OnAcceleratedPaint");
        }
    }
}