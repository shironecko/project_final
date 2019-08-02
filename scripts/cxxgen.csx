#r "nuget: Microsoft.CodeAnalysis.CSharp, 3.2.0"
#r "nuget: RazorEngine.NetCore, 2.2.2"

using System.Runtime.CompilerServices;
using System.Reflection;
using RazorEngine.Text;
using RazorEngine.Templating;
using RazorEngine.Roslyn.CSharp;
using RazorEngine.Configuration;
using RazorEngine;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

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
        Console.WriteLine("Injected IsMono() function is called, returning 'true'!");
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

Assembly assembly = CompileFilesIntoAssembly("../src/", "*.csx");
Debug.Assert(assembly != null);

Console.WriteLine("Exported types:");
foreach (Type type in assembly.GetExportedTypes()) {
    Console.WriteLine(type);
}

string template = File.ReadAllText("../src/gen/templates/test.t.cpp");
var config = new TemplateServiceConfiguration();
config.CompilerServiceFactory = new RazorEngine.Roslyn.RoslynCompilerServiceFactory();
config.Language = Language.CSharp;
config.EncodedStringFactory = new RawStringFactory();
var service = RazorEngineService.Create(config);
Engine.Razor = service;
var result =
	Engine.Razor.RunCompile(template, "templateKey", null, new { Types = assembly.GetExportedTypes() });
Console.WriteLine(result);
