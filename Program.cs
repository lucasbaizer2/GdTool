using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GdTool {
    public class Program {
        private static void Decode(string archivePath) {
            
        }

        private static void Build(string dirPath) {

        }

        static void Main(string[] args) {
            if (args.Length != 3) {
                Console.WriteLine("usage: gdtool.exe <decode|build> <path>");
                return;
            }

            if (args[1] == "decode") {
                Decode(args[2]);
            } else if (args[1] == "build") {
                Build(args[2]);
            } else {
                Console.WriteLine("usage: gdtool.exe <decode|build> <path>");
            }

            //byte[] bytes = File.ReadAllBytes(@"C:\Users\Lucas\Downloads\Godot RE Tools\tasteless-shores\game\player\player_controller.gdc");

            //BytecodeProvider provider = new BytecodeProvider {
            //    TypeNameProvider = TypeNameProviders.ProviderV3,
            //    BuiltInFunctions = null
            //};
            //GdcFile file = new GdcFile(bytes, provider);
            //// Console.WriteLine(file.Decompiled);

            //byte[] pckBytes = File.ReadAllBytes(@"C:\Users\Lucas\Downloads\Godot RE Tools\tasteless-shores.pck");
            //PckFile pck = new PckFile(pckBytes);
            //byte[] pckSerialized = pck.ToBytes();
            //PckFile rePck = new PckFile(pckSerialized);

            //Console.WriteLine("original length = " + pckBytes.Length);
            //Console.WriteLine("serialized length = " + pckSerialized.Length);
            //Console.WriteLine("pckFiles = " + pck.Entries.Count);
            //Console.WriteLine("rePckFiles = " + rePck.Entries.Count);
            //Console.ReadKey();
        }
    }
}
