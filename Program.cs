using CommandLine;
using System;
using System.IO;
using System.Text;

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
            string decompiled = GdScriptDecompiler.Decompile(compiled, new BytecodeProvider(0x5565f55));
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
                try {
                    string full = Path.Combine(outputDirectory, path);
                    string parent = Path.GetDirectoryName(full);
                    if (!Directory.Exists(parent)) {
                        Directory.CreateDirectory(parent);
                    }

                    if (options.Decompile && path.EndsWith(".gdc")) {
                        string decompiled = GdScriptDecompiler.Decompile(entry.Data, provider);
                        full = full.Substring(0, full.Length - 1); // convert exception from .gdc to .gd
                        File.WriteAllText(full, decompiled);
                    } else {
                        File.WriteAllBytes(full, entry.Data);
                    }

                    int percentage = (int)Math.Floor((i + 1) / (double)pck.Entries.Count * 100.0);
                    Console.Write("\rUnpacking: " + (i + 1) + "/" + pck.Entries.Count + " (" + percentage + "%)");
                } catch (Exception e) {
                    Console.WriteLine("\nError while decoding file: " + path);
                    Console.WriteLine(e);
                    Environment.Exit(1);
                }
            }
            Console.WriteLine();
        }

        private static void Build(BuildOptions options) {
            if (!Directory.Exists(options.InputPath)) {
                Console.WriteLine("Invalid directory (does not exist): " + options.InputPath);
                return;
            }

            if (!File.Exists(Path.Combine(options.InputPath, "project.binary"))) {
                Console.WriteLine("Invalid project (project.binary file not present in directory): " + options.InputPath);
                return;
            }

            PckFile pck = new PckFile(1, 3, 3, 3);
            BytecodeProvider provider = new BytecodeProvider(0x5565f55);
            string[] files = Directory.GetFiles(options.InputPath, "*", SearchOption.AllDirectories);
            pck.Entries.Capacity = files.Length;
            for (int i = 0; i < files.Length; i++) {
                string file = files[i];
                string relative = Path.GetRelativePath(options.InputPath, file);
                try {
                    string withPrefix = "res://" + relative.Replace('\\', '/');
                    byte[] contents = File.ReadAllBytes(file);
                    if (relative.EndsWith(".gd")) {
                        contents = GdScriptCompiler.Compile(Encoding.UTF8.GetString(contents), provider);
                        withPrefix += "c"; // convert ".gd" to ".gdc"
                    }
                    pck.Entries.Add(new PckFileEntry {
                        Path = withPrefix,
                        Data = contents
                    });

                    int percentage = (int)Math.Floor((i + 1) / (double)files.Length * 100.0);
                    Console.Write("\rPacking: " + (i + 1) + "/" + files.Length + " (" + percentage + "%)");
                } catch (Exception e) {
                    Console.WriteLine("\nError while building file: " + relative);
                    Console.WriteLine(e);
                    Environment.Exit(1);
                }
            }

            Console.WriteLine();

            byte[] serialized = pck.ToBytes();
            string outputFile = options.InputPath + ".pck";
            if (options.OutputFile != null) {
                outputFile = options.OutputFile;
            }
            Console.WriteLine("Writing PCK file to disk... ");
            File.WriteAllBytes(outputFile, serialized);
        }
    }
}
