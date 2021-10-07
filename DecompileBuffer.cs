using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GdTool {
    public class DecompileBuffer {
        private StringBuilder Text = new StringBuilder();
        public int Indentation { get; set; }

        public string Content {
            get {
                return Text.ToString();
            }
        }

        public DecompileBuffer Append(string val) {
            Text.Append(val);
            return this;
        }

        public DecompileBuffer AppendOp(string val) {
            if (Text.Length > 0 && Text[Text.Length - 1] != ' ') {
                Text.Append(' ');
            }
            Text.Append(val);
            Text.Append(' ');
            return this;
        }

        public void AppendNewLine() {
            Text.Append(Environment.NewLine);
            for (int i = 0; i < Indentation; i++) {
                Text.Append('\t');
            }
        }
    }
}
