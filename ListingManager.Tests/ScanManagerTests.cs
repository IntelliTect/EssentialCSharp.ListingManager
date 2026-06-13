using Xunit;

namespace EssentialCSharp.ListingManager.Tests;

[Collection(ConsoleOutputCollection.Name)]
public class ScanManagerTests : TempFileTestBase
{
    [Fact]
    public void ScanForMissingTests()
    {
        List<string> filesToMake = new()
        {
            Path.Join("Chapter01","Listing01.01.SpecifyingLiteralValues.cs"),
            Path.Join("Chapter01","Listing01.03.cs"),
            Path.Join("Chapter01.Tests","Listing01.03.Tests.cs"),
            Path.Join("Chapter02","Listing02.04.cs"),
            Path.Join("Chapter02","Listing02.06.Something.cs"),
            Path.Join("Chapter02.Tests","Listing02.06.Something.Tests.cs")
        };

        List<string> expected = new()
        {
            "Missing test for 1.1",
            "Missing test for 2.4"
        };

        List<string> toWrite = 
        [
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        ];
        DirectoryInfo tempDir = CreateTempDirectory();
        CreateTempDirectory(tempDir, "Chapter01");
        CreateTempDirectory(tempDir, "Chapter01.Tests");
        CreateTempDirectory(tempDir, "Chapter02");
        CreateTempDirectory(tempDir, "Chapter02.Tests");
        WriteFiles(tempDir, filesToMake, toWrite);

        TextWriter originalOut = Console.Out;
        using StringWriter output = new();
        try
        {
            Console.SetOut(output);
            ScanManager.ScanForAllMissingTests(tempDir, false);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        List<string> actual = output.ToString()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .OrderBy(line => line, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(expected.OrderBy(line => line, StringComparer.Ordinal), actual);
    }
}
