using Xunit;

namespace EssentialCSharp.ListingManager.Tests;

public class FileManagerTests : TempFileTestBase
{
    [Theory]
    [InlineData(new string[] { "Chapter01" }, 01)]
    [InlineData(new string[] { "asdasdsad","asd","asd","asd","Chapter42" }, 42)]
    public void GetFolderChapterNumber(string[] chapterFilePath, int expectedChapterNum)
    {
        Assert.Equal(expectedChapterNum, FileManager.GetFolderChapterNumber(Path.Combine(chapterFilePath)));
    }
    [Theory]
    [InlineData(new string[] { "Chapter01" }, 01)]
    [InlineData(new string[] { "asdasdsad", "asd", "asd", "asd", "Chapter42" }, 42)]
    public void GetFolderChapterNumber_VerifyMatchesListingInformationLogic(string[] chapterFilePath, int expectedChapterNum)
    {
        List<string> filesToMake = new()
        {
            Path.Combine(Path.Combine(chapterFilePath),$"Listing{expectedChapterNum:D2}.01.cs"),
        };

        IEnumerable<string> toWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter18.Listing18_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        };

        DirectoryInfo tempDir = CreateTempDirectory(new(Path.GetTempPath()));
        CreateTempDirectory(tempDir, name: Path.Combine(chapterFilePath));
        var writtenFiles = WriteFiles(tempDir, filesToMake, toWrite);
        var writtenFile = Assert.Single(writtenFiles);

        Assert.Equal(expectedChapterNum, (new ListingInformation(writtenFile.FullName)).OriginalChapterNumber);
    }
}