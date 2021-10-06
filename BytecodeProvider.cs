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

    public interface IOpcodeProvider {
        TokenType GetTokenType(uint token);
    }

    public class V13OpcodeProvider : IOpcodeProvider {
        private static readonly Dictionary<uint, TokenType> TypeMap = new Dictionary<uint, TokenType>() {
            [0] = TokenType.Empty,
            [1] = TokenType.Identifier,
            [2] = TokenType.Constant,
            [3] = TokenType.Self,
            [4] = TokenType.BuiltInType,
            [5] = TokenType.BuiltInFunc,
            [6] = TokenType.OpIn,
            [7] = TokenType.OpEqual,
            [8] = TokenType.OpNotEqual,
            [9] = TokenType.OpLess,
            [10] = TokenType.OpLessEqual,
            [11] = TokenType.OpGreater,
            [12] = TokenType.OpGreaterEqual,
            [13] = TokenType.OpAnd,
            [14] = TokenType.OpOr,
            [15] = TokenType.OpNot,
            [16] = TokenType.OpAdd,
            [17] = TokenType.OpSub,
            [18] = TokenType.OpMul,
            [19] = TokenType.OpDiv,
            [20] = TokenType.OpMod,
            [21] = TokenType.OpShiftLeft,
            [22] = TokenType.OpShiftRight,
            [23] = TokenType.OpAssign,
            [24] = TokenType.OpAssignAdd,
            [25] = TokenType.OpAssignSub,
            [26] = TokenType.OpAssignMul,
            [27] = TokenType.OpAssignDiv,
            [28] = TokenType.OpAssignMod,
            [29] = TokenType.OpAssignShiftLeft,
            [30] = TokenType.OpAssignShiftRight,
            [31] = TokenType.OpAssignBitAnd,
            [32] = TokenType.OpAssignBitOr,
            [33] = TokenType.OpAssignBitXor,
            [34] = TokenType.OpBitAnd,
            [35] = TokenType.OpBitOr,
            [36] = TokenType.OpBitXor,
            [37] = TokenType.OpBitInvert,
            [38] = TokenType.CfIf,
            [39] = TokenType.CfElif,
            [40] = TokenType.CfElse,
            [41] = TokenType.CfFor,
            [42] = TokenType.CfWhile,
            [43] = TokenType.CfBreak,
            [44] = TokenType.CfContinue,
            [45] = TokenType.CfPass,
            [46] = TokenType.CfReturn,
            [47] = TokenType.CfMatch,
            [48] = TokenType.PrFunction,
            [49] = TokenType.PrClass,
            [50] = TokenType.PrClassName,
            [51] = TokenType.PrExtends,
            [52] = TokenType.PrIs,
            [53] = TokenType.PrOnready,
            [54] = TokenType.PrTool,
            [55] = TokenType.PrStatic,
            [56] = TokenType.PrExport,
            [57] = TokenType.PrSetget,
            [58] = TokenType.PrConst,
            [59] = TokenType.PrVar,
            [60] = TokenType.PrAs,
            [61] = TokenType.PrVoid,
            [62] = TokenType.PrEnum,
            [63] = TokenType.PrPreload,
            [64] = TokenType.PrAssert,
            [65] = TokenType.PrYield,
            [66] = TokenType.PrSignal,
            [67] = TokenType.PrBreakpoint,
            [68] = TokenType.PrRemote,
            [69] = TokenType.PrSync,
            [70] = TokenType.PrMaster,
            [71] = TokenType.PrSlave,
            [72] = TokenType.PrPuppet,
            [73] = TokenType.PrRemoteSync,
            [74] = TokenType.PrMasterSync,
            [75] = TokenType.PrPuppetSync,
            [76] = TokenType.BracketOpen,
            [77] = TokenType.BracketClose,
            [78] = TokenType.CurlyBracketOpen,
            [79] = TokenType.CurlyBracketClose,
            [80] = TokenType.ParenthesisOpen,
            [81] = TokenType.ParenthesisClose,
            [82] = TokenType.Comma,
            [83] = TokenType.Semicolon,
            [84] = TokenType.Period,
            [85] = TokenType.QuestionMark,
            [86] = TokenType.Colon,
            [87] = TokenType.Dollar,
            [88] = TokenType.ForwardArrow,
            [89] = TokenType.Newline,
            [90] = TokenType.ConstPi,
            [91] = TokenType.ConstTau,
            [92] = TokenType.Wildcard,
            [93] = TokenType.ConstInf,
            [94] = TokenType.ConstNan,
            [95] = TokenType.Error,
            [96] = TokenType.Eof,
            [97] = TokenType.Cursor,
            [98] = TokenType.Max
        };

        public TokenType GetTokenType(uint token) {
            return TypeMap[token & 0xFF];
        }
    }

    public class OpcodeProviders {
        public static readonly IOpcodeProvider ProviderV13 = new V13OpcodeProvider();
    }

    public class BytecodeProvider {
        public ITypeNameProvider TypeNameProvider;
        public IOpcodeProvider OpcodeProvider;
        public string[] BuiltInFunctions;
    }
}
