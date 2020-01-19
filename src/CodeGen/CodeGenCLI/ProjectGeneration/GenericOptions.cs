using System.Collections.Generic;

namespace CodeGenCLI.ProjectGeneration {
    public class GenericOptions {
        public enum Optimization {
            None,
            Speed,
            Size,
            Full,
        }

        public enum DebugInfo {
            Off,
            On,
        }

        public enum Exceptions {
            Off,
            On,
        }

        public enum RTTI {
            Off,
            On,
        }

        public enum CppStd {
            Cpp03,
            Cpp11,
            Cpp14,
            Cpp17,
            //Cpp20,
        }
    }
}