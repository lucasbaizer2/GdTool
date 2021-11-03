using System;
using System.Text;

namespace GdTool {
    public class VersionDetector {
        private static bool IsHexadecimal(string str) {
            for (int i = 0; i < str.Length; i++) {
                if (str[i] >= '0' && str[i] <= '9') {
                    continue;
                }
                if (str[i] >= 'a' && str[i] <= 'f') {
                    continue;
                }
                return false;
            }
            return true;
        }

        public static BytecodeProvider Detect(byte[] binary) {
            int lastZeroByte = 0;
            for (int i = 0; i < binary.Length; i++) {
                if (binary[i] == 0) {
                    if (i - lastZeroByte == 41) { // search for a 40 character null terminated string
                        string hash = Encoding.ASCII.GetString(binary, lastZeroByte + 1, 40);
                        if (IsHexadecimal(hash)) {
                            string previousVersion = BytecodeProvider.FindPreviousMajorVersionHash(hash);
                            if (previousVersion != null) {
                                return BytecodeProvider.GetByCommitHash(previousVersion);
                            }
                        }
                    }
                    lastZeroByte = i;
                }
            }

            return null;
        }
    }
}
