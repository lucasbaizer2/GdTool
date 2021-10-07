using System;
using System.IO;

namespace GdTool {
    public enum GdcTokenType {
        Empty,
        Identifier,
        Constant,
        Self,
        BuiltInType,
        BuiltInFunc,
        OpIn,
        OpEqual,
        OpNotEqual,
        OpLess,
        OpLessEqual,
        OpGreater,
        OpGreaterEqual,
        OpAnd,
        OpOr,
        OpNot,
        OpAdd,
        OpSub,
        OpMul,
        OpDiv,
        OpMod,
        OpShiftLeft,
        OpShiftRight,
        OpAssign,
        OpAssignAdd,
        OpAssignSub,
        OpAssignMul,
        OpAssignDiv,
        OpAssignMod,
        OpAssignShiftLeft,
        OpAssignShiftRight,
        OpAssignBitAnd,
        OpAssignBitOr,
        OpAssignBitXor,
        OpBitAnd,
        OpBitOr,
        OpBitXor,
        OpBitInvert,
        CfIf,
        CfElif,
        CfElse,
        CfFor,
        CfWhile,
        CfBreak,
        CfContinue,
        CfPass,
        CfReturn,
        CfMatch,
        PrFunction,
        PrClass,
        PrClassName,
        PrExtends,
        PrIs,
        PrOnready,
        PrTool,
        PrStatic,
        PrExport,
        PrSetget,
        PrConst,
        PrVar,
        PrAs,
        PrVoid,
        PrEnum,
        PrPreload,
        PrAssert,
        PrYield,
        PrSignal,
        PrBreakpoint,
        PrRemote,
        PrSync,
        PrMaster,
        PrSlave,
        PrPuppet,
        PrRemotesync,
        PrMastersync,
        PrPuppetsync,
        BracketOpen,
        BracketClose,
        CurlyBracketOpen,
        CurlyBracketClose,
        ParenthesisOpen,
        ParenthesisClose,
        Comma,
        Semicolon,
        Period,
        QuestionMark,
        Colon,
        Dollar,
        ForwardArrow,
        Newline,
        ConstPi,
        ConstTau,
        Wildcard,
        ConstInf,
        ConstNan,
        Error,
        Eof,
        Cursor,
        Max
    }

    public class GdcToken {
        public uint LineCol;
        public IGdStructure Operand;
        public GdcTokenType Type;
        public uint Data;

        public void Compile(BinaryWriter writer, BytecodeProvider provider) {
            writer.Write(provider.TokenTypeProvider.GetTokenId(Type) & (Data << 8));
            if (Operand != null) {
                Operand.Serialize(writer, provider);
            }
        }

        public DecompileBuffer Decompile(DecompileBuffer buf, GdcTokenType previous, BytecodeProvider provider) {
            switch (Type) {
                case GdcTokenType.Empty:
                    return buf;
                case GdcTokenType.Identifier:
                    return buf.Append(Operand.ToString());
                case GdcTokenType.Constant:
                    return buf.Append(Operand.ToString());
                case GdcTokenType.Self:
                    return buf.Append("self");
                case GdcTokenType.BuiltInType:
                    return buf.Append(provider.TypeNameProvider.GetTypeName(Data));
                case GdcTokenType.BuiltInFunc:
                    return buf.Append(provider.BuiltInFunctions[Data]);
                case GdcTokenType.OpIn:
                    return buf.AppendOp("in");
                case GdcTokenType.OpEqual:
                    return buf.AppendOp("==");
                case GdcTokenType.OpNotEqual:
                    return buf.AppendOp("!=");
                case GdcTokenType.OpLess:
                    return buf.AppendOp("<");
                case GdcTokenType.OpLessEqual:
                    return buf.AppendOp("<=");
                case GdcTokenType.OpGreater:
                    return buf.AppendOp(">");
                case GdcTokenType.OpGreaterEqual:
                    return buf.AppendOp(">=");
                case GdcTokenType.OpAnd:
                    return buf.AppendOp("and");
                case GdcTokenType.OpOr:
                    return buf.AppendOp("or");
                case GdcTokenType.OpNot:
                    return buf.AppendOp("not");
                case GdcTokenType.OpAdd:
                    return buf.AppendOp("+");
                case GdcTokenType.OpSub:
                    return buf.AppendOp("-");
                case GdcTokenType.OpMul:
                    return buf.AppendOp("*");
                case GdcTokenType.OpDiv:
                    return buf.AppendOp("/");
                case GdcTokenType.OpMod:
                    return buf.AppendOp("%");
                case GdcTokenType.OpShiftLeft:
                    return buf.AppendOp("<<");
                case GdcTokenType.OpShiftRight:
                    return buf.AppendOp(">>");
                case GdcTokenType.OpAssign:
                    return buf.AppendOp("=");
                case GdcTokenType.OpAssignAdd:
                    return buf.AppendOp("+=");
                case GdcTokenType.OpAssignSub:
                    return buf.AppendOp("-=");
                case GdcTokenType.OpAssignMul:
                    return buf.AppendOp("*=");
                case GdcTokenType.OpAssignDiv:
                    return buf.AppendOp("/=");
                case GdcTokenType.OpAssignMod:
                    return buf.AppendOp("%=");
                case GdcTokenType.OpAssignShiftLeft:
                    return buf.AppendOp("<<=");
                case GdcTokenType.OpAssignShiftRight:
                    return buf.AppendOp(">>=");
                case GdcTokenType.OpAssignBitAnd:
                    return buf.AppendOp("&=");
                case GdcTokenType.OpAssignBitOr:
                    return buf.AppendOp("|=");
                case GdcTokenType.OpAssignBitXor:
                    return buf.AppendOp("^=");
                case GdcTokenType.OpBitAnd:
                    return buf.AppendOp("&");
                case GdcTokenType.OpBitOr:
                    return buf.AppendOp("|");
                case GdcTokenType.OpBitXor:
                    return buf.AppendOp("^");
                case GdcTokenType.OpBitInvert:
                    return buf.AppendOp("!");
                case GdcTokenType.CfIf:
                    if (previous != GdcTokenType.Newline) {
                        return buf.AppendOp("if");
                    } else {
                        return buf.Append("if ");
                    }
                case GdcTokenType.CfElif:
                    if (previous != GdcTokenType.Newline) {
                        return buf.AppendOp("elif");
                    } else {
                        return buf.Append("elif ");
                    }
                case GdcTokenType.CfElse:
                    if (previous != GdcTokenType.Newline) {
                        return buf.Append(" else");
                    } else {
                        return buf.Append("else");
                    }
                case GdcTokenType.CfFor:
                    return buf.Append("for ");
                case GdcTokenType.CfWhile:
                    return buf.Append("while ");
                case GdcTokenType.CfBreak:
                    return buf.Append("break");
                case GdcTokenType.CfContinue:
                    return buf.Append("continue");
                case GdcTokenType.CfPass:
                    return buf.Append("pass");
                case GdcTokenType.CfReturn:
                    return buf.Append("return ");
                case GdcTokenType.CfMatch:
                    return buf.Append("match ");
                case GdcTokenType.PrFunction:
                    return buf.Append("func ");
                case GdcTokenType.PrClass:
                    return buf.Append("class ");
                case GdcTokenType.PrClassName:
                    return buf.Append("class_name ");
                case GdcTokenType.PrExtends:
                    return buf.Append("extends ");
                case GdcTokenType.PrIs:
                    return buf.AppendOp("is");
                case GdcTokenType.PrOnready:
                    return buf.Append("onready ");
                case GdcTokenType.PrTool:
                    return buf.Append("tool ");
                case GdcTokenType.PrStatic:
                    return buf.Append("static ");
                case GdcTokenType.PrExport:
                    return buf.Append("export ");
                case GdcTokenType.PrSetget:
                    return buf.AppendOp("setget");
                case GdcTokenType.PrConst:
                    return buf.Append("const ");
                case GdcTokenType.PrVar:
                    return buf.Append("var ");
                case GdcTokenType.PrAs:
                    return buf.AppendOp("as");
                case GdcTokenType.PrVoid:
                    return buf.Append("void ");
                case GdcTokenType.PrEnum:
                    return buf.Append("enum ");
                case GdcTokenType.PrPreload:
                    return buf.Append("preload");
                case GdcTokenType.PrAssert:
                    return buf.Append("assert ");
                case GdcTokenType.PrYield:
                    return buf.Append("yield ");
                case GdcTokenType.PrSignal:
                    return buf.Append("signal ");
                case GdcTokenType.PrBreakpoint:
                    return buf.Append("breakpoint ");
                case GdcTokenType.PrRemote:
                    return buf.Append("remote ");
                case GdcTokenType.PrSync:
                    return buf.Append("sync ");
                case GdcTokenType.PrMaster:
                    return buf.Append("master ");
                case GdcTokenType.PrSlave:
                    return buf.Append("slave ");
                case GdcTokenType.PrPuppet:
                    return buf.Append("puppet ");
                case GdcTokenType.PrRemotesync:
                    return buf.Append("remotesync ");
                case GdcTokenType.PrMastersync:
                    return buf.Append("mastersync ");
                case GdcTokenType.PrPuppetsync:
                    return buf.Append("puppetsync ");
                case GdcTokenType.BracketOpen:
                    return buf.Append("[");
                case GdcTokenType.BracketClose:
                    return buf.Append("]");
                case GdcTokenType.CurlyBracketOpen:
                    return buf.AppendOp("{");
                case GdcTokenType.CurlyBracketClose:
                    return buf.Append("}");
                case GdcTokenType.ParenthesisOpen:
                    return buf.Append("(");
                case GdcTokenType.ParenthesisClose:
                    return buf.Append(")");
                case GdcTokenType.Comma:
                    return buf.Append(", ");
                case GdcTokenType.Semicolon:
                    return buf.Append(";");
                case GdcTokenType.Period:
                    return buf.Append(".");
                case GdcTokenType.QuestionMark:
                    return buf.Append("?");
                case GdcTokenType.Colon:
                    return buf.Append(":");
                case GdcTokenType.Dollar:
                    return buf.Append("$");
                case GdcTokenType.ForwardArrow:
                    return buf.Append("->");
                case GdcTokenType.Newline:
                    buf.Indentation = (int)Data;
                    buf.AppendNewLine();
                    return buf;
                case GdcTokenType.ConstPi:
                    return buf.Append("PI");
                case GdcTokenType.ConstTau:
                    return buf.Append("TAU");
                case GdcTokenType.Wildcard:
                    return buf.Append("*");
                case GdcTokenType.ConstInf:
                    return buf.Append("INF");
                case GdcTokenType.ConstNan:
                    return buf.Append("NAN");
                case GdcTokenType.Error:
                case GdcTokenType.Eof:
                case GdcTokenType.Cursor:
                case GdcTokenType.Max:
                    return buf;
                default:
                    throw new NotImplementedException(Type.ToString());
            }
        }
    }

    public class TokenTree {

    }
}
