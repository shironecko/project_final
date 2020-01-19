using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeGenCLI.ProjectGeneration.Compilers;
using RazorLight;

namespace CodeGenCLI.ProjectGeneration.BuildSystems {
    public struct FileWriteRequest {
        public FileWriteRequest(string path, string content) {
            Path = path;
            Content = content;
        }

        public string Path { get; private set; }
        public string Content { get; private set; }
    }

    public class TemplateModel {
        public TemplateModel(
            IProject project,
            Configuration configuration,
            string compilerArgs,
            List<string> sourceFiles) {
            Project = project;
            Configuration = configuration;
            CompilerArgs = compilerArgs;
            SourceFiles = sourceFiles;
        }

        public IProject Project { get; private set; }
        public Configuration Configuration { get; private set; }
        public string CompilerArgs { get; private set; }
        public List<string> SourceFiles { get; private set; }
    }

    public class Ninja {
        public async Task<IEnumerable<FileWriteRequest>> Generate(
            RazorLightEngine razor,
            IProject project,
            Configuration cfg) {
            var resultingFiles = new List<FileWriteRequest>();

            var compiler = new Clang();
            var sourceFiles = Directory.EnumerateFiles(cfg.SourceRoot)
                .Where(file => cfg.SourceExtensions.Contains(Path.GetExtension(file)))
                .Select(file => Path.GetRelativePath(cfg.SourceRoot, file))
                .ToList();
            var templateModel = new TemplateModel(project, cfg, GenerateCompilerArgs(cfg, compiler), sourceFiles);

            string generatedProject = await razor.CompileRenderAsync("Ninja.Project.ninja.cshtml", templateModel);
            string outPath = Path.Combine(cfg.ProjectPath ?? "", $"{cfg.ProjectName ?? "untitled"}.ninja");
            resultingFiles.Add(new FileWriteRequest(outPath, generatedProject));

            return resultingFiles;
        }

        private string GenerateCompilerArgs(Configuration cfg, ICompiler compiler) {
            string args = string.Join(' ', cfg.Options.Select(compiler.GenericOptionToCommandLine));
            return args;
        }
    }
}