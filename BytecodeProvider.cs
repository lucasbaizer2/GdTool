using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GdTool {
    public interface ITypeNameProvider {
        string GetTypeName(uint typeId);
    }

    internal class V3TypeNameProvider : ITypeNameProvider {
        public string GetTypeName(uint typeId) {
            switch (typeId) {
                case 0:
                    return "Nil";
                case 1:
                    return "bool";
                case 2:
                    return "int";
                case 3:
                    return "float";
                case 4:
                    return "String";
                case 5:
                    return "Vector2";
                case 6:
                    return "Rect2";
                case 7:
                    return "Transform2D";
                case 8:
                    return "Vector3";
                case 9:
                    return "Plane";
                case 10:
                    return "AABB";
                case 11:
                    return "Quat";
                case 12:
                    return "Basis";
                case 13:
                    return "Transform";
                case 14:
                    return "Color";
                case 15:
                    return "RID";
                case 16:
                    return "Object";
                case 17:
                    return "NodePath";
                case 18:
                    return "Dictionary";
                case 19:
                    return "Array";
                case 20:
                    return "PoolByteArray";
                case 21:
                    return "PoolIntArray";
                case 22:
                    return "PoolRealArray";
                case 23:
                    return "PoolStringArray";
                case 24:
                    return "PoolVector2Array";
                case 25:
                    return "PoolVector3Array";
                case 26:
                    return "PoolColorArray";
                default:
                    throw new InvalidOperationException("invalid type: " + typeId);
            }
        }
    }

    public class TypeNameProviders {
        public static readonly ITypeNameProvider ProviderV3 = new V3TypeNameProvider();
    }

    public class BytecodeProvider {
        public ITypeNameProvider TypeNameProvider;
        public string[] BuiltInFunctions;
    }
}
