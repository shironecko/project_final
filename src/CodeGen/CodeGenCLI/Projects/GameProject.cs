using System.IO;
using CodeGenCLI.ProjectGeneration;

namespace CodeGenCLI.Projects {
    public class GameProject : BaseProject {
        public override void Configure(Configuration cfg) {
            base.Configure(cfg);

            cfg.SourceRoot = Path.Combine(Options.SourceRoot, "Game");
            cfg.Type = Configuration.ArtifactType.Executable;
        }
    }
}