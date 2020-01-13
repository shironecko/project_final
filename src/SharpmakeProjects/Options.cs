using System;
using System.IO;
using Sharpmake;

namespace SharpmakeProjects {
    public static class Options {
        public static string Root =>
            Util.PathMakeStandard(
                Path.GetFullPath(
                    Path.Combine(Util.GetCurrentSharpmakeFileInfo().DirectoryName, "..", "..")));

        public static string SourceRoot => Path.Combine(Root, "src");
        public static string BuildRoot => Path.Combine(Root, "build");
        public static string ProjectsRoot => Path.Combine(BuildRoot, "projects");
        public static string OutputRoot => Path.Combine(BuildRoot, "output");
        public static string TempRoot => Path.Combine(BuildRoot, "tmp");

        public static Target GetCommonTarget(OutputType outputType = OutputType.Lib) =>
            new Target(
                Platform.win64 | Platform.mac,
                Util.IsRunningOnUnix() ? DevEnv.make : DevEnv.make | DevEnv.vs2019,
                Optimization.Debug | Optimization.Release,
                outputType);


        #region Path Forming Methods

        public static string GetProjectPath(Target target) {
            return Path.Combine(
                ProjectsRoot,
                GetPlatformDevEnvPath(target));
        }

        public static string GetTargetPath(Target target) {
            return Path.Combine(
                Options.OutputRoot,
                $"{GetPlatformDevEnvPath(target)}_{GetOptimizationModeOutputTypeFolderName(target)}");
        }

        public static string GetIntermediatePath(Target target) {
            return Path.Combine(
                Options.TempRoot,
                "Obj",
                GetPlatformDevEnvPath(target),
                "[project.Name]",
                GetOptimizationModeOutputTypeFolderName(target));
        }

        private static string GetPlatformDevEnvPath(Target target) {
            string devEnvPath = $"{target.Platform}_{target.DevEnv}";
            return devEnvPath;
        }

        private static string GetOptimizationModeOutputTypeFolderName(Target target) {
            return $"{target.Optimization}{ToPathExtension(target.OutputType)}";
        }

        private static string ToPathExtension(OutputType outputType) {
            switch (outputType) {
                case OutputType.Dll:
                    return "_dll";
                case OutputType.Lib:
                    return string.Empty;
                default:
                    throw new Error("Unknown OutputType type");
            }
        }

        #endregion Path Forming Methods
    }
}