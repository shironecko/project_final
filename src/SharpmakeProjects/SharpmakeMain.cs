using System;
using System.IO;
using Sharpmake;
using SharpmakeProjects.Projects;

[module: Include("GameSolution.cs")]

namespace SharpmakeProjects {
    public static class SharpmakeConfiguration {
        [Main]
        public static void SharpmakeMain(Arguments sharpmakeArgs) { sharpmakeArgs.Generate<GameSolution>(); }
    }
}