using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using SharpGen;
using SharpGen.CompilationHelper;

namespace SharpGenCLI {
    class Program {
        public class Options {
            [Option(
                "csharp-path",
                Required = true,
                HelpText = "Directory containing C# files with data structures for which code should be generated.")]
            public string CSharpPath { get; set; }

            [Option(
                "templates-path",
                Required = true,
                HelpText = "Directory containing Razor templates for code generation.")]
            public string TemplatesPath { get; set; }

            [Option("out", Required = true, HelpText = "Directory to write generated code into.")]
            public string OutputPath { get; set; }

            [Option(Default = ".cs", HelpText = "Extension of meta code C# files.")]
            public string CSharpExtension { get; set; }

            [Option(Default = ".razor", HelpText = "Extension of template files.")]
            public string TemplatesExtension { get; set; }
        }

        static async Task Main(string[] args) {
            Options options = null;
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o => options = o)
                .WithNotParsed(errors => Environment.Exit(-1));

            string csharpAbsPath = Path.GetFullPath(options.CSharpPath);
            string templatesAbsPath = Path.GetFullPath(options.TemplatesPath);

            var assembly = CompilationHelper.CompileFilesIntoAssembly(
                csharpAbsPath,
                options.CSharpExtension);
            
            await TemplateCompiler.CompileCodeTemplates(
                templatesAbsPath,
                options.OutputPath,
                assembly);
        }
    }
}