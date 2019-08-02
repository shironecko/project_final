#include <stdio.h>

void printTypeNames() {
    @foreach (Type type in @Model.Types) {
        @:printf("@type.Name");
    }
}