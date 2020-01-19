namespace CodeGenCLI.ProjectGeneration.Compilers {
    public interface ICompiler {
        string DefaultCompilerExecutable { get; }
        string GenericOptionToCommandLine(object option);
    }
}