using RDR2.Native;
using RDRNetwork.API;
using RDRNetwork.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;
using Xilium.CefGlue;
using Control = RDRNetwork.API.Control;

namespace RDRNetwork.Gui.Cef
{
    internal static class CefController
    {
        private static bool _showCursor;

        public static bool ShowCursor
        {
            get => _showCursor;
            set
            {
                if (!_showCursor && value)
                {
                    _justShownCursor = true;
                    _lastShownCursor = Util.TickCount;
                }
                _showCursor = value;

                CEFManager.SetMouseHidden(!value);
            }
        }

        private static bool _justShownCursor;
        private static long _lastShownCursor = 0;
        internal static PointF _lastMousePoint;
        private static Keys _lastKey;

        internal static CefEventFlags GetMouseModifiers(bool leftbutton, bool rightButton)
        {
            CefEventFlags mod = CefEventFlags.None;

            if (leftbutton) mod |= CefEventFlags.LeftMouseButton;
            if (rightButton) mod |= CefEventFlags.RightMouseButton;

            return mod;
        }

        internal static void OnTick()
        {
            if (ShowCursor)
            {
                Game.DisableAllControlsThisFrame(0);

                var res = Main.Screen;
                var mouseX = Game.GetDisabledControlNormal(0, Control.CursorX) * res.Width;
                var mouseY = Game.GetDisabledControlNormal(0, Control.CursorY) * res.Height;
                
                 _lastMousePoint = new PointF(mouseX, mouseY);

                 if (CEFManager.Cursor != null)
                 {
                     CEFManager.Cursor.Position = new Point((int)mouseX, (int)mouseY);
                 }

                 var mouseDown = Game.IsEnabledControlJustPressed(2, Control.CursorAccept);
                 var mouseDownRN = Game.IsEnabledControlJustPressed(2, Control.CursorAccept);
                 var mouseUp = Game.IsEnabledControlJustPressed(2, Control.CursorAccept);

                 var rmouseDown = Game.IsEnabledControlJustPressed(2, Control.CursorCancel);
                 var rmouseDownRN = Game.IsEnabledControlJustPressed(2, Control.CursorCancel);
                 var rmouseUp = Game.IsEnabledControlJustPressed(2, Control.CursorCancel);

                 var wumouseDown = Game.IsEnabledControlJustPressed(2, Control.CursorScrollUp);
                 var wdmouseDown = Game.IsEnabledControlJustPressed(2, Control.CursorScrollDown);

                 foreach (var browser in CEFManager.Browsers)
                 {
                     if (!browser.IsInitialized()) 
                        continue;

                     if (!browser._hasFocused)
                     {
                         browser._browser.GetHost().SetFocus(true);
                         browser._browser.GetHost().SendFocusEvent(true);
                         browser._hasFocused = true;
                     }

                     if (mouseX > browser.Position.X && mouseY > browser.Position.Y &&
                         mouseX < browser.Position.X + browser.Size.Width &&
                         mouseY < browser.Position.Y + browser.Size.Height)
                     {
                         var ev = new CefMouseEvent((int)(mouseX - browser.Position.X), (int)(mouseY - browser.Position.Y),
                                 GetMouseModifiers(mouseDownRN, rmouseDownRN));

                         browser._browser
                             .GetHost()
                             .SendMouseMoveEvent(ev, false);

                         if (mouseDown)
                             browser._browser
                                 .GetHost()
                                 .SendMouseClickEvent(ev, CefMouseButtonType.Left, false, 1);

                         if (mouseUp)
                             browser._browser
                                 .GetHost()
                                 .SendMouseClickEvent(ev, CefMouseButtonType.Left, true, 1);

                         if (rmouseDown)
                             browser._browser
                                 .GetHost()
                                 .SendMouseClickEvent(ev, CefMouseButtonType.Right, false, 1);

                         if (rmouseUp)
                             browser._browser
                                 .GetHost()
                                 .SendMouseClickEvent(ev, CefMouseButtonType.Right, true, 1);

                         if (wdmouseDown)
                             browser._browser
                                 .GetHost()
                                 .SendMouseWheelEvent(ev, 0, -30);

                         if (wumouseDown)
                             browser._browser
                                 .GetHost()
                                 .SendMouseWheelEvent(ev, 0, 30);
                     }
                 }
            }
            else
            {
                //Function.Call(Hash._SET_MOUSE_CURSOR_ACTIVE_THIS_FRAME);
            }
        }

        internal static void OnKeyDown(KeyEventArgs args)
        {
            if (!ShowCursor) return;

            if (_justShownCursor && Util.TickCount - _lastShownCursor < 500)
            {
                _justShownCursor = false;
                return;
            }

            foreach (var browser in CEFManager.Browsers)
            {
                if (!browser.IsInitialized())
                    continue;

                CefEventFlags mod = CefEventFlags.None;
                
                if (args.Control) mod |= CefEventFlags.ControlDown;
                if (args.Shift) mod |= CefEventFlags.ShiftDown;
                if (args.Alt) mod |= CefEventFlags.AltDown;
                
                CefKeyEvent kEvent = new CefKeyEvent();
                kEvent.EventType = CefKeyEventType.KeyDown;
                kEvent.Modifiers = mod;
                kEvent.WindowsKeyCode = (int)args.KeyCode;
                kEvent.NativeKeyCode = (int)args.KeyValue;
                browser._browser.GetHost().SendKeyEvent(kEvent);

                CefKeyEvent charEvent = new CefKeyEvent();
                charEvent.EventType = CefKeyEventType.Char;
                
                var key = args.KeyCode;

                if ((key == Keys.ShiftKey && _lastKey == Keys.Menu) ||
                    (key == Keys.Menu && _lastKey == Keys.ShiftKey))
                {
                    //ClassicChat.ActivateKeyboardLayout(1, 0);
                    return;
                }

                _lastKey = key;

                if (key == Keys.Escape)
                {
                    return;
                }
                /*
                var keyChar = ClassicChat.GetCharFromKey(key, Game.IsKeyPressed(Keys.ShiftKey), Game.IsKeyPressed(Keys.Menu) && Game.IsKeyPressed(Keys.ControlKey));

                if (keyChar.Length == 0 || keyChar[0] == 27) return;

                charEvent.WindowsKeyCode = keyChar[0];*/
                charEvent.Modifiers = mod;
                browser._browser.GetHost().SendKeyEvent(charEvent);
            }
        }

        internal static void OnKeyUp(KeyEventArgs args)
        {
            if (!ShowCursor) 
                return;

            foreach (var browser in CEFManager.Browsers)
            {
                if (!browser.IsInitialized()) 
                    continue;

                CefKeyEvent kEvent = new CefKeyEvent();
                kEvent.EventType = CefKeyEventType.KeyUp;
                kEvent.WindowsKeyCode = (int)args.KeyCode;
                browser._browser.GetHost().SendKeyEvent(kEvent);
            }
        }
    }
}
