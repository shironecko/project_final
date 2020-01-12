using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using RazorLight;
using RazorLight.Razor;

namespace SharpGen {
    public static class TemplateCompiler {
        public static async Task CompileCodeTemplates(
            string templatesRoot,
            string outputRoot,
            object model,
            string templateFileExtension = ".razor") {
            var relativeFilePaths = FileUtil.EnumerateFilesWithRelativePaths(templatesRoot, $"*{templateFileExtension}");
            await CompileCodeTemplates(templatesRoot, outputRoot, model, relativeFilePaths, templateFileExtension);
        }

        public static async Task CompileCodeTemplates(
            string templatesRoot,
            string outputRoot,
            object model,
            IEnumerable<string> relativeFilePaths,
            string templateFileExtension = ".razor") {
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(templatesRoot, templateFileExtension)
                .UseMemoryCachingProvider()
                .Build();

            var filePathList = relativeFilePaths.ToList();
            foreach (string filePath in filePathList) {
                Console.WriteLine($"Compiling and rendering template {filePath}...");
                string compiledSource = await engine.CompileRenderAsync(filePath, model);

                string outRelativePath = filePath;
                if (!string.IsNullOrEmpty(templateFileExtension)) {
                    outRelativePath = outRelativePath.Substring(
                        0,
                        outRelativePath.Length - templateFileExtension.Length);
                }

                string outPath = Path.Combine(outputRoot, outRelativePath);
                Console.WriteLine($"Writing result to {outPath}...");
                Directory.CreateDirectory(Path.GetDirectoryName(outPath));
                File.WriteAllText(outPath, compiledSource);
            }
        }
    }
}