using Microsoft.ClearScript.V8;

namespace RDRNetwork.Javascript
{
    internal class ClientsideScriptWrapper
    {
        internal ClientsideScriptWrapper(V8ScriptEngine en, string rs, string filename)
        {
            Engine = en;
            ResourceParent = rs;
            Filename = filename;
        }

        internal V8ScriptEngine Engine { get; set; }
        internal string ResourceParent { get; set; }
        internal string Filename { get; set; }
    }
}
