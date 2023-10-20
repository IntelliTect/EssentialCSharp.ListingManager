using Xunit;

namespace EssentialCSharp.ListingManager.Tests;

public class ListingInformationTests : TempFileTestBase
{
    [Theory]
    [InlineData("Listing01.01.cs", 1, 1, "", "")]
    [InlineData("Listing01.02A.cs", 1, 2, "A", "")]
    [InlineData("Listing01.02.something.cs", 1, 2, "", "something")]
    [InlineData("Listing05.04.Something.cs", 5, 4, "", "Something")]
    [InlineData("Listing09.13.Some.Parse.cs", 9, 13, "", "Some.Parse")]
    public void Constructor_GivenValidListings_PropertiesPopulatedSuccessfully(string listing,
        int chapterNumber, int listingNumber, string suffix, string caption)
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
        var writtenFile = Assert.Single(writtenFiles);

        ListingInformation listingInformation = new(writtenFile.FullName);

        Assert.Equal(chapterNumber, listingInformation.OriginalChapterNumber);
        Assert.Equal(listingInformation.NewChapterNumber, listingInformation.OriginalChapterNumber);

        Assert.Equal(listingNumber, listingInformation.OriginalListingNumber);
        Assert.Equal(listingInformation.NewListingNumber, listingInformation.OriginalListingNumber);

        Assert.Equal(suffix, listingInformation.OriginalListingNumberSuffix);
        Assert.Equal(listingInformation.NewListingNumberSuffix, listingInformation.OriginalListingNumberSuffix);

        Assert.Equal(caption, listingInformation.Caption);

        Assert.False(listingInformation.Changed);
    }

    [Theory]
    [InlineData("Listing01.01.cs")]
    [InlineData("Listing05.04.Something.xml")]
    [InlineData("Listing05.04.Something.XML")]
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
        var writtenFile = Assert.Single(writtenFiles);

        ListingInformation listingInformation = new(writtenFile.FullName);

        Assert.NotNull(listingInformation);

        Assert.Equal(Path.GetExtension(listing), listingInformation.ListingExtension);
        Assert.False(listingInformation.Changed);
    }

    [Theory]
    [InlineData("Listing01.01.cs", false, false)]
    [InlineData("Listing01.01.Something.Tests.cs", false, true)]
    [InlineData("Listing01.01.Tests.cs", false, true)]
    [InlineData("Listing01.01.Tests.cs", true, true)]
    [InlineData("Listing05.04.Something.xml", false, false)]
    [InlineData("Listing05.04.Something.XML", false, false)]
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
        var writtenFile = Assert.Single(writtenFiles);

        ListingInformation listingInformation = new(writtenFile.FullName, isTest);
        Assert.NotNull(listingInformation);
        Assert.Equal(Path.GetExtension(listing), listingInformation.ListingExtension);

        Assert.Equal(expectedIsTestResult, listingInformation.IsTest);
    }

    [Theory]
    [InlineData("Listing01.02.something.txt")]
    [InlineData("Listing01.02A.csproj")]
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
        var writtenFiles = WriteFiles(tempDir, filesToMake, toWrite);
        var writtenFile = Assert.Single(writtenFiles);
        Assert.Throws<ArgumentException>(() => new ListingInformation(writtenFile.FullName));
    }

    [Theory]
    [InlineData("01", "Listing01.01.cs")]
    [InlineData("03", "Listing01.03.cs")]
    [InlineData("05a", "Listing01.05a.cs")]
    [InlineData("07A", "Listing01.07A.cs")]
    [InlineData("10B", "Listing01.10B.cs")]
    [InlineData("13b", "Listing01.13b.cs")]
    [InlineData("24", "Listing01.24.cs")]
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
        var writtenFiles = WriteFiles(tempDir, filesToMake, toWrite);
        var writtenFile = Assert.Single(writtenFiles);
        Assert.Equal(expected, (new ListingInformation(writtenFile.FullName)).GetPaddedListingNumberWithSuffix());
    }

    [Theory]
    [InlineData("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_01", "Listing01.01.cs", false, false)]
    [InlineData("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_05", "Listing01.05.cs", false, false)]
    [InlineData("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_04a", "Listing01.04a.cs", false, false)]
    [InlineData("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_04a", "Listing01.04a.cs", true, false)]
    [InlineData("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_06B", "Listing01.06B.cs", false, false)]
    [InlineData("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_07C", "Listing01.07C.cs", true, false)]
    [InlineData("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_07C.Tests", "Listing01.07C.Tests.cs", true, true)]
    [InlineData("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_08.Tests", "Listing01.08.Tests.cs", true, true)]
    [InlineData("AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing01_10.Tests", "Listing01.10.Tests.cs", true, true)]
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
        var writtenFiles = WriteFiles(tempDir, filesToMake, toWrite);
        var writtenFile = Assert.Single(writtenFiles);

        Assert.Equal(expected, new ListingInformation(writtenFile.FullName, isTest).GetNewNamespace(chapterOnly));
    }

    [Theory]
    [InlineData("Listing01.01.cs", "Listing01.01.cs", false, false)]
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
        var writtenFiles = WriteFiles(tempDir, filesToMake, toWrite);
        var writtenFile = Assert.Single(writtenFiles);

        Assert.Equal(expected, new ListingInformation(writtenFile.FullName, isTest).GetNewFileName(chapterOnly));
    }
}