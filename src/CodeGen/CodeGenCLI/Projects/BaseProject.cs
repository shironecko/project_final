using System;
using System.IO;
using CodeGenCLI.ProjectGeneration;

namespace CodeGenCLI.Projects {
    public class BaseProject : IProject {
        public string Name => this.GetType().Name;
        public Type[] Dependencies => new Type[] { };

        public virtual void Configure(Configuration cfg) {
            cfg.ProjectName = Name;
            cfg.ProjectPath = Options.ProjectsRoot;
            cfg.ArtifactName = Name;
            cfg.ArtifactPath = Options.OutputRoot;
            cfg.TempPath = Path.Combine(Options.TempRoot, Name);
            
            cfg.AddOptions(
                GenericOptions.CppStd.Cpp17,
                GenericOptions.Optimization.None,
                GenericOptions.DebugInfo.On,
                GenericOptions.Exceptions.Off,
                GenericOptions.RTTI.Off);
        }
    }
}