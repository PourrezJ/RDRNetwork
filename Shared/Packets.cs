using System;
using System.Collections.Generic;

namespace RDRN_Shared
{
    public class ClientsideScript
    {
        public string ResourceParent { get; set; }

        public string Script { get; set; }

        public string Filename { get; set; }

        public string MD5Hash { get; set; }
    }

    public class ScriptCollection
    {
        public List<ClientsideScript> ClientsideScripts { get; set; }
    }
}
