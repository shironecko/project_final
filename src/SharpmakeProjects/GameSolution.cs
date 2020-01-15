using Sharpmake;
using SharpmakeProjects.Projects;

[module: Include("Options.cs")]
[module: Include("Projects/GameProject.cs")]

namespace SharpmakeProjects {
    [Generate]
    public class GameSolution : Solution {
        public GameSolution() {
            Name = "Game";

            var devEnv = Util.IsRunningOnUnix() ? DevEnv.make : DevEnv.vs2019;
            AddTargets(Options.GetCommonTarget());
        }

        [Configure]
        public void ConfigureAll(Solution.Configuration conf, Target target) {
            conf.SolutionPath = Options.ProjectsRoot;
            conf.SolutionFileName = "[solution.Name].[target.Platform].[target.DevEnv]";
            conf.AddProject<GameProject>(target);
        }
    }
}