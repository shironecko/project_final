using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CodeGenCLI.ProjectGeneration;
using CodeGenCLI.ProjectGeneration.BuildSystems;
using CodeGenCLI.Projects;
using MetaTypes;
using RazorLight;

namespace CodeGenCLI {
    internal static class Program {
        static async Task Main(string[] args) {
            var razor = new RazorLightEngineBuilder()
                .DisableEncoding()
                .UseEmbeddedResourcesProject(Assembly.GetExecutingAssembly(), "Templates.BuildSystems")
                .UseMemoryCachingProvider()
                .Build();

            var project = new GameProject();
            var configuration = new Configuration();
            project.Configure(configuration);
            var ninja = new Ninja();
            var generatedFiles = await ninja.Generate(razor, project, configuration);
            foreach (var file in generatedFiles) {
                Directory.CreateDirectory(Path.GetDirectoryName(file.Path));
                File.WriteAllText(file.Path, file.Content);
            }

            {
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

                    string outFilePath = Path.Combine(Options.CodeGenRoot, template);
                    string outDirectory = Path.GetDirectoryName(outFilePath) ?? "";
                    if (!string.IsNullOrWhiteSpace(outDirectory)) {
                        Directory.CreateDirectory(outDirectory);
                    }

                    File.WriteAllText(outFilePath, compiledSrc);
                }
            }
        }
    }
}