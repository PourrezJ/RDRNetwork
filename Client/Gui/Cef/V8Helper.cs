using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace RDRNetwork.Gui.Cef
{
    public static class V8Helper
    {
        public static object GetValue(this CefV8Value val)
        {
            if (val.IsNull || val.IsUndefined) return null;

            if (val.IsArray) return new V8Array(val);
            if (val.IsBool) return val.GetBoolValue();
            if (val.IsDouble) return val.GetDoubleValue();
            if (val.IsInt) return val.GetIntValue();
            if (val.IsString) return val.GetStringValue();
            if (val.IsUInt) return val.GetUIntValue();

            return null;
        }

        public static CefV8Value CreateValue(object value)
        {
            if (value == null)
                return CefV8Value.CreateNull();
            if (value is bool)
                return CefV8Value.CreateBool((bool)value);
            if (value is double)
                return CefV8Value.CreateDouble((double)value);
            if (value is float)
                return CefV8Value.CreateDouble((double)(float)value);
            if (value is int)
                return CefV8Value.CreateInt((int)value);
            var s = value as string;
            if (s != null)
                return CefV8Value.CreateString(s);
            if (value is uint)
                return CefV8Value.CreateUInt((uint)value);
            var list = value as IList;
            if (list == null) return CefV8Value.CreateUndefined();
            var val = list;

            var arr = CefV8Value.CreateArray(val.Count);

            for (var i = 0; i < val.Count; i++)
            {
                arr.SetValue(i, CreateValue(val[i]));
            }

            return arr;
        }
    }
}
