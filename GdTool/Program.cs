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

            [Option('b', "bytecode-version", Required = true, HelpText = "The commit hash of the bytecode version to use.")]
            public string BytecodeVersion { get; set; }

            [Option('d', "decompile", Required = false, HelpText = "Decompiles GDC files when in decode mode.")]
            public bool Decompile { get; set; }

            [Option('o', "out", Required = false, HelpText = "Output directory to extract files to.")]
            public string OutputDirectory { get; set; }
        }

        [Verb("build", HelpText = "Packs a directory into a PCK file.")]
        public class BuildOptions {
            [Option('i', "in", Required = true, HelpText = "The directory to pack.")]
            public string InputPath { get; set; }

            [Option('b', "bytecode-version", Required = true, HelpText = "The commit hash of the bytecode version to use.")]
            public string BytecodeVersion { get; set; }

            [Option('o', "out", Required = false, HelpText = "Output file to place the PCK.")]
            public string OutputFile { get; set; }
        }

        [Verb("detect", HelpText = "Detects information for a game executable without executing it.")]
        public class DetectVersionOptions {
            [Option('i', "in", Required = true, HelpText = "The game executable to probe information from.")]
            public string InputPath { get; set; }
        }

        static void Main(string[] args) {
            Parser.Default.ParseArguments<DecodeOptions, BuildOptions, DetectVersionOptions>(args)
                .WithParsed<DecodeOptions>(Decode)
                .WithParsed<BuildOptions>(Build)
                .WithParsed<DetectVersionOptions>(DetectVersion);
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

            BytecodeProvider provider = BytecodeProvider.GetByCommitHash(options.BytecodeVersion);
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

            PckFile pck = new PckFile(1, 3, 3, 3); // TODO: automate or require as an argument
            BytecodeProvider provider = BytecodeProvider.GetByCommitHash(options.BytecodeVersion);
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

        private static void DetectVersion(DetectVersionOptions options) {
            if (!File.Exists(options.InputPath)) {
                Console.WriteLine("Invalid game executable file (does not exist): " + options.InputPath);
                return;
            }

            byte[] binary = File.ReadAllBytes(options.InputPath);
            BytecodeProvider provider = VersionDetector.Detect(binary);
            if (provider == null) {
                Console.WriteLine("A known commit hash could not be found within the binary. Are you sure you supplied a Godot game executable?");
                Console.WriteLine("If you definitely passed a valid executable, it might be compiled with a version newer than this build of GdTool.");
                Console.WriteLine("If this is the case, try compiling with the newest bytecode version GdTool supports if it's still compatible.");
                return;
            }

            Console.WriteLine("Bytecode version hash: " + provider.ProviderData.CommitHash.Substring(0, 7) + " (" + provider.ProviderData.CommitHash + ")");
            Console.WriteLine(provider.ProviderData.Description);
        }
    }
}
