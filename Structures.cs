using System;
using System.IO;
using System.Text;

namespace GdTool {
    public interface IGdStructure {
        void Serialize(BinaryWriter buf, BytecodeProvider provider);

        IGdStructure Deserialize(BinaryReader reader);
    }

    public class GdcIdentifier : IGdStructure {
        public string Identifier;

        public GdcIdentifier(string identifier) {
            Identifier = identifier;
        }

        public IGdStructure Deserialize(BinaryReader reader) {
            throw new InvalidOperationException();
        }

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            throw new InvalidOperationException();
        }

        public override string ToString() {
            return Identifier;
        }

        public override bool Equals(object obj) {
            if (obj is GdcIdentifier o) {
                return o.Identifier == Identifier;
            }
            return false;
        }

        public override int GetHashCode() {
            return Identifier.GetHashCode();
        }
    }

    public class GdcNull : IGdStructure {
        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("Nil"));
            // buf.Write(0U);
        }

        public IGdStructure Deserialize(BinaryReader reader) {
            // reader.BaseStream.Position += 4;
            return this;
        }

        public override string ToString() {
            return "null";
        }

        public override bool Equals(object obj) {
            return obj is GdcNull;
        }

        public override int GetHashCode() {
            return 0;
        }
    }

    public class GdcBool : IGdStructure {
        public bool Value;

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("bool"));
            buf.Write(Value ? 1U : 0U);
        }

        public IGdStructure Deserialize(BinaryReader reader) {
            Value = reader.ReadUInt32() != 0;
            return this;
        }

        public override string ToString() {
            return Value.ToString().ToLower();
        }

        public override bool Equals(object obj) {
            if (obj is GdcBool o) {
                return o.Value == Value;
            }
            return false;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    public class GdcUInt32 : IGdStructure {
        public uint Value;

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("int"));
            buf.Write(Value);
        }

        public IGdStructure Deserialize(BinaryReader reader) {
            Value = reader.ReadUInt32();
            return this;
        }

        public override string ToString() {
            return Value.ToString();
        }

        public override bool Equals(object obj) {
            if (obj is GdcUInt32 o) {
                return o.Value == Value;
            }
            return false;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    public class GdcUInt64 : IGdStructure {
        public ulong Value;

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("int") | (1 << 16));
            buf.Write(Value);
        }

        public IGdStructure Deserialize(BinaryReader reader) {
            Value = reader.ReadUInt64();
            return this;
        }

        public override string ToString() {
            return Value.ToString();
        }

        public override bool Equals(object obj) {
            if (obj is GdcUInt64 o) {
                return o.Value == Value;
            }
            return false;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    public class GdcSingle : IGdStructure {
        public float Value;

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("float"));
            buf.Write(Value);
        }

        public IGdStructure Deserialize(BinaryReader reader) {
            Value = reader.ReadSingle();
            return this;
        }

        public override string ToString() {
            string str = Value.ToString();
            if (!str.Contains(".")) {
                return str + ".0";
            }
            return str;
        }

        public override bool Equals(object obj) {
            if (obj is GdcSingle o) {
                return o.Value == Value;
            }
            return false;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    public class GdcDouble : IGdStructure {
        public double Value;

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("float") | (1 << 16));
            buf.Write(Value);
        }

        public IGdStructure Deserialize(BinaryReader reader) {
            Value = reader.ReadDouble();
            return this;
        }

        public override string ToString() {
            string str = Value.ToString();
            if (!str.Contains(".")) {
                return str + ".0";
            }
            return str;
        }

        public override bool Equals(object obj) {
            if (obj is GdcDouble o) {
                return o.Value == Value;
            }
            return false;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    public class GdcString : IGdStructure {
        public string Value;

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("String"));
            byte[] strBytes = Encoding.UTF8.GetBytes(Value);
            buf.Write((uint)strBytes.Length);
            buf.Write(strBytes);
            if (strBytes.Length % 4 != 0) {
                buf.Write(new byte[4 - (strBytes.Length % 4)]);
            }
        }

        public IGdStructure Deserialize(BinaryReader buf) {
            uint len = buf.ReadUInt32();
            uint padding = 0;
            if (len % 4 != 0) {
                padding = 4 - (len % 4);
            }

            byte[] strBytes = buf.ReadBytes((int)len);
            Value = Encoding.UTF8.GetString(strBytes);

            buf.ReadBytes((int)padding);

            return this;
        }

        public override string ToString() {
            return "\"" +
                Value
                .Replace("\\", "\\\\")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\f", "\\f")
                .Replace("\v", "\\v")
                .Replace("\a", "\\a")
                .Replace("\b", "\\b")
                .Replace("\"", "\\\"")
                + "\"";
        }

        public override bool Equals(object obj) {
            if (obj is GdcString o) {
                return o.Value == Value;
            }
            return false;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    public class Vector2 : IGdStructure {
        public float X;
        public float Y;

        public IGdStructure Deserialize(BinaryReader reader) {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            return this;
        }

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("Vector2"));
            SerializeRaw(buf);
        }

        internal void SerializeRaw(BinaryWriter buf) {
            buf.Write(X);
            buf.Write(Y);
        }

        public override string ToString() {
            return "Vector2(" + X + ", " + Y + ")";
        }

        public override bool Equals(object obj) {
            if (obj is Vector2 o) {
                return o.X == X && o.Y == Y;
            }
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine(X, Y);
        }
    }

    public class Rect2 : IGdStructure {
        public Vector2 Position;
        public Vector2 Size;

        public IGdStructure Deserialize(BinaryReader reader) {
            Position = (Vector2)new Vector2().Deserialize(reader);
            Size = (Vector2)new Vector2().Deserialize(reader);
            return this;
        }

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("Rect2"));
            Position.SerializeRaw(buf);
            Size.SerializeRaw(buf);
        }

        public override string ToString() {
            return "Rect2(" + Position.X + ", " + Position.Y + ", " + Size.X + ", " + Size.Y + ")";
        }

        public override bool Equals(object obj) {
            if (obj is Rect2 o) {
                return o.Position.Equals(Position) && o.Size.Equals(Size);
            }
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine(Position, Size);
        }
    }

    public class Vector3 : IGdStructure {
        public float X;
        public float Y;
        public float Z;

        public IGdStructure Deserialize(BinaryReader reader) {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
            return this;
        }

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("Vector3"));
            SerializeRaw(buf);
        }

        internal void SerializeRaw(BinaryWriter buf) {
            buf.Write(X);
            buf.Write(Y);
            buf.Write(Z);
        }

        public override string ToString() {
            return "Vector3(" + X + ", " + Y + ", " + Z + ")";
        }

        public override bool Equals(object obj) {
            if (obj is Vector3 o) {
                return o.X == X && o.Y == Y && o.Z == Z;
            }
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine(X, Y, Z);
        }
    }

    public class Transform2d : IGdStructure {
        public Vector2 Origin;
        public Vector2 X;
        public Vector2 Y;

        public IGdStructure Deserialize(BinaryReader reader) {
            Origin = (Vector2)new Vector2().Deserialize(reader);
            X = (Vector2)new Vector2().Deserialize(reader);
            Y = (Vector2)new Vector2().Deserialize(reader);
            return this;
        }

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("Transform2D"));
            Origin.SerializeRaw(buf);
            X.SerializeRaw(buf);
            Y.SerializeRaw(buf);
        }

        public override string ToString() {
            return "Transform2D(" + X + ", " + Y + ", " + Origin + ")";
        }

        public override bool Equals(object obj) {
            if (obj is Transform2d o) {
                return o.Origin.Equals(Origin) && o.X.Equals(X) && o.Y.Equals(Y);
            }
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine(Origin, X, Y);
        }
    }

    public class Plane : IGdStructure {
        public Vector3 Normal;
        public float D;

        public IGdStructure Deserialize(BinaryReader reader) {
            Normal = (Vector3)new Vector3().Deserialize(reader);
            D = reader.ReadSingle();
            return this;
        }

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("Plane"));
            Normal.SerializeRaw(buf);
            buf.Write(D);
        }

        public override string ToString() {
            return "Plane(" + Normal + ", " + D + ")";
        }

        public override bool Equals(object obj) {
            if (obj is Plane o) {
                return o.Normal.Equals(Normal) && o.D == D;
            }
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine(Normal, D);
        }
    }

    public class Quat : IGdStructure {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public IGdStructure Deserialize(BinaryReader reader) {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
            W = reader.ReadSingle();
            return this;
        }

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("Quat"));
            buf.Write(X);
            buf.Write(Y);
            buf.Write(Z);
            buf.Write(W);
        }

        public override string ToString() {
            return "Quat(" + X + ", " + Y + ", " + Z + ", " + W + ")";
        }

        public override bool Equals(object obj) {
            if (obj is Quat o) {
                return o.X == X && o.Y == Y && o.Z == Z && o.W == W;
            }
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine(X, Y, Z, W);
        }
    }

    public class Aabb : IGdStructure {
        public Vector3 Position;
        public Vector3 Size;

        public IGdStructure Deserialize(BinaryReader reader) {
            Position = (Vector3)new Vector3().Deserialize(reader);
            Size = (Vector3)new Vector3().Deserialize(reader);
            return this;
        }

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("AABB"));
            Position.SerializeRaw(buf);
            Size.SerializeRaw(buf);
        }

        public override string ToString() {
            return "AABB(" + Position + ", " + Size + ")";
        }

        public override bool Equals(object obj) {
            if (obj is Aabb o) {
                return o.Position.Equals(Position) && o.Size.Equals(Size);
            }
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine(Position, Size);
        }
    }

    public class Basis : IGdStructure {
        public Vector3 X;
        public Vector3 Y;
        public Vector3 Z;

        public IGdStructure Deserialize(BinaryReader reader) {
            X = (Vector3)new Vector3().Deserialize(reader);
            Y = (Vector3)new Vector3().Deserialize(reader);
            Z = (Vector3)new Vector3().Deserialize(reader);
            return this;
        }

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("Basis"));
            SerializeRaw(buf);
        }

        internal void SerializeRaw(BinaryWriter buf) {
            X.SerializeRaw(buf);
            Y.SerializeRaw(buf);
            Z.SerializeRaw(buf);
        }

        public override string ToString() {
            return "Basis(" + X + ", " + Y + ", " + Z + ")";
        }

        public override bool Equals(object obj) {
            if (obj is Basis o) {
                return o.X.Equals(X) && o.Y.Equals(Y) && o.Z.Equals(Z);
            }
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine(X, Y, Z);
        }
    }

    public class Transform : IGdStructure {
        public Basis Basis;
        public Vector3 Origin;

        public IGdStructure Deserialize(BinaryReader reader) {
            Basis = (Basis)new Basis().Deserialize(reader);
            Origin = (Vector3)new Vector3().Deserialize(reader);
            return this;
        }

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("Transform"));
            Basis.SerializeRaw(buf);
            Origin.SerializeRaw(buf);
        }

        public override string ToString() {
            return "Transform(" + Basis + ", " + Origin + ")";
        }

        public override bool Equals(object obj) {
            if (obj is Transform o) {
                return o.Basis.Equals(Basis) && o.Origin.Equals(Origin);
            }
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine(Basis, Origin);
        }
    }

    public class Color : IGdStructure {
        public float R;
        public float G;
        public float B;
        public float A;

        public IGdStructure Deserialize(BinaryReader reader) {
            R = reader.ReadSingle();
            G = reader.ReadSingle();
            B = reader.ReadSingle();
            A = reader.ReadSingle();
            return this;
        }

        public void Serialize(BinaryWriter buf, BytecodeProvider provider) {
            buf.Write(provider.TypeNameProvider.GetTypeId("Color"));
            buf.Write(R);
            buf.Write(G);
            buf.Write(B);
            buf.Write(A);
        }

        public override string ToString() {
            return "Color(" + R + ", " + G + ", " + B + ", " + A + ")";
        }

        public override bool Equals(object obj) {
            if (obj is Color o) {
                return o.R == R && o.G == G && o.B == B && o.A == A;
            }
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine(R, G, B, A);
        }
    }
}
