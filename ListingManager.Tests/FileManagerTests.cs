using Xunit;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EssentialCSharp.ListingManager.Tests;

public class FileManagerTests : TempFileTestBase
{
    [Theory]
    [InlineData("Chapter01", 01)]
    [InlineData("asdasdsad/asd/asd/asd/Chapter42", 42)]
    public void GetFolderChapterNumber(string chapterFilePath, int expectedChapterNum)
    {
        Assert.Equal(expectedChapterNum, FileManager.GetFolderChapterNumber(chapterFilePath));
    }
    [Theory]
    [InlineData("Chapter01", 01)]
    [InlineData("asdasdsad/asd/asd/asd/Chapter42", 42)]
    public void GetFolderChapterNumber_VerifyMatchesListingInformationLogic(string chapterFilePath, int expectedChapterNum)
    {
        List<string> filesToMake = new()
        {
            Path.Combine(chapterFilePath,$"Listing{expectedChapterNum:D2}.01.cs"),
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
        CreateTempDirectory(tempDir, name: chapterFilePath);
        var writtenFiles = WriteFiles(tempDir, filesToMake, toWrite);
        Assert.Single(writtenFiles);

        Assert.Equal(expectedChapterNum, (new ListingInformation(writtenFiles.First().FullName)).OriginalChapterNumber);
    }
}