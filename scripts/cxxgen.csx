#r "nuget: Microsoft.CodeAnalysis.CSharp, 3.2.0"

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

Func<string, SyntaxTree> createSyntaxTree = (string path) => {
    Console.WriteLine($"Parsing file: {path}");
    string sourceCode = File.ReadAllText(path);
    return CSharpSyntaxTree.ParseText(sourceCode);
};

var syntaxTrees = 
    (from filePath in Directory.GetFiles("../src/", "*.csx", SearchOption.AllDirectories)
    select createSyntaxTree(filePath)).ToList();

CSharpCompilation compilation = CSharpCompilation.Create(
    "MetaCode",
    syntaxTrees,
    new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

using (var dllStream = new MemoryStream())
using (var pdbStream = new MemoryStream()) {
    var emitResult = compilation.Emit(dllStream, pdbStream);
    if (!emitResult.Success) {
        // emitResult.Diagnostics
        foreach(var diagnostic in emitResult.Diagnostics) {
            Console.WriteLine(diagnostic.ToString());
        }
    } else {
        var assembly = Assembly.Load(dllStream.GetBuffer());
        Console.WriteLine("Exported types:");
        foreach (Type type in assembly.GetExportedTypes()) {
            Console.WriteLine(type);
        }
    }
}