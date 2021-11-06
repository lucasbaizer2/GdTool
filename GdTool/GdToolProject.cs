using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GdTool {
    public class GdToolProject {
        [JsonProperty]
        public uint PackFormatVersion;
        [JsonProperty]
        public uint VersionMajor;
        [JsonProperty]
        public uint VersionMinor;
        [JsonProperty]
        public uint VersionPatch;
    }
}
