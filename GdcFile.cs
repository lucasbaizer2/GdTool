using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GdTool {
    public class GdcFile {
        public uint Version { get; private set; }
        public string Decompiled { get; private set; }

        public GdcFile(byte[] arr, BytecodeProvider provider) {
            using (MemoryStream ms = new MemoryStream(arr)) {
                using (BinaryReader buf = new BinaryReader(ms)) {
                    string magicHeader = Encoding.ASCII.GetString(buf.ReadBytes(4));
                    if (magicHeader != "GDSC") {
                        throw new Exception("Invalid GDC file: missing magic header");
                    }

                    Version = buf.ReadUInt32();
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

                    object[] constants = new object[constantCount];
                    for (int i = 0; i < constantCount; i++) {
                        constants[i] = DecodeConstant(buf);
                    }

                    Dictionary<uint, uint> tokenLineMap = new Dictionary<uint, uint>((int)lineCount);
                    for (int i = 0; i < lineCount; i++) {
                        tokenLineMap.Add(buf.ReadUInt32(), buf.ReadUInt32());
                    }

                    uint[] tokenTypes = new uint[tokenCount];
                    for (int i = 0; i < tokenCount; i++) {
                        byte cur = arr[buf.BaseStream.Position];

                        uint tokenType;
                        if ((cur & 0x80) != 0) {
                            tokenType = buf.ReadUInt32() ^ 0x80;
                        } else {
                            tokenType = cur;
                            buf.BaseStream.Position += 1;
                        }

                        tokenTypes[i] = tokenType;
                    }

                    DecompileBuffer decompile = new DecompileBuffer();

                    Token[] tokens = new Token[tokenCount];
                    TokenType previous = TokenType.Newline;
                    for (int i = 0; i < tokenCount; i++) {
                        tokens[i] = new Token {
                            Type = provider.OpcodeProvider.GetTokenType(tokenTypes[i]),
                            Data = tokenTypes[i] >> 8
                        };
                        ReadToken(tokens[i], identifiers, constants);

                        tokens[i].Decompile(decompile, previous, provider);
                        previous = tokens[i].Type;
                    }

                    Decompiled = decompile.Content;
                }
            }
        }

        private void ReadToken(Token token, string[] identifiers, object[] constants) {
            switch (token.Type) {
                case TokenType.Identifier:
                    token.Operand = identifiers[token.Data];
                    return;
                case TokenType.Constant:
                    token.Operand = constants[token.Data];
                    return;
                default:
                    return;
            }
        }

        private string DecodeString(BinaryReader buf) {
            uint len = buf.ReadUInt32();
            uint padding = 0;
            if (len % 4 != 0) {
                padding = 4 - len % 4;
            }

            byte[] strBytes = buf.ReadBytes((int)len);
            string str = Encoding.UTF8.GetString(strBytes);

            buf.ReadBytes((int)padding);

            return str;
        }

        private object DecodeConstant(BinaryReader buf) {
            uint type = buf.ReadUInt32();
            uint typeWithoutFlags = type & 0xFF;

            switch (typeWithoutFlags) {
                case 0: // NIL
                    return new Nil();
                case 1: // BOOL
                    return buf.ReadUInt32() != 0;
                case 2: // INT
                    if ((type & (1 << 16)) != 0) {
                        return buf.ReadUInt64();
                    } else {
                        return buf.ReadUInt32();
                    }
                case 3: // REAL
                    if ((type & (1 << 16)) != 0) {
                        return buf.ReadDouble();
                    } else {
                        return buf.ReadSingle();
                    }
                case 4: // STRING
                    return new GdcString(DecodeString(buf));
                case 5: // VECTOR2
                    return new Vector2 {
                        X = buf.ReadSingle(),
                        Y = buf.ReadSingle()
                    };
                case 6: // RECT2
                    return new Rect2 {
                        Position = new Vector2 {
                            X = buf.ReadSingle(),
                            Y = buf.ReadSingle()
                        },
                        Size = new Vector2 {
                            X = buf.ReadSingle(),
                            Y = buf.ReadSingle()
                        },
                    };
                case 7: // VECTOR3
                    return new Vector3 {
                        X = buf.ReadSingle(),
                        Y = buf.ReadSingle(),
                        Z = buf.ReadSingle()
                    };
                case 8: // TRANSFORM2D
                    return new Transform2d {
                        Origin = new Vector2 {
                            X = buf.ReadSingle(),
                            Y = buf.ReadSingle()
                        },
                        X = new Vector2 {
                            X = buf.ReadSingle(),
                            Y = buf.ReadSingle()
                        },
                        Y = new Vector2 {
                            X = buf.ReadSingle(),
                            Y = buf.ReadSingle()
                        },
                    };
                case 9: // PLANE
                    return new Plane {
                        Normal = new Vector3 {
                            X = buf.ReadSingle(),
                            Y = buf.ReadSingle(),
                            Z = buf.ReadSingle()
                        },
                        D = buf.ReadSingle()
                    };
                case 10: // QUAT
                    return new Quat {
                        X = buf.ReadSingle(),
                        Y = buf.ReadSingle(),
                        Z = buf.ReadSingle(),
                        W = buf.ReadSingle()
                    };
                case 11: // AABB
                    return new Aabb {
                        Position = new Vector3 {
                            X = buf.ReadSingle(),
                            Y = buf.ReadSingle(),
                            Z = buf.ReadSingle()
                        },
                        Size = new Vector3 {
                            X = buf.ReadSingle(),
                            Y = buf.ReadSingle(),
                            Z = buf.ReadSingle()
                        }
                    };
                case 12: // BASIS
                    return new Basis {
                        X = new Vector3 {
                            X = buf.ReadSingle(),
                            Y = buf.ReadSingle(),
                            Z = buf.ReadSingle()
                        },
                        Y = new Vector3 {
                            X = buf.ReadSingle(),
                            Y = buf.ReadSingle(),
                            Z = buf.ReadSingle()
                        },
                        Z = new Vector3 {
                            X = buf.ReadSingle(),
                            Y = buf.ReadSingle(),
                            Z = buf.ReadSingle()
                        }
                    };
                case 13: // TRANSFORM
                    return new Transform {
                        Basis = new Basis {
                            X = new Vector3 {
                                X = buf.ReadSingle(),
                                Y = buf.ReadSingle(),
                                Z = buf.ReadSingle()
                            },
                            Y = new Vector3 {
                                X = buf.ReadSingle(),
                                Y = buf.ReadSingle(),
                                Z = buf.ReadSingle()
                            },
                            Z = new Vector3 {
                                X = buf.ReadSingle(),
                                Y = buf.ReadSingle(),
                                Z = buf.ReadSingle()
                            }
                        },
                        Origin = new Vector3 {
                            X = buf.ReadSingle(),
                            Y = buf.ReadSingle(),
                            Z = buf.ReadSingle()
                        }
                    };
                case 14: // COLOR
                    return new Color {
                        R = buf.ReadSingle(),
                        G = buf.ReadSingle(),
                        B = buf.ReadSingle(),
                        A = buf.ReadSingle()
                    };
                case 15: // NODE_PATH
                    uint strlen = buf.ReadUInt32();
                    if ((strlen & (strlen << 31)) != 0) {
                        uint nameCount = strlen &= 0x7FFFFFFF;
                        uint subnameCount = buf.ReadUInt32();
                        uint flags = buf.ReadUInt32();

                        if ((flags & 2) != 0) {
                            subnameCount++;
                        }

                        string[] names = new string[nameCount];
                        string[] subnames = new string[subnameCount];
                        for (int i = 0; i < nameCount; i++) {
                            names[i] = DecodeString(buf);
                        }
                        for (int i = 0; i < subnameCount; i++) {
                            subnames[i] = DecodeString(buf);
                        }

                        return new NodePath {
                            Names = names,
                            Subnames = subnames,
                            Flags = flags & 1
                        };
                    } else {
                        throw new InvalidOperationException("invalid data");
                        // buf.Skip(-4);
                        // return DecodeString(buf);
                    }
                case 16: // RID
                    throw new NotImplementedException("RID");
                default:
                    throw new NotImplementedException(type.ToString());
            }
        }
    }
}
