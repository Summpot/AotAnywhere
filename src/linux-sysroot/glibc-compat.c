#include <stdlib.h>
#include <stdio.h>
#include <stdarg.h>

char __libc_single_threaded = 0;

unsigned long long __isoc23_strtoull(const char *nptr, char **endptr, int base) {
    return strtoull(nptr, endptr, base);
}

int __isoc23_sscanf(const char *s, const char *format, ...) {
    va_list args;
    va_start(args, format);
    int ret = vsscanf(s, format, args);
    va_end(args);
    return ret;
}

void _ZSt28__throw_bad_array_new_lengthv(void) {
    abort();
}

int _ZNSt19_Sp_make_shared_tag5_S_eqERKSt9type_info(const void* ti) {
    return 1;
}
