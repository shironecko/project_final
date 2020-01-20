using System;
using System.Diagnostics.CodeAnalysis;

namespace CodeGenCLI.ProjectGeneration.Compilers {
    public class Clang : ICompiler {
        public ICompiler.DepsOutputType DepsType => ICompiler.DepsOutputType.GCC;
        public string DefaultCompilerExecutable => "clang++";
        public string ArgsInputPrefix => "-c";
        public string ArgsOutputPrefix => "-o";

        public string GenericOptionToCommandLine(object option) =>
            option switch {
                GenericOptions.CppStd std => std switch {
                    GenericOptions.CppStd.Cpp03 => "-std=c++03",
                    GenericOptions.CppStd.Cpp11 => "-std=c++11",
                    GenericOptions.CppStd.Cpp14 => "-std=c++14",
                    GenericOptions.CppStd.Cpp17 => "-std=c++17",
                    object obj => throw new NotImplementedException($"CPP STD value of {obj} is not supported!")
                },
                GenericOptions.Optimization opt => opt switch {
                    GenericOptions.Optimization.None => "-O0",
                    GenericOptions.Optimization.Speed => "-O3",
                    GenericOptions.Optimization.Size => "-Os",
                    GenericOptions.Optimization.Full => "-O3",
                    object obj => throw new NotImplementedException($"Optimization value of {obj} is not supported!")
                },
                GenericOptions.DebugInfo dbg => dbg switch {
                    GenericOptions.DebugInfo.Off => "-g0",
                    GenericOptions.DebugInfo.On => "-g",
                    object obj => throw new NotImplementedException($"Debug info option of {obj} is not supported!") 
                },
                GenericOptions.RTTI rtti => rtti switch {
                    GenericOptions.RTTI.Off => "-fno-rtti",
                    GenericOptions.RTTI.On => "-frtti",
                    object obj => throw new NotImplementedException($"RTTI setting of {obj} is not supported!") 
                },
                GenericOptions.Exceptions ex => ex switch {
                    GenericOptions.Exceptions.Off => "-fno-exceptions",
                    GenericOptions.Exceptions.On => "-fexceptions",
                    object obj => throw new NotImplementedException($"Exception setting of {obj} in not supported!")
                },
                object unknown => throw new NotImplementedException(
                    $"Conversion is undefined to type {unknown.GetType()}"),
            };
    }
}