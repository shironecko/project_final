using System;
using System.Collections.Generic;
using CodeGenCLI.ProjectGeneration;
using CodeGenCLI.ProjectGeneration.Compilers;
using NUnit.Framework;

namespace Tests {
    public class Tests {
        private List<object> m_PossibleGenericOptionValues = new List<object>();

        [SetUp]
        public void Setup() {
            Type t = typeof(GenericOptions);
            foreach (Type nested in t.GetNestedTypes()) {
                Assert.IsTrue(nested.IsEnum, "Only enum-based generic options are supported by the test suite, please extend it!");
                foreach (string enumValueStr in nested.GetEnumNames()) {
                    object enumValue = Enum.Parse(nested, enumValueStr);
                    m_PossibleGenericOptionValues.Add(enumValue);
                }
            }
        }

        [Test]
        public void TestClang() {
            var clang = new Clang();
            foreach (object genericOption in m_PossibleGenericOptionValues) {
                Assert.DoesNotThrow(() => clang.GenericOptionToCommandLine(genericOption));
            }
        }
    }
}