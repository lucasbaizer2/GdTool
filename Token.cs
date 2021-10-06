using System;

namespace GdTool {
    public enum TokenType {
        Empty = 0,
        Identifier = 1,
        Constant = 2,
        Self = 3,
        BuiltInType = 4,
        BuiltInFunc = 5,
        OpIn = 6,
        OpEqual = 7,
        OpNotEqual = 8,
        OpLess = 9,
        OpLessEqual = 10,
        OpGreater = 11,
        OpGreaterEqual = 12,
        OpAnd = 13,
        OpOr = 14,
        OpNot = 15,
        OpAdd = 16,
        OpSub = 17,
        OpMul = 18,
        OpDiv = 19,
        OpMod = 20,
        OpShiftLeft = 21,
        OpShiftRight = 22,
        OpAssign = 23,
        OpAssignAdd = 24,
        OpAssignSub = 25,
        OpAssignMul = 26,
        OpAssignDiv = 27,
        OpAssignMod = 28,
        OpAssignShiftLeft = 29,
        OpAssignShiftRight = 30,
        OpAssignBitAnd = 31,
        OpAssignBitOr = 32,
        OpAssignBitXor = 33,
        OpBitAnd = 34,
        OpBitOr = 35,
        OpBitXor = 36,
        OpBitInvert = 37,
        CfIf = 38,
        CfElif = 39,
        CfElse = 40,
        CfFor = 41,
        CfWhile = 42,
        CfBreak = 43,
        CfContinue = 44,
        CfPass = 45,
        CfReturn = 46,
        CfMatch = 47,
        PrFunction = 48,
        PrClass = 49,
        PrClassName = 50,
        PrExtends = 51,
        PrIs = 52,
        PrOnready = 53,
        PrTool = 54,
        PrStatic = 55,
        PrExport = 56,
        PrSetget = 57,
        PrConst = 58,
        PrVar = 59,
        PrAs = 60,
        PrVoid = 61,
        PrEnum = 62,
        PrPreload = 63,
        PrAssert = 64,
        PrYield = 65,
        PrSignal = 66,
        PrBreakpoint = 67,
        PrRemote = 68,
        PrSync = 69,
        PrMaster = 70,
        PrSlave = 71,
        PrPuppet = 72,
        PrRemoteSync = 73,
        PrMasterSync = 74,
        PrPuppetSync = 75,
        BracketOpen = 76,
        BracketClose = 77,
        CurlyBracketOpen = 78,
        CurlyBracketClose = 79,
        ParenthesisOpen = 80,
        ParenthesisClose = 81,
        Comma = 82,
        Semicolon = 83,
        Period = 84,
        QuestionMark = 85,
        Colon = 86,
        Dollar = 87,
        ForwardArrow = 88,
        Newline = 89,
        ConstPi = 90,
        ConstTau = 91,
        Wildcard = 92,
        ConstInf = 93,
        ConstNan = 94,
        Error = 95,
        Eof = 96,
        Cursor = 97,
        Max = 98
    }

    public class Token {
        private static readonly string[] FunctionNames = new string[] {
            "sin",
            "cos",
            "tan",
            "sinh",
            "cosh",
            "tanh",
            "asin",
            "acos",
            "atan",
            "atan2",
            "sqrt",
            "fmod",
            "fposmod",
            "posmod",
            "floor",
            "ceil",
            "round",
            "abs",
            "sign",
            "pow",
            "log",
            "exp",
            "is_nan",
            "is_inf",
            "is_equal_approx",
            "is_zero_approx",
            "ease",
            "decimals",
            "step_decimals",
            "stepify",
            "lerp",
            "lerp_angle",
            "inverse_lerp",
            "range_lerp",
            "smoothstep",
            "move_toward",
            "dectime",
            "randomize",
            "randi",
            "randf",
            "rand_range",
            "seed",
            "rand_seed",
            "deg2rad",
            "rad2deg",
            "linear2db",
            "db2linear",
            "polar2cartesian",
            "cartesian2polar",
            "wrapi",
            "wrapf",
            "max",
            "min",
            "clamp",
            "nearest_po2",
            "weakref",
            "funcref",
            "convert",
            "typeof",
            "type_exists",
            "char",
            "ord",
            "str",
            "print",
            "printt",
            "prints",
            "printerr",
            "printraw",
            "print_debug",
            "push_error",
            "push_warning",
            "var2str",
            "str2var",
            "var2bytes",
            "bytes2var",
            "range",
            "load",
            "inst2dict",
            "dict2inst",
            "validate_json",
            "parse_json",
            "to_json",
            "hash",
            "Color8",
            "ColorN",
            "print_stack",
            "get_stack",
            "instance_from_id",
            "len",
            "is_instance_valid",
        };

        public uint LineCol;
        public object Operand;

        internal uint Definition;

        public TokenType Type {
            get {
                return (TokenType)(Definition & 0xFF);
            }
        }
        public uint Data {
            get {
                return Definition >> 8;
            }
        }

        public DecompileBuffer Decompile(DecompileBuffer buf, TokenType previous, BytecodeProvider provider) {
            switch (Type) {
                case TokenType.Empty:
                    return buf;
                case TokenType.Identifier:
                    return buf.Append(Operand.ToString());
                case TokenType.Constant:
                    return buf.Append(Operand.ToString());
                case TokenType.Self:
                    return buf.Append("self");
                case TokenType.BuiltInType:
                    return buf.Append(provider.TypeNameProvider.GetTypeName(Data));
                case TokenType.BuiltInFunc:
                    return buf.Append(FunctionNames[Data]);
                case TokenType.OpIn:
                    return buf.AppendOp("in");
                case TokenType.OpEqual:
                    return buf.AppendOp("==");
                case TokenType.OpNotEqual:
                    return buf.AppendOp("!=");
                case TokenType.OpLess:
                    return buf.AppendOp("<");
                case TokenType.OpLessEqual:
                    return buf.AppendOp("<=");
                case TokenType.OpGreater:
                    return buf.AppendOp(">");
                case TokenType.OpGreaterEqual:
                    return buf.AppendOp(">=");
                case TokenType.OpAnd:
                    return buf.AppendOp("and");
                case TokenType.OpOr:
                    return buf.AppendOp("or");
                case TokenType.OpNot:
                    return buf.AppendOp("not");
                case TokenType.OpAdd:
                    return buf.AppendOp("+");
                case TokenType.OpSub:
                    return buf.AppendOp("-");
                case TokenType.OpMul:
                    return buf.AppendOp("*");
                case TokenType.OpDiv:
                    return buf.AppendOp("/");
                case TokenType.OpMod:
                    return buf.AppendOp("%");
                case TokenType.OpShiftLeft:
                    return buf.AppendOp("<<");
                case TokenType.OpShiftRight:
                    return buf.AppendOp(">>");
                case TokenType.OpAssign:
                    return buf.AppendOp("=");
                case TokenType.OpAssignAdd:
                    return buf.AppendOp("+=");
                case TokenType.OpAssignSub:
                    return buf.AppendOp("-=");
                case TokenType.OpAssignMul:
                    return buf.AppendOp("*=");
                case TokenType.OpAssignDiv:
                    return buf.AppendOp("/=");
                case TokenType.OpAssignMod:
                    return buf.AppendOp("%=");
                case TokenType.OpAssignShiftLeft:
                    return buf.AppendOp("<<=");
                case TokenType.OpAssignShiftRight:
                    return buf.AppendOp(">>=");
                case TokenType.OpAssignBitAnd:
                    return buf.AppendOp("&=");
                case TokenType.OpAssignBitOr:
                    return buf.AppendOp("|=");
                case TokenType.OpAssignBitXor:
                    return buf.AppendOp("^=");
                case TokenType.OpBitAnd:
                    return buf.AppendOp("&");
                case TokenType.OpBitOr:
                    return buf.AppendOp("|");
                case TokenType.OpBitXor:
                    return buf.AppendOp("^");
                case TokenType.OpBitInvert:
                    return buf.AppendOp("!");
                case TokenType.CfIf:
                    if (previous != TokenType.Newline) {
                        return buf.AppendOp("if");
                    } else {
                        return buf.Append("if ");
                    }
                case TokenType.CfElif:
                    if (previous != TokenType.Newline) {
                        return buf.AppendOp("elif");
                    } else {
                        return buf.Append("elif ");
                    }
                case TokenType.CfElse:
                    if (previous != TokenType.Newline) {
                        return buf.Append(" else");
                    } else {
                        return buf.Append("else");
                    }
                case TokenType.CfFor:
                    return buf.Append("for ");
                case TokenType.CfWhile:
                    return buf.Append("while ");
                case TokenType.CfBreak:
                    return buf.Append("break");
                case TokenType.CfContinue:
                    return buf.Append("continue");
                case TokenType.CfPass:
                    return buf.Append("pass");
                case TokenType.CfReturn:
                    return buf.Append("return ");
                case TokenType.CfMatch:
                    return buf.Append("match ");
                case TokenType.PrFunction:
                    return buf.Append("func ");
                case TokenType.PrClass:
                    return buf.Append("class ");
                case TokenType.PrClassName:
                    return buf.Append("class_name ");
                case TokenType.PrExtends:
                    return buf.Append("extends ");
                case TokenType.PrIs:
                    return buf.AppendOp("is");
                case TokenType.PrOnready:
                    return buf.Append("onready ");
                case TokenType.PrTool:
                    return buf.Append("tool ");
                case TokenType.PrStatic:
                    return buf.Append("static ");
                case TokenType.PrExport:
                    return buf.Append("export ");
                case TokenType.PrSetget:
                    return buf.AppendOp("setget");
                case TokenType.PrConst:
                    return buf.Append("const ");
                case TokenType.PrVar:
                    return buf.Append("var ");
                case TokenType.PrAs:
                    return buf.AppendOp("as");
                case TokenType.PrVoid:
                    return buf.Append("void ");
                case TokenType.PrEnum:
                    return buf.Append("enum ");
                case TokenType.PrPreload:
                    return buf.Append("preload");
                case TokenType.PrAssert:
                    return buf.Append("assert ");
                case TokenType.PrYield:
                    return buf.Append("yield ");
                case TokenType.PrSignal:
                    return buf.Append("signal ");
                case TokenType.PrBreakpoint:
                    return buf.Append("breakpoint ");
                case TokenType.PrRemote:
                    return buf.Append("remote ");
                case TokenType.PrSync:
                    return buf.Append("sync ");
                case TokenType.PrMaster:
                    return buf.Append("master ");
                case TokenType.PrSlave:
                    return buf.Append("slave ");
                case TokenType.PrPuppet:
                    return buf.Append("puppet ");
                case TokenType.PrRemoteSync:
                    return buf.Append("remotesync ");
                case TokenType.PrMasterSync:
                    return buf.Append("mastersync ");
                case TokenType.PrPuppetSync:
                    return buf.Append("puppetsync ");
                case TokenType.BracketOpen:
                    return buf.Append("[");
                case TokenType.BracketClose:
                    return buf.Append("]");
                case TokenType.CurlyBracketOpen:
                    return buf.Append("{");
                case TokenType.CurlyBracketClose:
                    return buf.Append("}");
                case TokenType.ParenthesisOpen:
                    return buf.Append("(");
                case TokenType.ParenthesisClose:
                    return buf.Append(")");
                case TokenType.Comma:
                    return buf.Append(", ");
                case TokenType.Semicolon:
                    return buf.Append(";");
                case TokenType.Period:
                    return buf.Append(".");
                case TokenType.QuestionMark:
                    return buf.Append("?");
                case TokenType.Colon:
                    return buf.Append(":");
                case TokenType.Dollar:
                    return buf.Append("$");
                case TokenType.ForwardArrow:
                    return buf.Append("->");
                case TokenType.Newline:
                    buf.Indentation = (int)Data;
                    buf.AppendNewLine();
                    return buf;
                case TokenType.ConstPi:
                    return buf.Append("PI");
                case TokenType.ConstTau:
                    return buf.Append("TAU");
                case TokenType.Wildcard:
                    return buf.Append("*");
                case TokenType.ConstInf:
                    return buf.Append("INF");
                case TokenType.ConstNan:
                    return buf.Append("NAN");
                case TokenType.Error:
                case TokenType.Eof:
                case TokenType.Cursor:
                case TokenType.Max:
                    return buf;
                default:
                    throw new NotImplementedException(Type.ToString());
            }
        }
    }

    public class TokenTree {

    }
}
