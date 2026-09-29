namespace AotAnywhere.MSBuild.Tests;

public class ZigBuildTests
{
    [Test]
    public async Task ZigBuildDisabledByDefaultWhenNoProjectFound()
    {
        var enabled = Harness.EvalProp(
            new Dictionary<string, string> { ["RuntimeIdentifier"] = "linux-x64" },
            "ZigBuildEnabled");

        await Assert.That(enabled).IsEqualTo("false");
    }

    [Test]
    public async Task ZigBuildEnabledWhenExplicitProjectProvided()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var enabled = Harness.EvalProp(
                new Dictionary<string, string>
                {
                    ["RuntimeIdentifier"] = "linux-x64",
                    ["ZigBuildProject"] = tempFile
                },
                "ZigBuildEnabled");

            await Assert.That(enabled).IsEqualTo("true");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Test]
    public async Task ZigBuildOptimizeDefaultsToReleaseFastInRelease()
    {
        var optimize = Harness.EvalProp(
            new Dictionary<string, string>
            {
                ["RuntimeIdentifier"] = "linux-x64",
                ["Configuration"] = "Release"
            },
            "ZigBuildOptimize");

        await Assert.That(optimize).IsEqualTo("ReleaseFast");
    }

    [Test]
    public async Task ZigBuildOptimizeDefaultsToDebugInDebug()
    {
        var optimize = Harness.EvalProp(
            new Dictionary<string, string>
            {
                ["RuntimeIdentifier"] = "linux-x64",
                ["Configuration"] = "Debug"
            },
            "ZigBuildOptimize");

        await Assert.That(optimize).IsEqualTo("Debug");
    }

    [Test]
    public async Task RegisterZigOutputsDiscoversLibrariesAndAppendsSearchPath()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var libDir = Path.Combine(tempDir, "lib");
        Directory.CreateDirectory(libDir);

        var aFile = Path.Combine(libDir, "libfontconfig.a");
        var libFile = Path.Combine(libDir, "freetype.lib");
        File.WriteAllText(aFile, "!<arch>\n");
        File.WriteAllText(libFile, "!<arch>\n");

        try
        {
            var result = Harness.Run("AotAnywhereRegisterZigOutputs",
                new Dictionary<string, string>
                {
                    ["RuntimeIdentifier"] = "linux-x64",
                    ["ZigBuildEnabled"] = "true",
                    ["ZigBuildOutputDir"] = tempDir,
                    ["ZigAutoLinkLibraries"] = "true"
                });

            await Assert.That(result.Success).IsTrue();

            var nativeLibraries = result.Items("NativeLibrary").ToList();
            await Assert.That(nativeLibraries.Any(l => l.EndsWith("libfontconfig.a"))).IsTrue();
            await Assert.That(nativeLibraries.Any(l => l.EndsWith("freetype.lib"))).IsTrue();

            var linkerArgs = result.Items("LinkerArg").ToList();
            var expectedLibDir = Path.GetFullPath(libDir);
            await Assert.That(linkerArgs.Any(arg => arg.StartsWith("-L") && (arg.Contains(expectedLibDir) || arg.Contains(expectedLibDir.Replace('\\', '/'))))).IsTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
