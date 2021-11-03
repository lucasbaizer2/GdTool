using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace GdTool {
    public interface ITypeNameProvider {
        string GetTypeName(uint typeId);

        uint GetTypeId(string name);

        string[] GetAllTypeNames();
    }

    internal class ArrayTypeNameProvider : ITypeNameProvider {
        private string[] TypeNames;

        public ArrayTypeNameProvider(string[] typeNames) {
            TypeNames = typeNames;
        }

        public string GetTypeName(uint typeId) {
            if (typeId >= TypeNames.Length) {
                throw new InvalidOperationException("invalid type: " + typeId);
            }
            return TypeNames[typeId];
        }

        public uint GetTypeId(string name) {
            int id = Array.IndexOf(TypeNames, name);
            if (id == -1) {
                throw new InvalidOperationException("invalid type: " + name);
            }
            return (uint)id;
        }

        public string[] GetAllTypeNames() {
            return TypeNames;
        }
    }

    public class TypeNameProviders {
        public static readonly ITypeNameProvider ProviderV3 = new ArrayTypeNameProvider(new string[] {
            "Nil", "bool", "int", "float", "String", "Vector2", "Rect2", "Vector3", "Transform2D",
            "Plane", "Quat", "AABB", "Basis", "Transform", "Color", "NodePath", "RID", "Object",
            "Dictionary", "Array", "PoolByteArray", "PoolIntArray", "PoolRealArray", "PoolStringArray",
            "PoolVector2Array", "PoolVector3Array", "PoolColorArray" });

        public static readonly ITypeNameProvider ProviderV2 = new ArrayTypeNameProvider(new string[] {
            "Nil", "bool", "int", "float", "String", "Vector2", "Rect2", "Vector3", "Matrix32", "Plane",
            "Quat", "AABB", "Matrix3", "Transform", "Color", "Image", "NodePath", "RID", "Object",
            "InputEvent", "Dictionary", "Array", "RawArray", "IntArray", "FloatArray", "StringArray",
            "Vector2Array", "Vector3Array", "ColorArray" });
    }

    public class BytecodeProviderData {
        [JsonProperty]
        public string CommitHash;
        [JsonProperty]
        public int TypeNameVersion;
        [JsonProperty]
        public string Description;
        [JsonProperty]
        public string[] FunctionNames;
        [JsonProperty]
        public string[] Tokens;
        [JsonProperty]
        public uint BytecodeVersion;
    }

    public class CommitHistory {
        [JsonProperty]
        public string Branch;
        [JsonProperty]
        public string[] History;
    }

    public interface ITokenTypeProvider {
        GdcTokenType GetTokenType(uint token);

        int GetTokenId(GdcTokenType type);
    }

    internal class DefaultTokenTypeProvider : ITokenTypeProvider {
        public BytecodeProviderData File;

        public DefaultTokenTypeProvider(BytecodeProviderData file) {
            File = file;
        }

        public GdcTokenType GetTokenType(uint token) {
            if (token < 0 || token >= File.Tokens.Length) {
                throw new InvalidOperationException("token id not defined in current bytecode version: " + token);
            }
            if (Enum.TryParse(File.Tokens[token], out GdcTokenType result)) {
                return result;
            }
            throw new InvalidOperationException("token not defined in current bytecode version: " + File.Tokens[token]);
        }

        public int GetTokenId(GdcTokenType type) {
            string name = type.ToString();
            int id = Array.IndexOf(File.Tokens, name);
            if (id == -1) {
                throw new InvalidOperationException("token type not defined in current bytecode version: " + type);
            }
            return id;
        }
    }

    public class BytecodeProvider {
        private static readonly Dictionary<string, string> ProviderShortFormLookup = new Dictionary<string, string>();
        private static readonly Dictionary<string, BytecodeProvider> Providers = new Dictionary<string, BytecodeProvider>();
        private static readonly List<CommitHistory> CommitHistories = new List<CommitHistory>();

        private static byte[] ReadEmbeddedResource(string name) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(name)) {
                using (MemoryStream mem = new MemoryStream()) {
                    stream.CopyTo(mem);
                    return mem.ToArray();
                }
            }
        }

        private static T ReadEmbeddedJson<T>(string name) {
            byte[] raw = ReadEmbeddedResource(name);
            string jsonString = Encoding.UTF8.GetString(raw);
            jsonString = jsonString.Substring(jsonString.IndexOf('{'));
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        static BytecodeProvider() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] names = assembly.GetManifestResourceNames();
            foreach (string name in names) {
                if (name.EndsWith(".json")) {
                    if (name.StartsWith("GdTool.Resources.BytecodeProviders")) {
                        BytecodeProviderData file = ReadEmbeddedJson<BytecodeProviderData>(name);

                        Providers[file.CommitHash] = new BytecodeProvider(file);
                        ProviderShortFormLookup[file.CommitHash.Substring(0, 7)] = file.CommitHash;
                    } else if (name.StartsWith("GdTool.Resources.CommitHistories")) {
                        CommitHistory history = ReadEmbeddedJson<CommitHistory>(name);
                        CommitHistories.Add(history);
                    }
                }
            }
        }

        public ITypeNameProvider TypeNameProvider { get; private set; }
        public ITokenTypeProvider TokenTypeProvider { get; private set; }
        public BytecodeProviderData ProviderData { get; private set; }

        public BytecodeProvider(BytecodeProviderData file) {
            switch (file.TypeNameVersion) {
                case 3:
                    TypeNameProvider = TypeNameProviders.ProviderV3;
                    break;
                case 2:
                    TypeNameProvider = TypeNameProviders.ProviderV2;
                    break;
                default:
                    throw new InvalidOperationException("Invalid type name version");
            }

            ProviderData = file;
            TokenTypeProvider = new DefaultTokenTypeProvider(file);
        }

        public static bool HasProviderForHash(string hash) {
            if (hash.Length == 7) {
                return ProviderShortFormLookup.ContainsKey(hash);
            } else if (hash.Length == 40) {
                return Providers.ContainsKey(hash);
            }
            throw new InvalidOperationException("commit hash must be either 7 (shortened form) or 40 (full form) hex characters");
        }

        public static BytecodeProvider GetByCommitHash(string hash) {
            if (hash.Length != 7 && hash.Length != 40) {
                throw new InvalidOperationException("commit hash must be either 7 (shortened form) or 40 (full form) hex characters");
            }
            if (hash.Length == 7) {
                string shortHash = hash;
                if (!ProviderShortFormLookup.TryGetValue(shortHash, out hash)) {
                    throw new InvalidOperationException("there is no bytecode version associated with the given commit hash: " + shortHash);
                }
            }
            if (!Providers.TryGetValue(hash, out BytecodeProvider provider)) {
                throw new InvalidOperationException("there is no bytecode version associated with the given commit hash: " + hash);
            }
            return provider;
        }

        public static string FindPreviousMajorVersionHash(string hash) {
            if (hash.Length < 7) {
                throw new InvalidOperationException("commit hash must be 7 or more hex characters");
            }

            foreach (CommitHistory history in CommitHistories) {
                bool seenRequestedHash = false;
                foreach (string version in history.History) {
                    if (version.StartsWith(hash)) {
                        seenRequestedHash = true;
                    }
                    if (seenRequestedHash && Providers.ContainsKey(version)) {
                        return version;
                    }
                }
            }

            return null;
        }
    }
}
