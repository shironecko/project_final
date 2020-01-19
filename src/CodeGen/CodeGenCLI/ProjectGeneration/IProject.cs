using System;

namespace CodeGenCLI.ProjectGeneration {
    public interface IProject {
        string Name { get; }
        Type[] Dependencies { get; }

        void Configure(Configuration cfg);
    }
}