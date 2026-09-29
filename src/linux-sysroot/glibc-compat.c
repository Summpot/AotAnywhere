#define _GNU_SOURCE
#include <stdlib.h>
#include <stdio.h>
#include <stdarg.h>
#include <pthread.h>
#include <dlfcn.h>

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

static int (*__real_pthread_attr_setstacksize)(pthread_attr_t *, size_t) = NULL;

int pthread_attr_setstacksize(pthread_attr_t *attr, size_t stacksize) {
    if (!__real_pthread_attr_setstacksize) {
        __real_pthread_attr_setstacksize = (int (*)(pthread_attr_t *, size_t))dlsym(RTLD_NEXT, "pthread_attr_setstacksize");
    }
    /* .NET GateThread requests 256KB (262144 bytes).
     * When native libraries with Thread-Local Storage (TLS) like Skia, HarfBuzz,
     * and Rust UniFFI are loaded, glibc static TLS overhead exceeds 256KB (e.g. 268KB+).
     * In this situation, pthread_create rejects the thread with EINVAL (22),
     * which .NET surfaces as an OutOfMemoryException terminating the process.
     * Ensure any small stack size (< 1MB) is clamped to at least 1MB.
     */
    if (stacksize < 1024 * 1024) {
        stacksize = 1024 * 1024;
    }
    if (__real_pthread_attr_setstacksize) {
        return __real_pthread_attr_setstacksize(attr, stacksize);
    }
    return 0;
}
