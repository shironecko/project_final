#!/usr/bin/env dotnet script
#r "nuget: Microsoft.CodeAnalysis.CSharp, 3.2.0"
#r "nuget: RazorEngine.NetCore, 2.2.2"
#r "nuget: CommandLineParser, 2.6.0"

using System.Runtime.CompilerServices;
using System.Reflection;
using RazorEngine.Text;
using RazorEngine.Templating;
using RazorEngine.Roslyn.CSharp;
using RazorEngine.Configuration;
using RazorEngine;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using CommandLine;

const string META_CODE_EXTENSION = ".csx";
const string TEMPLATE_EXTENSION = ".t.cpp";
const string GEN_CODE_EXTENSION = ".g.cpp";
const string GEN_HEADER_EXTENSION = ".g.h";

static string ExtensionMask(string extension) {
    return $"*{extension}";
}

public class RazorInjectionHack {
    public static void Install() {
        MethodInfo methodToReplace = typeof(RoslynCompilerServiceBase).GetMethod("IsMono", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        MethodInfo methodToInject = typeof(RazorInjectionHack).GetMethod("IsMono_Injected", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        RuntimeHelpers.PrepareMethod(methodToReplace.MethodHandle);
        RuntimeHelpers.PrepareMethod(methodToInject.MethodHandle);

        unsafe {
            if (IntPtr.Size == 4) {
                int* inj = (int*)methodToInject.MethodHandle.Value.ToPointer() + 2;
                int* tar = (int*)methodToReplace.MethodHandle.Value.ToPointer() + 2;
#if DEBUG
                Console.WriteLine("\nVersion x86 Debug\n");

                byte* injInst = (byte*)*inj;
                byte* tarInst = (byte*)*tar;

                int* injSrc = (int*)(injInst + 1);
                int* tarSrc = (int*)(tarInst + 1);

                *tarSrc = (((int)injInst + 5) + *injSrc) - ((int)tarInst + 5);
#else
                Console.WriteLine("\nVersion x86 Release\n");
                *tar = *inj;
#endif
            } else {
                long* inj = (long*)methodToInject.MethodHandle.Value.ToPointer()+1;
                long* tar = (long*)methodToReplace.MethodHandle.Value.ToPointer()+1;
#if DEBUG
                Console.WriteLine("\nVersion x64 Debug\n");
                byte* injInst = (byte*)*inj;
                byte* tarInst = (byte*)*tar;


                int* injSrc = (int*)(injInst + 1);
                int* tarSrc = (int*)(tarInst + 1);

                *tarSrc = (((int)injInst + 5) + *injSrc) - ((int)tarInst + 5);
#else
                Console.WriteLine("\nVersion x64 Release\n");
                *tar = *inj;
#endif
            }
        }
    }

    private static bool IsMono_Injected() {
        return true;
    }
}

Console.WriteLine("Installing Razor injection hack...");
RazorInjectionHack.Install();

static Assembly CompileFilesIntoAssembly(string directory, string searchMask) {
    static SyntaxTree CreateSyntaxTree(string path) {
        Console.WriteLine($"Parsing file: {path}");
        string sourceCode = File.ReadAllText(path);
        return CSharpSyntaxTree.ParseText(sourceCode);
    }

    var syntaxTrees = 
        (from filePath in Directory.GetFiles(directory, searchMask, SearchOption.AllDirectories)
        select CreateSyntaxTree(filePath)).ToList();

    CSharpCompilation compilation = CSharpCompilation.Create(
        "MetaCode",
        syntaxTrees,
        new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    using var dllStream = new MemoryStream();
    using var pdbStream = new MemoryStream();
    var emitResult = compilation.Emit(dllStream, pdbStream);
    if (!emitResult.Success) {
        // emitResult.Diagnostics
        foreach (var diagnostic in emitResult.Diagnostics) {
            Console.WriteLine(diagnostic.ToString());
        }
        return null;
    } else {
        var assembly = Assembly.Load(dllStream.GetBuffer());
        return assembly;
    }
}

public class Options {
    [Option("meta", Required = true, HelpText = "Directory containing C# files with data structures for which code should be generated.")]
    public string MetaCodeDirectory { get; set; }

    [Option("templates", Required = true, HelpText = "Directory containing Razor templates for code generation.")]
    public string TemplatesDirectory { get; set; }

    [Option("out", Required = true, HelpText = "Directory to write generated code into.")]
    public string OutputDirectory { get; set; }

    [Option(Default = ".csx", HelpText = "Extension of meta code C# files.")]
    public string MetaCodeExtension { get; set; }

    [Option(Default = "t", HelpText = "Prefix for which to look when searching template files. For example 't' prefix will match 'source.t.cpp' file.")]
    public string TemplateExtensionPrefix { get; set; }

    [Option(Default = "g", HelpText = "Template extension prefix will be substituted for this prefix when generating output files. For example 'source.t.cpp' template will produce 'source.g.cpp' output file with default settings.")]
    public string GeneratedExtensionPrefix { get; set; }
}

Options options = null;
Parser.Default.ParseArguments<Options>(Args)
    .WithParsed(o => options = o)
    .WithNotParsed(errors => Environment.Exit(-1));

Assembly assembly = CompileFilesIntoAssembly(options.MetaCodeDirectory, ExtensionMask(options.MetaCodeExtension));
Debug.Assert(assembly != null);

Console.WriteLine("Exported types:");
foreach (Type type in assembly.GetExportedTypes()) {
    Console.WriteLine(type);
}

static void GenerateCodeFromTemplates(Assembly assembly, string templatesPath, string outPath) {
    if (Directory.Exists(outPath)) {
        var oldGeneratedFiles = Directory.GetFiles(outPath, ExtensionMask(GEN_CODE_EXTENSION), SearchOption.AllDirectories).ToList();
        oldGeneratedFiles.AddRange(Directory.GetFiles(outPath, ExtensionMask(GEN_HEADER_EXTENSION), SearchOption.AllDirectories));
        Console.WriteLine("Removing old generated files:");
        foreach (string path in oldGeneratedFiles) {
            Console.WriteLine(path);
            File.Delete(path);
        }
    } else {
        Directory.CreateDirectory(outPath);
    }

    {
        var templateConfig = new TemplateServiceConfiguration {
            CompilerServiceFactory = new RazorEngine.Roslyn.RoslynCompilerServiceFactory(),
            Language = Language.CSharp,
            EncodedStringFactory = new RawStringFactory()
        };
        var razorEngine = RazorEngineService.Create(templateConfig);
        Engine.Razor = razorEngine;
    }

    var templates =
        (from path in Directory.GetFiles(templatesPath, ExtensionMask(TEMPLATE_EXTENSION), SearchOption.AllDirectories)
        select new { path, relativePath = Path.GetRelativePath(templatesPath, path), content = File.ReadAllText(path) }).ToList();
    
    var templateModel = new { Types = assembly.GetExportedTypes() };
    System.Console.WriteLine("Templates:");
    foreach (var template in templates) {
        System.Console.WriteLine($"In path:  {template.path}");
        string templateName = Path.GetFileName(template.path);
        Debug.Assert(templateName.EndsWith(TEMPLATE_EXTENSION));
        templateName = string.Concat(templateName.Take(templateName.Length - TEMPLATE_EXTENSION.Length));
        string templateOutPath = Path.Combine(
            outPath, 
            Path.Combine(Path.GetDirectoryName(template.relativePath), templateName + GEN_CODE_EXTENSION)
        );
        System.Console.WriteLine($"Out path: {templateOutPath}");

        var templateResult =
            Engine.Razor.RunCompile(template.content, template.path, null, templateModel);

        Directory.CreateDirectory(Path.GetDirectoryName(templateOutPath));
        File.WriteAllText(templateOutPath, templateResult);
    }
}

GenerateCodeFromTemplates(assembly, options.TemplatesDirectory, options.OutputDirectory);