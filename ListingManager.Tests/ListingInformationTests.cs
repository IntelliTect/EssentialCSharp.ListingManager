using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EssentialCSharp.ListingManager.Tests;

[TestClass]
public class ListingInformationTests : TempFileTestBase
{
    [TestMethod]
    [DataRow("Listing01.01.cs", 1, 1, null, null)]
    [DataRow("Listing01.02A.cs", 1, 2, "A", null)]
    [DataRow("Listing01.02.something.cs", 1, 2, null, "something")]
    [DataRow("Listing05.04.Something.cs", 5, 4, null, "Something")]
    [DataRow("Listing09.13.Some.Parse.cs", 9, 13, null, "Some.Parse")]
    public void Constructor_GivenValidListings_PropertiesPopulatedSuccessfully(string listing,
        int chapterNumber, int listingNumber, string suffix, string description)
    {
        List<string> filesToMake = new()
        {
            listing
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
        WriteFiles(tempDir, filesToMake, toWrite);
        var writtenFiles = WriteFiles(tempDir, filesToMake, toWrite);
        Xunit.Assert.Single(writtenFiles);

        ListingInformation listingInformation = new(writtenFiles.First().FullName);

        Assert.AreEqual(chapterNumber, listingInformation.OriginalChapterNumber);
        Assert.AreEqual(listingNumber, listingInformation.OriginalListingNumber);

        Assert.AreEqual(suffix ?? "", listingInformation.OriginalListingNumberSuffix);

        Assert.AreEqual(description ?? "", listingInformation.ListingDescription);
    }

    [TestMethod]
    [DataRow("Listing01.01.cs")]
    [DataRow("Listing05.04.Something.xml")]
    [DataRow("Listing05.04.Something.XML")]
    public void Constructor_GivenValidListingFileTypes_CreatesNewListingInformation(string listing)
    {
        List<string> filesToMake = new()
        {
            listing
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
        WriteFiles(tempDir, filesToMake, toWrite);
        var writtenFiles = WriteFiles(tempDir, filesToMake, toWrite);
        Xunit.Assert.Single(writtenFiles);

        ListingInformation listingInformation = new(writtenFiles.First().FullName);
        Assert.IsNotNull(listingInformation);
        Assert.AreEqual(Path.GetExtension(listing), listingInformation.ListingExtension);
        Assert.IsFalse(listingInformation.Changed);
    }    [TestMethod]
    [DataRow("Listing01.01.cs", false, false)]
    [DataRow("Listing01.01.Something.Tests.cs", false, true)]
    [DataRow("Listing01.01.Tests.cs", false, true)]
    [DataRow("Listing01.01.Tests.cs", true, true)]
    [DataRow("Listing05.04.Something.xml", false, false)]
    [DataRow("Listing05.04.Something.XML", false, false)]
    public void Constructor_GivenACodeFileOrTest_CorrectlyIdentifiesTestOrNot(string listing, bool isTest, bool expectedIsTestResult)
    {
        List<string> filesToMake = new()
        {
            listing
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
        WriteFiles(tempDir, filesToMake, toWrite);
        var writtenFiles = WriteFiles(tempDir, filesToMake, toWrite);
        Xunit.Assert.Single(writtenFiles);

        ListingInformation listingInformation = new(writtenFiles.First().FullName, isTest);
        Assert.IsNotNull(listingInformation);
        Assert.AreEqual(Path.GetExtension(listing), listingInformation.ListingExtension);

        Assert.AreEqual(expectedIsTestResult, listingInformation.IsTest);
    }
    [TestMethod]
    [DataRow("Listing01.02.something.txt")]
    [DataRow("Listing01.02A.csproj")]
    public void Constructor_GivenInvalidListingFileTypes_ThrowsArgumentException(string listing)
    {
        List<string> filesToMake = new()
        {
            listing
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
        WriteFiles(tempDir, filesToMake, toWrite);
        var writtenFiles = WriteFiles(tempDir, filesToMake, toWrite);
        Xunit.Assert.Single(writtenFiles);

        Assert.ThrowsException<System.ArgumentException>(() => new ListingInformation(writtenFiles.First().FullName));
    }
    [TestMethod]
    [DataRow("01", "Listing01.01.cs")]
    [DataRow("03", "Listing01.03.cs")]
    [DataRow("05a", "Listing01.05a.cs")]
    [DataRow("07A", "Listing01.07A.cs")]
    [DataRow("10B", "Listing01.10B.cs")]
    [DataRow("13b", "Listing01.13b.cs")]
    [DataRow("24", "Listing01.24.cs")]
    public void GetPaddedListingNumber_ListingNumber_ReturnCorrectPaddedListingNumber(string expected, string listingPath)
    {
        List<string> filesToMake = new()
        {
            listingPath
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
        WriteFiles(tempDir, filesToMake, toWrite);
        var writtenFiles = WriteFiles(tempDir, filesToMake, toWrite);
        Xunit.Assert.Single(writtenFiles);

        Assert.AreEqual(expected, (new ListingInformation(writtenFiles.First().FullName)).GetPaddedListingNumberWithSuffix());
    }

    [TestMethod]
    [DataRow("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_01", "Listing01.01.cs", false, false)]
    [DataRow("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_05", "Listing01.05.cs", false, false)]
    [DataRow("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_04a", "Listing01.04a.cs", false, false)]
    [DataRow("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_04a", "Listing01.04a.cs", true, false)]
    [DataRow("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_06B", "Listing01.06B.cs", false, false)]
    [DataRow("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_07C", "Listing01.07C.cs", true, false)]
    [DataRow("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_07C.Tests", "Listing01.07C.Tests.cs", true, true)]
    [DataRow("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_08.Tests", "Listing01.08.Tests.cs", true, true)]
    [DataRow("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_10.Tests", "Listing01.10.Tests.cs", true, true)]
    public void GetNewNamespace_Namespace_ReturnCorrectNewNamespace(string expected, string listingPath, bool chapterOnly, bool isTest)
    {
        List<string> filesToMake = new()
        {
            listingPath
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
        WriteFiles(tempDir, filesToMake, toWrite);
        var writtenFiles = WriteFiles(tempDir, filesToMake, toWrite);
        Xunit.Assert.Single(writtenFiles);

        Assert.AreEqual(expected, new ListingInformation(writtenFiles.First().FullName, isTest).GetNewNamespace(chapterOnly));
    }

    [TestMethod]
    [DataRow("Listing01.01.cs", "Listing01.01.cs", false, false)]
    public void GetNewFileName_FileName_ReturnCorrectNewFileName(string expected, string listingPath, bool chapterOnly, bool isTest)
    {
        List<string> filesToMake = new()
        {
            listingPath
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
        WriteFiles(tempDir, filesToMake, toWrite);
        var writtenFiles = WriteFiles(tempDir, filesToMake, toWrite);
        Xunit.Assert.Single(writtenFiles);

        Assert.AreEqual(expected, new ListingInformation(writtenFiles.First().FullName, isTest).GetNewFileName(chapterOnly));
    }
}