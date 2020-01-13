using System.IO;
using Sharpmake;

[module: Include("Options.cs", IncludeType.NearestMatchInParentPath)]

namespace SharpmakeProjects.Projects {
    public abstract class BaseProject : Project {
        public abstract string ProjectFolder { get; }
        public abstract string TargetFileName { get; }

        public abstract Configuration.OutputType OutputType { get; }

        protected BaseProject()
            : base() {
            IsFileNameToLower = false;
            IsTargetFileNameToLower = false;

            RootPath = Options.Root;
        }

        [Configure]
        public virtual void ConfigureAll(Configuration conf, Target target) {
            conf.SolutionFolder = ProjectFolder;

            conf.ProjectName = Name;
            conf.ProjectFileName = Name;
            conf.Output = OutputType;
            conf.ProjectPath = Options.GetProjectPath(target);
            conf.TargetPath = Options.GetTargetPath(target);
            conf.TargetFileName = TargetFileName;
            conf.IntermediatePath = Options.GetIntermediatePath(target);

            conf.IncludePrivatePaths.Add(Options.Root);

            conf.Options.Add(Sharpmake.Options.Vc.Compiler.Exceptions.Disable);
            conf.Options.Add(Sharpmake.Options.Vc.Compiler.CppLanguageStandard.CPP17);
        }

        [Configure(Platform.win64)]
        public virtual void ConfigureWin64(Configuration conf, Target target) {
            conf.Defines.Add("WIN64");
            conf.Defines.Add("_WINDOWS");
            conf.Defines.Add("_SCL_SECURE_NO_WARNINGS");
            conf.Defines.Add("_CRT_SECURE_NO_WARNINGS");

            conf.Defines.Add("WIN32_LEAN_AND_MEAN"); // Exclude rarely-used stuff from Windows headers
            conf.Defines.Add("_WINSOCKAPI_"); // Prevent inclusion of winsock.h in windows.h
        }

        [Configure(Platform.win64, DevEnv.VisualStudio)]
        public virtual void ConfigureWin64VS(Configuration conf, Target target) {
            // Disable macro redefinition warning which is caused by Winsock2.h not checking for _WINSOCKAPI_ existance before defining it
            conf.Options.Add(new Sharpmake.Options.Vc.Compiler.DisableSpecificWarnings("4005"));

            conf.Options.Add(Sharpmake.Options.Vc.General.WarningLevel.Level3);
            conf.Options.Add(Sharpmake.Options.Vc.General.TreatWarningsAsErrors.Enable);
            conf.Options.Add(Sharpmake.Options.Vc.General.DebugInformation.ProgramDatabase);
            conf.Options.Add(Sharpmake.Options.Vc.General.CharacterSet.MultiByte);
            conf.Options.Add(Sharpmake.Options.Vc.General.WindowsTargetPlatformVersion.v10_0_17763_0);
            conf.Options.Add(Sharpmake.Options.Vc.General.PreferredToolArchitecture.x64);
            conf.Options.Add(Sharpmake.Options.Vc.Compiler.MultiProcessorCompilation.Enable);

            conf.Options.Add(Sharpmake.Options.Vc.Compiler.FloatingPointModel.Precise);
            conf.Options.Add(Sharpmake.Options.Vc.Compiler.FloatingPointExceptions.Enable);
            conf.Options.Add(Sharpmake.Options.Vc.Compiler.RTTI.Disable);
            conf.Options.Add(Sharpmake.Options.Vc.Compiler.Inline.Default);
            conf.Options.Add(Sharpmake.Options.Vc.Linker.SubSystem.Application);

            conf.Options.Add(Sharpmake.Options.Vc.Linker.GenerateDebugInformation.Enable);
            conf.Options.Add(Sharpmake.Options.Vc.Linker.GenerateFullProgramDatabaseFile.Enable);

            if (target.Optimization == Optimization.Debug) {
                conf.Options.Add(Sharpmake.Options.Vc.Compiler.Optimization.Disable);
                conf.Options.Add(Sharpmake.Options.Vc.Compiler.StringPooling.Disable);
                conf.Options.Add(Sharpmake.Options.Vc.Compiler.RuntimeChecks.Both);
            } else {
                conf.Options.Add(Sharpmake.Options.Vc.Compiler.Optimization.MaximizeSpeed);
                conf.Options.Add(Sharpmake.Options.Vc.Compiler.StringPooling.Enable);
                conf.Options.Add(Sharpmake.Options.Vc.Compiler.RuntimeChecks.Default);
            }

            if (target.Optimization == Optimization.Debug) {
                conf.Options.Add(Sharpmake.Options.Vc.Compiler.RuntimeLibrary.MultiThreadedDebugDLL);
            } else {
                conf.Options.Add(Sharpmake.Options.Vc.Compiler.RuntimeLibrary.MultiThreadedDLL);
            }
        }

        [Configure(Platform.win64, DevEnv.make)]
        public virtual void ConfigureWin64Make(Configuration conf, Target target) {
            conf.Options.Add(Sharpmake.Options.Makefile.General.PlatformToolset.Clang);
            conf.Options.Add(Sharpmake.Options.Makefile.Compiler.CppLanguageStandard.Cpp17);
            conf.Options.Add(Sharpmake.Options.Makefile.Compiler.Exceptions.Enable);
            conf.Options.Add(Sharpmake.Options.Makefile.Compiler.ExtraWarnings.Disable);
            conf.Options.Add(Sharpmake.Options.Makefile.Compiler.Rtti.Enable);
            conf.Options.Add(Sharpmake.Options.Makefile.Compiler.TreatWarningsAsErrors.Disable);
            conf.Options.Add(Sharpmake.Options.Makefile.Compiler.Warnings.Disable);

            if (target.Optimization == Optimization.Debug) {
                conf.Options.Add(Sharpmake.Options.Makefile.Compiler.GenerateDebugInformation.Enable);
                conf.Options.Add(Sharpmake.Options.Makefile.Compiler.OptimizationLevel.Disable);
            } else {
                conf.Options.Add(Sharpmake.Options.Makefile.Compiler.GenerateDebugInformation.Disable);
                conf.Options.Add(Sharpmake.Options.Makefile.Compiler.OptimizationLevel.FullWithInlining);
            }
        }
    }
}