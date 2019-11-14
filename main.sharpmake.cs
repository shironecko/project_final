using System;
using System.IO;
using Sharpmake;

static class Utils
{
    public static bool IsPosixPlatform()
    {
        int p = (int) Environment.OSVersion.Platform;
        return (p == 4) || (p == 6) || (p == 128);
    }
}

[Generate]
class GameProject : Project
{
    public GameProject()
    {
        Name = "Game";
        SourceRootPath = Path.Combine("[project.SharpmakeCsPath]", "src");

        AddTargets(new Target(
            Platform.win64 | Platform.mac,
            DevEnv.vs2019 | DevEnv.make,
            Optimization.Debug | Optimization.Release));
    }

    [Configure]
    public void ConfigureAll(Project.Configuration conf, Target target)
    {
        conf.ProjectPath = Path.Combine("[project.SharpmakeCsPath]", "build/projects");
        conf.ProjectFileName = "[project.Name].[target.Platform].[target.DevEnv]";
    }
}

[Generate]
class GameSolution : Solution
{
    public GameSolution()
    {
        Name = "Game";

        var devEnv = Utils.IsPosixPlatform() ? DevEnv.make : DevEnv.vs2019;
        AddTargets(new Target(
            Platform.win64 | Platform.mac,
            devEnv,
            Optimization.Debug | Optimization.Release));
    }

    [Configure]
    public void ConfigureAll(Solution.Configuration conf, Target target)
    {
        conf.SolutionPath = Path.Combine("[solution.SharpmakeCsPath]", "build/projects");
        conf.SolutionFileName = "[solution.Name].[target.Platform].[target.DevEnv]";
        conf.AddProject<GameProject>(target);
    }

    [Main]
    public static void SharpmakeMain(Arguments sharpmakeArgs)
    {
        sharpmakeArgs.Generate<GameSolution>();
    }
}