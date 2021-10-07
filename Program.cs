using CommandLine;
using System;
using System.IO;

namespace GdTool {
    public class Program {
        [Verb("decode", HelpText = "Decodes and extracts a PCK file.")]
        public class DecodeOptions {
            [Option('i', "in", Required = true, HelpText = "The PCK file to extract.")]
            public string InputPath { get; set; }

            [Option('d', "decompile", Required = false, HelpText = "Decompiles GDC files when in decode mode.")]
            public bool Decompile { get; set; }

            [Option('o', "out", Required = false, HelpText = "Output directory to extract files to.")]
            public string OutputDirectory { get; set; }
        }

        [Verb("build", HelpText = "Packs a directory into a PCK file.")]
        public class BuildOptions {
            [Option('i', "in", Required = true, HelpText = "The directory to pack.")]
            public string InputPath { get; set; }

            [Option('o', "out", Required = false, HelpText = "Output file to place the PCK.")]
            public string OutputFile { get; set; }
        }

        static void Main(string[] args) {
            byte[] compiled = GdScriptCompiler.Compile(File.ReadAllText(@"C:\Users\Lucas\Downloads\Godot RE Tools\tasteless-shores\game\player\player_controller.gd"), new BytecodeProvider(0x5565f55));
            string decompiled = GcScriptDecompiler.Decompile(compiled, new BytecodeProvider(0x5565f55));
            Console.WriteLine(decompiled);

            Parser.Default.ParseArguments<DecodeOptions, BuildOptions>(args).WithParsed<DecodeOptions>(Decode).WithParsed<BuildOptions>(Build);
        }

        private static void Decode(DecodeOptions options) {
            if (!File.Exists(options.InputPath)) {
                Console.WriteLine("Invalid PCK file (does not exist): " + options.InputPath);
                return;
            }

            string fileName = Path.GetFileName(options.InputPath);
            if (fileName.Contains(".")) {
                fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
            }
            string outputDirectory;
            if (options.OutputDirectory != null) {
                outputDirectory = options.OutputDirectory;
            } else {
                outputDirectory = Path.Combine(Path.GetDirectoryName(options.InputPath), fileName);
                if (Directory.Exists(outputDirectory)) {
                    Console.Write("Output directory \"" + outputDirectory + "\" already exists. Do you want to overwrite? (y/n): ");
                    if (Console.ReadLine().ToLower() != "y") {
                        return;
                    }
                }
            }

            if (!Directory.Exists(outputDirectory)) {
                Directory.CreateDirectory(outputDirectory);
            }

            byte[] pckBytes = File.ReadAllBytes(options.InputPath);
            PckFile pck;
            try {
                Console.Write("Reading PCK file... ");
                pck = new PckFile(pckBytes);
            } catch (Exception e) {
                Console.WriteLine("invalid (could not parse).");
                Console.WriteLine(e);
                return;
            }

            Console.WriteLine("success.");

            BytecodeProvider provider = new BytecodeProvider(0x5565f55);
            for (int i = 0; i < pck.Entries.Count; i++) {
                PckFileEntry entry = pck.Entries[i];
                string path = entry.Path.Substring(6); // remove res://
                string full = Path.Combine(outputDirectory, path);
                string parent = Path.GetDirectoryName(full);
                if (!Directory.Exists(parent)) {
                    Directory.CreateDirectory(parent);
                }

                if (options.Decompile && path.EndsWith(".gdc")) {
                    string decompiled = GcScriptDecompiler.Decompile(entry.Data, provider);
                    full = full.Substring(0, full.Length - 1); // convert exception from .gdc to .gd
                    File.WriteAllText(full, decompiled);
                } else {
                    File.WriteAllBytes(full, entry.Data);
                }
                File.WriteAllBytes(full, entry.Data);

                int percentage = (int)Math.Floor((i + 1) / (double)pck.Entries.Count * 100.0);
                Console.Write("\rUnpacking: " + (i + 1) + "/" + pck.Entries.Count + " (" + percentage + "%)");
            }
            Console.WriteLine();
        }

        private static void Build(BuildOptions options) {

        }
    }
}
