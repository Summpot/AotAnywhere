namespace AotAnywhere.MSBuild.Tests;

// OverwriteTargetTriple maps a RID to the zig target triple that is injected as
// --target=<triple> and drives every cross-compile. Pure RID logic, so these
// are host-independent.
public class TargetTripleTests
{
    [Test]
    [Arguments("linux-x64", "x86_64-linux-gnu")]
    [Arguments("linux-arm64", "aarch64-linux-gnu")]
    [Arguments("linux-arm", "arm-linux-gnueabihf")]        // armv7 hard-float ABI suffix
    [Arguments("linux-musl-x64", "x86_64-linux-musl")]
    [Arguments("linux-musl-arm", "arm-linux-musleabihf")]
    [Arguments("alpine-x64", "x86_64-linux-musl")]
    [Arguments("osx-x64", "x86_64-macos")]
    [Arguments("osx-arm64", "aarch64-macos")]
    [Arguments("win-x64", "x86_64-windows-gnu")]
    [Arguments("win-arm64", "aarch64-windows-gnu")]
    public async Task Triple(string rid, string expected)
    {
        var result = Harness.Run("OverwriteTargetTriple",
            new Dictionary<string, string> { ["RuntimeIdentifier"] = rid });

        await Assert.That(result.Success)
            .IsTrue().Because($"OverwriteTargetTriple failed: {result.ErrorText}");
        await Assert.That(result.Prop("TargetTriple")).IsEqualTo(expected);
    }

    // The injected triple lands in @(LinkerArg) so the link step actually
    // cross-targets.
    [Test]
    public async Task TripleIsAppendedToLinkerArgs()
    {
        var result = Harness.Run("OverwriteTargetTriple",
            new Dictionary<string, string> { ["RuntimeIdentifier"] = "linux-arm64" });

        await Assert.That(result.Items("LinkerArg").Any(a => a == "--target=aarch64-linux-gnu"))
            .IsTrue();
    }

    [Test]
    [Arguments("linux-x64", "2.31", "x86_64-linux-gnu.2.31")]
    [Arguments("linux-arm64", "2.28", "aarch64-linux-gnu.2.28")]
    [Arguments("linux-arm", "2.31", "arm-linux-gnueabihf.2.31")]
    public async Task GlibcVersionAppendsToTriple(string rid, string glibcVersion, string expected)
    {
        var result = Harness.Run("OverwriteTargetTriple",
            new Dictionary<string, string>
            {
                ["RuntimeIdentifier"] = rid,
                ["GlibcVersion"] = glibcVersion
            });

        await Assert.That(result.Success)
            .IsTrue().Because($"OverwriteTargetTriple failed: {result.ErrorText}");
        await Assert.That(result.Prop("TargetTriple")).IsEqualTo(expected);
        await Assert.That(result.Items("LinkerArg").Any(a => a == $"--target={expected}"))
            .IsTrue();
    }

    [Test]
    public async Task CustomTargetTripleIsPreserved()
    {
        var result = Harness.Run("OverwriteTargetTriple",
            new Dictionary<string, string>
            {
                ["RuntimeIdentifier"] = "linux-x64",
                ["TargetTriple"] = "custom-x86_64-linux-gnu"
            });

        await Assert.That(result.Success)
            .IsTrue().Because($"OverwriteTargetTriple failed: {result.ErrorText}");
        await Assert.That(result.Prop("TargetTriple")).IsEqualTo("custom-x86_64-linux-gnu");
    }
}
