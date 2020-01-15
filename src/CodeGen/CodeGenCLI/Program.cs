using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MetaTypes;
using RazorLight;

namespace CodeGenCLI {
    internal static class Program {
        private static string OutputPath => "build/autogen";

        static async Task Main(string[] args) {
            var typesAssembly = Assembly.GetAssembly(typeof(TestClassA));
            Debug.Assert(typesAssembly != null);

            var razorEngine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(Assembly.GetExecutingAssembly(), "Templates.CodeGen")
                .UseMemoryCachingProvider()
                .Build();

            var templates = new[] { "Types.h" };
            foreach (string template in templates) {
                Console.WriteLine($"Rendering template {template}...");
                string namespacedTemplate = template.Replace('/', '.');
                string compiledSrc = await razorEngine.CompileRenderAsync(namespacedTemplate, typesAssembly);

                string outFilePath = Path.Combine(OutputPath, template);
                string outDirectory = Path.GetDirectoryName(outFilePath);
                if (!string.IsNullOrWhiteSpace(outDirectory)) {
                    Directory.CreateDirectory(outDirectory);
                }

                File.WriteAllText(outFilePath, compiledSrc);
            }
        }
    }
}