#include <stdio.h>

int main() {
/*[[[cog
import cog
from code_generation import Struct, generateStructDeclaration
struct = Struct("TestStruct", "")
generateStructDeclaration(struct, lambda str : cog.outl(str))

# fnames = ['Hello', 'COG', 'C++']
# for fn in fnames:
#     cog.outl("printf(\"%s \");" % fn)
# cog.outl("printf(\"\\n\");")
]]]*/
struct TestStruct {
};
//[[[end]]]

    printf("Hello, world!\n");
}