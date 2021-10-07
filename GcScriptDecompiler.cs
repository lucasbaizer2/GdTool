using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GdTool {
    public class GcScriptDecompiler {
        public static string Decompile(byte[] arr, BytecodeProvider provider) {
            using (MemoryStream ms = new MemoryStream(arr)) {
                using (BinaryReader buf = new BinaryReader(ms)) {
                    string magicHeader = Encoding.ASCII.GetString(buf.ReadBytes(4));
                    if (magicHeader != "GDSC") {
                        throw new Exception("Invalid GDC file: missing magic header");
                    }

                    uint version = buf.ReadUInt32();
                    if (version != provider.BytecodeVersion) {
                        throw new Exception("Invalid GDC file: bytecode version does not match supplied provider");
                    }
                    uint identifierCount = buf.ReadUInt32();
                    uint constantCount = buf.ReadUInt32();
                    uint lineCount = buf.ReadUInt32();
                    uint tokenCount = buf.ReadUInt32();

                    string[] identifiers = new string[identifierCount];
                    for (int i = 0; i < identifierCount; i++) {
                        uint len = buf.ReadUInt32();
                        byte[] strBytes = new byte[len];
                        for (int j = 0; j < len; j++) {
                            strBytes[j] = (byte)(buf.ReadByte() ^ 0xB6);
                        }
                        string ident = Encoding.UTF8.GetString(strBytes).Replace("\0", "");
                        identifiers[i] = ident;
                    }

                    IGdStructure[] constants = new IGdStructure[constantCount];
                    for (int i = 0; i < constantCount; i++) {
                        constants[i] = DecodeConstant(buf, provider);
                    }

                    Dictionary<uint, uint> tokenLineMap = new Dictionary<uint, uint>((int)lineCount);
                    for (int i = 0; i < lineCount; i++) {
                        tokenLineMap.Add(buf.ReadUInt32(), buf.ReadUInt32());
                    }

                    DecompileBuffer decompile = new DecompileBuffer();
                    GdcTokenType previous = GdcTokenType.Newline;
                    for (int i = 0; i < tokenCount; i++) {
                        byte cur = arr[buf.BaseStream.Position];

                        uint tokenType;
                        if ((cur & 0x80) != 0) {
                            tokenType = buf.ReadUInt32() ^ 0x80;
                        } else {
                            tokenType = cur;
                            buf.BaseStream.Position += 1;
                        }

                        GdcToken token = new GdcToken {
                            Type = provider.TokenTypeProvider.GetTokenType(tokenType & 0xFF),
                            Data = tokenType >> 8
                        };
                        ReadToken(token, identifiers, constants);

                        token.Decompile(decompile, previous, provider);
                        previous = token.Type;
                    }

                    return decompile.Content;
                }
            }
        }

        private static void ReadToken(GdcToken token, string[] identifiers, IGdStructure[] constants) {
            switch (token.Type) {
                case GdcTokenType.Identifier:
                    token.Operand = new GdcIdentifier(identifiers[token.Data]);
                    return;
                case GdcTokenType.Constant:
                    token.Operand = constants[token.Data];
                    return;
                default:
                    return;
            }
        }

        private static IGdStructure DecodeConstant(BinaryReader buf, BytecodeProvider provider) {
            uint type = buf.ReadUInt32();
            uint typeWithoutFlags = type & 0xFF;
            string typeName = provider.TypeNameProvider.GetTypeName(typeWithoutFlags);

            switch (typeName) {
                case "Nil":
                    return new Nil().Deserialize(buf);
                case "bool":
                    return new GdcBool().Deserialize(buf);
                case "int":
                    if ((type & (1 << 16)) != 0) {
                        return new GdcUInt64().Deserialize(buf);
                    } else {
                        return new GdcUInt32().Deserialize(buf);
                    }
                case "float":
                    if ((type & (1 << 16)) != 0) {
                        return new GdcDouble().Deserialize(buf);
                    } else {
                        return new GdcSingle().Deserialize(buf);
                    }
                case "String":
                    return new GdcString().Deserialize(buf);
                case "Vector2":
                    return new Vector2().Deserialize(buf);
                case "Rect2":
                    return new Rect2().Deserialize(buf);
                case "Vector3":
                    return new Vector3().Deserialize(buf);
                case "Transform2D":
                    return new Transform2d().Deserialize(buf);
                case "Plane":
                    return new Plane().Deserialize(buf);
                case "Quat":
                    return new Quat().Deserialize(buf);
                case "AABB":
                    return new Aabb().Deserialize(buf);
                case "Basis":
                    return new Basis().Deserialize(buf);
                case "Transform":
                    return new Transform().Deserialize(buf);
                case "Color":
                    return new Color().Deserialize(buf);
                case "NodePath":
                    throw new NotImplementedException("NodePath");
                case "RID":
                    throw new NotImplementedException("RID");
                default:
                    throw new NotImplementedException(type.ToString());
            }
        }
    }
}
