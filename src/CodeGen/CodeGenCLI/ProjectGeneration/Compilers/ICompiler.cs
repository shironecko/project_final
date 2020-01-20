namespace CodeGenCLI.ProjectGeneration.Compilers {
    public interface ICompiler {
        public enum DepsOutputType {
            GCC,
            MSVC,
        }
        
        DepsOutputType DepsType { get; }
        
        string DefaultCompilerExecutable { get; }
        string ArgsInputPrefix { get; }
        string ArgsOutputPrefix { get; }
        string GenericOptionToCommandLine(object option);
    }
}