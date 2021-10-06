using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace GdTool {
    public class PckFileEntry {
        public string Path;
        public byte[] Data;
    }

    public class PckFile {
        public uint PackFormatVersion;
        public uint VersionMajor;
        public uint VersionMinor;
        public uint VersionPatch;
        public List<PckFileEntry> Entries;

        public PckFile(byte[] arr) {
            using (MemoryStream ms = new MemoryStream(arr)) {
                using (BinaryReader buf = new BinaryReader(ms)) {
                    string magicHeader = Encoding.ASCII.GetString(buf.ReadBytes(4));
                    if (magicHeader != "GDPC") {
                        throw new Exception("Invalid PCK file: missing magic header");
                    }

                    PackFormatVersion = buf.ReadUInt32();
                    VersionMajor = buf.ReadUInt32();
                    VersionMinor = buf.ReadUInt32();
                    VersionPatch = buf.ReadUInt32();

                    buf.BaseStream.Position += 4 * 16;

                    uint filesCount = buf.ReadUInt32();

                    Entries = new List<PckFileEntry>((int)filesCount);
                    for (int i = 0; i < filesCount; i++) {
                        string path = Encoding.UTF8.GetString(buf.ReadBytes((int)buf.ReadUInt32())).Replace("\0", "");
                        ulong fileOffset = buf.ReadUInt64();
                        ulong fileLength = buf.ReadUInt64();
                        byte[] md5 = buf.ReadBytes(16);

                        long pos = buf.BaseStream.Position;
                        buf.BaseStream.Position = (long)fileOffset;

                        byte[] fileData = buf.ReadBytes((int)fileLength);
                        Entries.Add(new PckFileEntry {
                            Path = path,
                            Data = fileData
                        });

                        buf.BaseStream.Position = pos;
                    }
                }
            }
        }

        public byte[] ToBytes() {
            int totalSize =
                4 + // magic header
                4 * 4 + // version info
                4 * 16 + // padding
                4 + // files count
                Entries.Select(entry =>
                    4 + // path length prefix
                    Encoding.UTF8.GetBytes(entry.Path).Length + // size of path
                    8 * 2 + // offset and size
                    16 + // md5 hash
                    entry.Data.Length // file bytes
                ).Sum();
            byte[] arr = new byte[totalSize];
            using (MemoryStream ms = new MemoryStream(arr)) {
                using (BinaryWriter buf = new BinaryWriter(ms)) {
                    MD5 md5 = MD5.Create();

                    buf.Write(Encoding.ASCII.GetBytes("GDPC"));
                    buf.Write(PackFormatVersion);
                    buf.Write(VersionMajor);
                    buf.Write(VersionMinor);
                    buf.Write(VersionPatch);
                    buf.Write(new byte[4 * 16]);
                    buf.Write((uint)Entries.Count);

                    long[] fileOffsets = new long[Entries.Count];

                    for (int i = 0; i < Entries.Count; i++) {
                        PckFileEntry entry = Entries[i];
                        byte[] pathBytes = Encoding.UTF8.GetBytes(entry.Path);
                        buf.Write((uint)pathBytes.Length);
                        buf.Write(pathBytes);
                        fileOffsets[i] = buf.BaseStream.Position;
                        buf.Write(0UL);
                        buf.Write((ulong)entry.Data.Length);
                        buf.Write(md5.ComputeHash(entry.Data));
                    }

                    for (int i = 0; i < Entries.Count; i++) {
                        long curPos = buf.BaseStream.Position;
                        buf.BaseStream.Position = fileOffsets[i];
                        buf.Write((ulong)curPos);
                        buf.BaseStream.Position = curPos;

                        PckFileEntry entry = Entries[i];
                        buf.Write(entry.Data);
                    }
                }
            }

            return arr;
        }
    }
}
