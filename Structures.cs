using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GdTool {
    public class Nil {
        public override string ToString() {
            return "Nil";
        }
    }

    public class GdcString {
        public string Value;

        public GdcString(string value) {
            Value = value;
        }

        public override string ToString() {
            return "\"" + Value.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") + "\"";
        }
    }

    public class Vector2 {
        public float X;
        public float Y;

        public override string ToString() {
            return "Vector2(" + X + ", " + Y + ")";
        }
    }

    public class Rect2 {
        public Vector2 Position;
        public Vector2 Size;

        public override string ToString() {
            return "Rect2(" + Position + ", " + Size + ")";
        }
    }

    public class Vector3 {
        public float X;
        public float Y;
        public float Z;

        public override string ToString() {
            return "Vector3(" + X + ", " + Y + ", " + Z + ")";
        }
    }

    public class Transform2d {
        public Vector2 Origin;
        public Vector2 X;
        public Vector2 Y;

        public override string ToString() {
            return "Transform2D(" + X + ", " + Y + ", " + Origin + ")";
        }
    }

    public class Plane {
        public Vector3 Normal;
        public float D;

        public override string ToString() {
            return "Plane(" + Normal + ", " + D + ")";
        }
    }

    public class Quat {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public override string ToString() {
            return "Quat(" + X + ", " + Y + ", " + Z + ", " + W + ")";
        }
    }

    public class Aabb {
        public Vector3 Position;
        public Vector3 Size;

        public override string ToString() {
            return "AABB(" + Position + ", " + Size + ")";
        }
    }

    public class Basis {
        public Vector3 X;
        public Vector3 Y;
        public Vector3 Z;

        public override string ToString() {
            return "Basis(" + X + ", " + Y + ", " + Z + ")";
        }
    }

    public class Transform {
        public Basis Basis;
        public Vector3 Origin;

        public override string ToString() {
            return "AABB(" + Basis + ", " + Origin + ")";
        }
    }

    public class Color {
        public float R;
        public float G;
        public float B;
        public float A;

        public override string ToString() {
            return "Color(" + R + ", " + G + ", " + B + ", " + A + ")";
        }
    }

    public class NodePath {
        public string[] Names;
        public string[] Subnames;
        public uint Flags;
    }
}
