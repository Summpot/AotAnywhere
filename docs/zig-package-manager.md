# Zig Package Manager & Static Library Integration

AotAnywhere supports executing `zig build` before the NativeAOT linking step. This allows using the **Zig Package Manager** (`build.zig.zon`) and `build.zig` to fetch, cross-compile, and statically link third-party C/C++ or Zig libraries (such as `freetype`, `fontconfig`, `expat`, etc.) directly into your Native AOT binary.

## Key Features

1. **Configurable GLIBC Version**: Target older Linux distributions (e.g. Ubuntu 20.04) via `<GlibcVersion>2.31</GlibcVersion>`.
2. **Automatic Zig Build Execution**: Automatically invokes `zig build` with the exact target triple (e.g. `x86_64-linux-gnu.2.31`) and optimization mode matching your project configuration.
3. **Automatic Library Registration**: Discovers all static libraries (`.a` / `.lib`) produced by `zig build` in the output directory and registers them directly into `@(NativeLibrary)` for static linking.
4. **Automatic Search Path Injection**: Appends `-L<output>/lib` into `@(LinkerArg)` so references to `-l<name>` resolve seamlessly.

## Configuration Options

Add these properties to your `.csproj`:

| Property | Default | Description |
|---|---|---|
| `<GlibcVersion>` | `(empty)` | Target GLIBC version for `linux-gnu` targets (e.g. `2.31` or `2.28`). Appends `.<version>` to the target triple. |
| `<ZigBuildProject>` | Auto-detected | Path to `build.zig` or a directory containing `build.zig`. Auto-searches `./build.zig`, `./native/build.zig`, `./zig/build.zig`. |
| `<ZigBuildEnabled>` | Auto | Set to `true` to enable or `false` to disable. Defaults to `true` if `ZigBuildProject` exists. |
| `<ZigBuildOptimize>` | `ReleaseFast` / `Debug` | Optimization level passed as `-Doptimize=` (`ReleaseFast`, `ReleaseSmall`, `ReleaseSafe`, `Debug`). |
| `<ZigBuildTarget>` | `$(TargetTriple)` | Target triple passed to `zig build -Dtarget=`. |
| `<ZigBuildOutputDir>` | `$(IntermediateOutputPath)zig-out` | Destination output directory passed as `--prefix`. |
| `<ZigAutoLinkLibraries>` | `true` | When `true`, automatically adds all `.a` / `.lib` files from `$(ZigBuildOutputDir)/lib` to `@(NativeLibrary)`. |
| `<ZigBuildArgs>` | `(empty)` | Extra command-line arguments passed to `zig build`. |

## Quick Example

### 1. In your `.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <Sdk Name="StuDev.AotAnywhere" Version="1.0.5" />

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <PublishAot>true</PublishAot>

    <!-- Target GLIBC 2.31 (Ubuntu 20.04+) -->
    <GlibcVersion>2.31</GlibcVersion>

    <!-- Point to your Zig build file -->
    <ZigBuildProject>native/build.zig</ZigBuildProject>
  </PropertyGroup>
</Project>
```

### 2. In `native/build.zig.zon`:

```zig
.{
    .name = .native_deps,
    .version = "1.0.0",
    .fingerprint = 0x12345678,
    .dependencies = .{
        // Add your dependencies here, e.g. freetype, expat, fontconfig
    },
    .paths = .{
        "build.zig",
        "build.zig.zon",
    },
}
```

### 3. In `native/build.zig`:

```zig
const std = @import("std");

pub fn build(b: *std.Build) void {
    const target = b.standardTargetOptions(.{});
    const optimize = b.standardOptimizeOption(.{});

    // Build your static libraries here using b.addStaticLibrary
    // or through b.dependency(...)
    //
    // For example:
    // const lib = b.addStaticLibrary(.{
    //     .name = "my_native_lib",
    //     .target = target,
    //     .optimize = optimize,
    // });
    // b.installArtifact(lib);
}
```

When you run `dotnet publish -r linux-x64`:
1. `AotAnywhere` computes the target triple: `x86_64-linux-gnu.2.31`.
2. It invokes `zig build` with `-Dtarget=x86_64-linux-gnu.2.31 -Doptimize=ReleaseFast`.
3. The resulting `.a` files are compiled with GLIBC 2.31 ABI compatibility.
4. NativeAOT links them into your final binary automatically.
