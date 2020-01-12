using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace SharpGen.CompilationHelper {
    public static class CompilationHelper {
        public static Assembly CompileFilesIntoAssembly(string sourceRoot, string sourceExtension = ".cs") {
            var filePaths = Directory.EnumerateFiles(sourceRoot, $"*{sourceExtension}", SearchOption.AllDirectories);
            return CompileFilesIntoAssembly(filePaths);
        }

        /// <summary>
        /// Helper function for compiling a bunch of CSharp files into an assembly.
        /// Intended only for making initial integration faster,
        /// you'll be better of with building an assembly using your usual tools in the long run.
        /// </summary>
        /// <returns>Compiled assembly or null in case of errors</returns>
        public static Assembly CompileFilesIntoAssembly(IEnumerable<string> filePaths) {
            static SyntaxTree CreateSyntaxTree(string path) {
                Console.WriteLine($"Parsing code file: {path}");
                try {
                    string sourceCode = File.ReadAllText(path);
                    var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

                    return syntaxTree;
                } catch (Exception e) {
                    Console.WriteLine($"Error while parsing file {path}: {e.ToString()}");

                    return null;
                }
            }

            var syntaxTrees = filePaths.Select(CreateSyntaxTree)
                .Where(tree => tree != null)
                .ToList();

            var compilation = CSharpCompilation.Create(
                "MetaCode",
                syntaxTrees,
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var dllStream = new MemoryStream();
            using var pdbStream = new MemoryStream();
            var emitResult = compilation.Emit(dllStream, pdbStream);
            if (!emitResult.Success) {
                foreach (var diagnostic in emitResult.Diagnostics) {
                    Console.WriteLine(diagnostic.ToString());
                }

                return null;
            }

            var assembly = Assembly.Load(dllStream.GetBuffer());
            return assembly;
        }
    }
}