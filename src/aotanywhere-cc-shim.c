#include <process.h>
#include <stdlib.h>
#include <string.h>

int main(int argc, char* argv[]) {
    int has_target = 0;
    for (int i = 1; i < argc; i++) {
        if (strcmp(argv[i], "-target") == 0 || strncmp(argv[i], "--target", 8) == 0) {
            has_target = 1;
            break;
        }
    }
    const char* default_target = getenv("TARGET_TRIPLE");
    if (!default_target || !default_target[0]) {
        default_target = "x86_64-linux-gnu.2.28";
    }

    char** new_argv;
    if (has_target) {
        new_argv = (char**)malloc((argc + 2) * sizeof(char*));
        new_argv[0] = "zig";
        new_argv[1] = "cc";
        for (int i = 1; i < argc; i++) new_argv[i + 1] = argv[i];
        new_argv[argc + 1] = NULL;
    } else {
        new_argv = (char**)malloc((argc + 4) * sizeof(char*));
        new_argv[0] = "zig";
        new_argv[1] = "cc";
        new_argv[2] = "-target";
        new_argv[3] = (char*)default_target;
        for (int i = 1; i < argc; i++) new_argv[i + 3] = argv[i];
        new_argv[argc + 3] = NULL;
    }
    return _spawnvp(_P_WAIT, "zig", (const char* const*)new_argv);
}
