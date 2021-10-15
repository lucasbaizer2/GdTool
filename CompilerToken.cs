using System;
using System.IO;
using System.Text;

namespace GdTool {
    public class CompilerTokenData {
        public ICompilerToken Creator;
        public uint Data;
        public IGdStructure Operand;

        public CompilerTokenData(ICompilerToken creator) {
            Creator = creator;
        }
    }

    public interface ICompilerToken {
        CompilerTokenData Parse(SourceCodeReader reader, BytecodeProvider provider);

        void Write(BinaryWriter writer, BytecodeProvider provider, CompilerTokenData data);
    }

    public class BasicCompilerToken : ICompilerToken {
        public GdcTokenType? Type;
        public string Value;

        public BasicCompilerToken(GdcTokenType? type, string value) {
            Type = type;
            Value = value;
        }

        public virtual CompilerTokenData Parse(SourceCodeReader reader, BytecodeProvider provider) {
            string peek = reader.Peek(Value.Length);
            if (peek != null && peek == Value) {
                reader.Position += Value.Length;
                return new CompilerTokenData(this);
            }
            return null;
        }

        public void Write(BinaryWriter writer, BytecodeProvider provider, CompilerTokenData data) {
            if (Type != null) {
                writer.Write((byte)provider.TokenTypeProvider.GetTokenId(Type.Value));
            }
        }
    }

    public class WildcardCompilerToken : ICompilerToken {
        public virtual CompilerTokenData Parse(SourceCodeReader reader, BytecodeProvider provider) {
            string peek = reader.Peek(2);
            if (peek != null && peek == "_:") {
                reader.Position += 2;
                return new CompilerTokenData(this);
            }
            return null;
        }

        public void Write(BinaryWriter writer, BytecodeProvider provider, CompilerTokenData data) {
            writer.Write((byte)provider.TokenTypeProvider.GetTokenId(GdcTokenType.Wildcard));
            writer.Write((byte)provider.TokenTypeProvider.GetTokenId(GdcTokenType.Colon));
        }
    }

    public class NewlineCompilerToken : ICompilerToken {
        public virtual CompilerTokenData Parse(SourceCodeReader reader, BytecodeProvider provider) {
            string newlineCharacter = null;

            string peek = reader.Peek(1);
            if (peek != null && peek == "\n") {
                newlineCharacter = "\n";
            }
            if (newlineCharacter == null) {
                peek = reader.Peek(2);
                if (peek != null && peek == "\r\n") {
                    newlineCharacter = "\r\n";
                }
            }

            if (newlineCharacter == null) {
                return null;
            }

            reader.Position += newlineCharacter.Length;

            uint indentation = 0;
            while (true) {
                string indentChar = reader.Peek(1);
                if (indentChar == " ") {
                    throw new Exception("gdtool compiler requires tabs instead of spaces");
                } else if (indentChar == "\t") {
                    indentation++;
                    reader.Position += 1;
                } else {
                    break;
                }
            }

            return new CompilerTokenData(this) {
                Data = indentation
            };
        }

        public void Write(BinaryWriter writer, BytecodeProvider provider, CompilerTokenData data) {
            writer.Write(provider.TokenTypeProvider.GetTokenId(GdcTokenType.Newline) | (data.Data << 8) | 0x80);
        }
    }

    public class KeywordCompilerToken : BasicCompilerToken {
        public KeywordCompilerToken(GdcTokenType? type, string value) : base(type, value) { }

        public override CompilerTokenData Parse(SourceCodeReader reader, BytecodeProvider provider) {
            string peek = reader.Peek(Value.Length);
            if (peek != null && peek == Value) {
                reader.Position += Value.Length;
                string nextChar = reader.Peek(1);
                if (nextChar == null || (!char.IsLetterOrDigit(nextChar[0]) && nextChar[0] != '_')) {
                    return new CompilerTokenData(this);
                } else {
                    reader.Position -= Value.Length;
                }
            }
            return null;
        }
    }

    public class BuiltInFuncCompilerToken : ICompilerToken {
        public CompilerTokenData Parse(SourceCodeReader reader, BytecodeProvider provider) {
            for (uint i = 0; i < provider.BuiltInFunctions.Length; i++) {
                string func = provider.BuiltInFunctions[i];
                string peek = reader.Peek(func.Length);
                if (peek != null && peek == func) {
                    reader.Position += func.Length;
                    return new CompilerTokenData(this) {
                        Data = i
                    };
                }
            }
            return null;
        }

        public void Write(BinaryWriter writer, BytecodeProvider provider, CompilerTokenData data) {
            writer.Write(provider.TokenTypeProvider.GetTokenId(GdcTokenType.BuiltInFunc) | (data.Data << 8) | 0x80);
        }
    }

    public class BuiltInTypeCompilerToken : ICompilerToken {
        public CompilerTokenData Parse(SourceCodeReader reader, BytecodeProvider provider) {
            string[] typeNames = provider.TypeNameProvider.GetAllTypeNames();
            for (uint i = 0; i < typeNames.Length; i++) {
                string type = typeNames[i];
                string peek = reader.Peek(type.Length);
                if (peek != null && peek == type) {
                    reader.Position += type.Length;
                    return new CompilerTokenData(this) {
                        Data = i
                    };
                }
            }
            return null;
        }

        public void Write(BinaryWriter writer, BytecodeProvider provider, CompilerTokenData data) {
            writer.Write(provider.TokenTypeProvider.GetTokenId(GdcTokenType.BuiltInType) | (data.Data << 8) | 0x80);
        }
    }

    public class IdentifierCompilerToken : ICompilerToken {
        public CompilerTokenData Parse(SourceCodeReader reader, BytecodeProvider provider) {
            char first = reader.Read(1)[0];
            if (!char.IsLetter(first) && first != '_') {
                return null;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(first);
            while (true) {
                string ch = reader.Peek(1);
                if (ch == null || (!char.IsLetterOrDigit(ch[0]) && ch[0] != '_')) {
                    return new CompilerTokenData(this) {
                        Operand = new GdcIdentifier(builder.ToString())
                    };
                }

                builder.Append(ch);
                reader.Position++;
            }
        }

        public void Write(BinaryWriter writer, BytecodeProvider provider, CompilerTokenData data) {
            writer.Write(provider.TokenTypeProvider.GetTokenId(GdcTokenType.Identifier) | (data.Data << 8) | 0x80);
        }
    }

    public class ConstantCompilerToken : ICompilerToken {
        public CompilerTokenData Parse(SourceCodeReader reader, BytecodeProvider provider) {
            char first = reader.Peek(1)[0];
            if (first == '"') { // read a string
                                // TODO multiline strings
                reader.Position++;

                StringBuilder str = new StringBuilder();
                char lastChar = '\0';
                while (true) {
                    string next = reader.Read(1);
                    if (next == null) {
                        return null;
                    }
                    char ch = next[0];
                    if (ch == '"') {
                        if (lastChar == '\\') {
                            str.Append(ch);
                            lastChar = ch;
                            continue;
                        }

                        return new CompilerTokenData(this) {
                            Operand = new GdcString {
                                Value = str.ToString()
                            }
                        };
                    }

                    str.Append(ch);
                    lastChar = ch;
                }
            } else if (char.IsDigit(first)) { // read a numeric value
                StringBuilder numberBuffer = new StringBuilder();
                int numberBase = 10;
                if (first == '0') {
                    string prefix = reader.Peek(2);
                    if (prefix != null) {
                        if (prefix == "0b") {
                            numberBase = 2;
                        } else if (prefix == "0x") {
                            numberBase = 16;
                        }
                    }
                }

                while (true) {
                    string next = reader.Peek(1);
                    if (next == null || (!char.IsDigit(next[0]) && next[0] != '.' && next[0] != '_')) {
                        string num = numberBuffer.ToString();
                        if (num.Contains(".")) {
                            if (!double.TryParse(num, out double val)) {
                                return null;
                            }
                            float valFloat = (float)val;
                            if (valFloat != val) { // if there is loss of precision by casting to float
                                return new CompilerTokenData(this) {
                                    Operand = new GdcDouble {
                                        Value = val
                                    }
                                };
                            } else {
                                return new CompilerTokenData(this) {
                                    Operand = new GdcSingle {
                                        Value = valFloat
                                    }
                                };
                            }
                        } else {
                            long val;
                            try {
                                val = Convert.ToInt64(num, numberBase);
                            } catch (Exception) {
                                return null;
                            }
                            if (val > int.MaxValue) {
                                return new CompilerTokenData(this) {
                                    Operand = new GdcUInt64 {
                                        Value = (ulong)val
                                    }
                                };
                            } else {
                                return new CompilerTokenData(this) {
                                    Operand = new GdcUInt32 {
                                        Value = (uint)val
                                    }
                                };
                            }
                        }
                    }

                    if (next[0] == '_') {
                        continue;
                    }

                    numberBuffer.Append(next[0]);
                    reader.Position++;
                }
            } else if (reader.Peek(4) != null && reader.Peek(4) == "null") {
                reader.Position += 4;
                return new CompilerTokenData(this) {
                    Operand = new GdcNull()
                };
            }

            return null;
        }

        public void Write(BinaryWriter writer, BytecodeProvider provider, CompilerTokenData data) {
            writer.Write(provider.TokenTypeProvider.GetTokenId(GdcTokenType.Constant) | (data.Data << 8) | 0x80);
        }
    }
}
