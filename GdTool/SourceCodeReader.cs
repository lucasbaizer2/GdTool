using System.Linq;

namespace GdTool {
    public class SourceCodeReader {
        public string Source;
        public int Position;
        public bool HasRemaining {
            get {
                return Position < Source.Length;
            }
        }
        public int CurrentRow {
            get {
                return Source.Substring(0, Position).Count(ch => ch == '\n') + 1;
            }
        }

        public SourceCodeReader(string source) {
            Source = source;
        }

        public string Peek(int characters) {
            if (Position + characters > Source.Length) {
                return null;
            }

            return Source.Substring(Position, characters);
        }

        public string Read(int characters) {
            string str = Peek(characters);
            if (str != null) {
                Position += characters;
            }
            return str;
        }
    }
}
