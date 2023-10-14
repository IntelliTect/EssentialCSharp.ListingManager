using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EssentialCSharp.ListingManager.Tests;

[TestClass]
public class FileManagerTests : TempFileTestBase
{
    [TestMethod]
    [DataRow("Chapter01", 01)]
    [DataRow("asdasdsad/asd/asd/asd/Chapter42", 42)]
    public void GetFolderChapterNumber(string chapterFilePath, int expectedChapterNum)
    {
        Assert.AreEqual(expectedChapterNum, FileManager.GetFolderChapterNumber(chapterFilePath));
    }
    [TestMethod]
    [DataRow("Chapter01", 01)]
    [DataRow("asdasdsad/asd/asd/asd/Chapter42", 42)]
    public void GetFolderChapterNumber_VerifyMatchesListingInformationLogic(string chapterFilePath, int expectedChapterNum)
    {
        List<string> filesToMake = new()
        {
            Path.Combine(chapterFilePath,$"Listing{expectedChapterNum.ToString("D2")}.01.cs"),
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
        Xunit.Assert.Single(writtenFiles);

        Xunit.Assert.Equal(expectedChapterNum, (new ListingInformation(writtenFiles.First().FullName)).OriginalChapterNumber);
    }
}