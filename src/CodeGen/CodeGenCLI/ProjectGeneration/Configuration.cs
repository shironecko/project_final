using System;
using System.Collections.Generic;

namespace CodeGenCLI.ProjectGeneration {
    public class Configuration {
        #region Paths
        
        public string? SourceRoot { get; set; }
        public List<string> SourceExtensions { get; set; } = new List<string>() { ".cpp" };
        public List<string> HeaderExtensions { get; set; } = new List<string>() { ".hpp", ".h" };

        public string? ProjectPath { get; set; }
        public string? ProjectName { get; set; }

        public string? ArtifactPath { get; set; }
        public string? ArtifactName { get; set; }

        public string? TempPath { get; set; }

        #endregion

        public enum ArtifactType {
            None,
            Executable,
            StaticLib,
            DynamicLib,
        }

        public ArtifactType Type { get; set; }

        [Flags]
        public enum Scope {
            Private = 1 << 0,            // used in project internally
            Exported = 1 << 1,           // inherited by projects depending on this one
            Public = Private | Exported, // both
        }

        public void AddDefines(Scope scope, params string[] defines) {
            AddScopedStrings(scope, defines, DefinesPrivate, DefinesExported);
        }

        public void AddIncludePaths(Scope scope, params string[] paths) {
            AddScopedStrings(scope, paths, IncludePathsPrivate, IncludePathsExported);
        }

        public void AddLibraryPaths(Scope scope, params string[] paths) {
            AddScopedStrings(scope, paths, LibraryPathsPrivate, LibraryPathsExported);
        }

        public void AddLibraryFiles(Scope scope, params string[] paths) {
            AddScopedStrings(scope, paths, LibraryFilesPrivate, LibraryFilesExported);
        }

        public void AddOptions(params object[] options) { Options.AddRange(options); }

        public List<string> DefinesPrivate { get; private set; } = new List<string>();
        public List<string> DefinesExported { get; private set; } = new List<string>();
        public List<string> IncludePathsPrivate { get; private set; } = new List<string>();
        public List<string> IncludePathsExported { get; private set; } = new List<string>();
        public List<string> LibraryPathsPrivate { get; private set; } = new List<string>();
        public List<string> LibraryPathsExported { get; private set; } = new List<string>();
        public List<string> LibraryFilesPrivate { get; private set; } = new List<string>();
        public List<string> LibraryFilesExported { get; private set; } = new List<string>();

        public List<object> Options { get; private set; } = new List<object>();

        private void AddScopedStrings(
            Scope scope,
            string[] strings,
            List<string> privateStorage,
            List<string> exportedStorage) {
            if (scope.HasFlag(Scope.Private)) {
                privateStorage.AddRange(strings);
            }

            if (scope.HasFlag(Scope.Exported)) {
                exportedStorage.AddRange(strings);
            }
        }
    }
}