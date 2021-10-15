using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace GdTool {
    public interface ITypeNameProvider {
        string GetTypeName(uint typeId);

        uint GetTypeId(string name);

        string[] GetAllTypeNames();
    }

    internal class V3TypeNameProvider : ITypeNameProvider {
        private static readonly string[] TypeNames = {
            "Nil", "bool", "int", "float", "String", "Vector2", "Rect2", "Vector3", "Transform2D",
            "Plane", "Quat", "AABB", "Basis", "Transform", "Color", "NodePath", "RID", "Object",
            "Dictionary", "Array", "PoolByteArray", "PoolIntArray", "PoolRealArray", "PoolStringArray",
            "PoolVector2Array", "PoolVector3Array", "PoolColorArray"
        };

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
        public static readonly ITypeNameProvider ProviderV3 = new V3TypeNameProvider();
    }

#pragma warning disable CS0649
    internal class BytecodeFile {
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
#pragma warning restore CS0649

    public interface ITokenTypeProvider {
        GdcTokenType GetTokenType(uint token);

        uint GetTokenId(GdcTokenType type);
    }

    internal class DefaultTokenTypeProvider : ITokenTypeProvider {
        public BytecodeFile File;

        public DefaultTokenTypeProvider(BytecodeFile file) {
            File = file;
        }

        public GdcTokenType GetTokenType(uint token) {
            return (GdcTokenType)Enum.Parse(typeof(GdcTokenType), File.Tokens[token]);
        }

        public uint GetTokenId(GdcTokenType type) {
            string name = type.ToString();
            return (uint)Array.IndexOf(File.Tokens, name);
        }
    }

    public class BytecodeProvider {
        public ITypeNameProvider TypeNameProvider;
        public ITokenTypeProvider TokenTypeProvider;
        public string[] BuiltInFunctions;
        public uint BytecodeVersion;

        public BytecodeProvider(uint commitHash) {
            byte[] jsonBytes = ReadEmbeddedSource("GdTool.Resources.bytecode_" + commitHash.ToString("x") + ".json");
            if (jsonBytes == null) {
                throw new InvalidOperationException("Invalid commit hash: " + commitHash.ToString("x"));
            }
            string jsonString = Encoding.UTF8.GetString(jsonBytes);
            jsonString = jsonString.Substring(jsonString.IndexOf('{'));
            BytecodeFile file = JsonConvert.DeserializeObject<BytecodeFile>(jsonString);

            switch (file.TypeNameVersion) {
                case 3:
                    TypeNameProvider = TypeNameProviders.ProviderV3;
                    break;
                default:
                    throw new InvalidOperationException("Invalid type name version");
            }
            TokenTypeProvider = new DefaultTokenTypeProvider(file);
            BuiltInFunctions = file.FunctionNames;
            BytecodeVersion = file.BytecodeVersion;
        }

        private static byte[] ReadEmbeddedSource(string name) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly.GetManifestResourceInfo(name) == null) {
                return null;
            }
            using (Stream stream = assembly.GetManifestResourceStream(name)) {
                using (MemoryStream mem = new MemoryStream()) {
                    stream.CopyTo(mem);
                    return mem.ToArray();
                }
            }
        }
    }
}
