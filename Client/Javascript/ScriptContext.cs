using Microsoft.ClearScript.V8;

namespace RDRNetwork.Javascript
{
    public class ScriptContext
    {
        public ScriptContext(V8ScriptEngine engine)
        {
            Engine = engine;
        }

        internal bool isDisposing;
        internal string ParentResourceName;
        internal V8ScriptEngine Engine;

    }
}
