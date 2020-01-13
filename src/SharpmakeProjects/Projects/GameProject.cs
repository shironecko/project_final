using System.IO;
using Sharpmake;

[module: Include("BaseProject.cs")]

namespace SharpmakeProjects.Projects {
    [Generate]
    public class GameProject : BaseProject {
        public sealed override string ProjectFolder => Path.Combine(Options.SourceRoot, "Game");
        public override string TargetFileName => "[project.Name]_[target.Optimization]";
        public override Configuration.OutputType OutputType => Configuration.OutputType.Exe;

        public GameProject() : base() {
            SourceRootPath = ProjectFolder;
            AddTargets(Options.GetCommonTarget());
        }
    }
}