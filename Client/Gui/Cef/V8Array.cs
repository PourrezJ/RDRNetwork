using Xilium.CefGlue;

namespace RDRNetwork.Gui.Cef
{
    internal class V8Array
    {
        private CefV8Value _value;

        internal V8Array(CefV8Value val)
        {
            _value = val;
        }

        public object this[int index] => _value.GetValue(index).GetValue();

        public int length => _value.GetArrayLength();
    }
}
