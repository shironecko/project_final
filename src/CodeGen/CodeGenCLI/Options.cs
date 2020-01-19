using System.IO;

namespace CodeGenCLI {
    public static class Options {
        public static string Root => "";

        public static string SourceRoot => Path.Combine(Root, "src");
        public static string BuildRoot => Path.Combine(Root, "build");
        public static string ProjectsRoot => Path.Combine(BuildRoot, "projects");
        public static string CodeGenRoot => Path.Combine(BuildRoot, "autogen");
        public static string OutputRoot => Path.Combine(BuildRoot, "output");
        public static string TempRoot => Path.Combine(BuildRoot, "tmp");
    }
}