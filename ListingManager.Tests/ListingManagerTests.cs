using LibGit2Sharp;
using Xunit;

namespace EssentialCSharp.ListingManager.Tests;

public class ListingManagerTests : TempFileTestBase
{
    // Create the committer's signature and commit
    private Signature Author { get; } = new("IntellitectTestingBot", "info@intellitect.com", DateTime.Now);

    #region IsExtraListing
    [Theory]
    [InlineData("Listing02.01.SpecifyingLiteralValues.cs", false)]
    [InlineData("Listing02.01A.SpecifyingLiteralValues.cs", true)]
    [InlineData("Listing02.01.cs", false)]
    public void IsIncorrectListingFromPath_FindsIncorrectListing(string fileName, bool expectedResult)
    {
        string path = Path.Combine(TempDirectory.ToString(), fileName);

        bool actualResult = ListingManager.IsExtraListing(path);

        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("/Chapter02.Tests", "Listing02.01A.SpecifyingLiteralValues.cs", false)]
    public void ListingsInTestDirectories_AreNotCountedAsExtraListings(string parentDirectory, string fileName,
        bool expectedResult)
    {
        var directory = CreateTempDirectory(name: parentDirectory);

        string path = Path.Combine(directory.FullName, fileName);

        bool actualResult = ListingManager.IsExtraListing(path);

        Assert.Equal(expectedResult, actualResult);
    }
    #endregion IsExtraListing

    [Fact]
    public void GetAllExtraListings_ExtraListingsReturned()
    {
        ICollection<string> filesToMake = new List<string>
        {
            @"Listing02.01B.Something.cs",
            @"Listing02.02.cs",
            @"Listing02.03C.cs"
        };

        DirectoryInfo tempDir = CreateTempDirectory(name: "Chapter02");
        ICollection<string> expectedFiles = filesToMake;
        expectedFiles.Remove(@"Listing02.02.cs");
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

        WriteFiles(tempDir, filesToMake, null);

        var extraListings = ListingManager.GetAllExtraListings(tempDir.FullName).ToList();

        Assert.Equal(expectedFiles, extraListings);
    }

    #region UpdateChapterListingNumbers
    #region GitStorageManager
    [Fact]
    public void UpdateChapterListingNumbers_GitStorageManagerByFolder_NamespacesUpdated()
    {
        ICollection<string> filesToMake = new List<string>
        {
            Path.Join("Chapter42","Listing18.06.cs"),
        };

        ICollection<string> expectedFiles = new List<string>
        {
            Path.Join("Chapter42","Listing42.01.cs")
        };

        IEnumerable<string> toWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter18.Listing18_06",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        };

        IEnumerable<string> expectedToWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter42.Listing42_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}",
        };
        DirectoryInfo tempDir = CreateTempDirectory();
        DirectoryInfo chapterDir = CreateTempDirectory(tempDir, name: "Chapter42");
        CreateTempDirectory(tempDir, name: "Chapter42.Tests");
        WriteFiles(tempDir, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

        Repository.Init(tempDir.FullName);
        using var repo = new Repository(tempDir.FullName);
        Commands.Stage(repo, "*");
        // Commit to the repository
        repo.Commit("Here's a commit i made!", Author, Author);

        ListingManager listingManager = new(tempDir, new GitStorageManager(tempDir.FullName));

        listingManager.UpdateChapterListingNumbers(chapterDir, byFolder: true);

        List<string> files = FileManager.GetAllFilesAtPath(tempDir.FullName, true)
            .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

        // Assert
        string expectedFile = Assert.Single(files);
        Assert.Equivalent(expectedFiles, files);

        Assert.Equal(string.Join(Environment.NewLine, expectedToWrite) + Environment.NewLine, File.ReadAllText(expectedFile));
    }

    [Fact]
    public void UpdateChapterListingNumbers_GitStorageManager_NamespacesUpdated()
    {
        ICollection<string> filesToMake = new List<string>
        {
            Path.Join("Chapter42","Listing42.06.cs"),
        };

        ICollection<string> expectedFiles = new List<string>
        {
            Path.Join("Chapter42","Listing42.01.cs")
        };

        IEnumerable<string> toWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter18.Listing18_06",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        };

        IEnumerable<string> expectedToWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter42.Listing42_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}",
        };
        DirectoryInfo tempDir = CreateTempDirectory();
        DirectoryInfo chapterDir = CreateTempDirectory(tempDir, name: "Chapter42");
        CreateTempDirectory(tempDir, name: "Chapter42.Tests");
        WriteFiles(tempDir, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

        Repository.Init(tempDir.FullName);
        using var repo = new Repository(tempDir.FullName);
        Commands.Stage(repo, "*");
        // Commit to the repository
        repo.Commit("Here's a commit i made!", Author, Author);

        ListingManager listingManager = new(tempDir, new GitStorageManager(tempDir.FullName));
        listingManager.UpdateChapterListingNumbers(chapterDir);

        List<string> files = FileManager.GetAllFilesAtPath(tempDir.FullName, true)
            .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

        // Assert
        string expectedFile = Assert.Single(files);
        Assert.Equivalent(expectedFiles, files);

        Assert.Equal(string.Join(Environment.NewLine, expectedToWrite) + Environment.NewLine, File.ReadAllText(expectedFile));
    }

    [Fact]
    public void UpdateChapterListingNumbers_GitStorageManager_ListingsWithinListMissing_ListingsRenumbered()
    {
        List<string> filesToMake = new()
        {
            "Listing01.01.SpecifyingLiteralValues.cs",
            "Listing01.02.cs",
            "Listing01.04.cs",
            "Listing01.06.Something.cs"
        };

        List<string> expectedFiles = new()
        {
            "Listing01.01.SpecifyingLiteralValues.cs",
            "Listing01.02.cs",
            "Listing01.03.cs",
            "Listing01.04.Something.cs"
        };

        List<string> toWrite = new()
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter18.Listing18_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        };
        WriteFiles(TempDirectory, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, null).ToList();

        string rootedPath = Repository.Init(TempDirectory.FullName);
        using var repo = new Repository(TempDirectory.FullName);

        Commands.Stage(repo, "*");

        // Commit to the repository
        repo.Commit("Here's a commit i made!", Author, Author);

        ListingManager listingManager = new(TempDirectory, new GitStorageManager(TempDirectory.FullName));
        listingManager.UpdateChapterListingNumbers(TempDirectory, singleDir: true);
        List<string> files = Directory.EnumerateFiles(TempDirectory.FullName)
            .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();
        Assert.Equal(expectedFiles, files);

        Commands.Stage(repo, "*");
        repo.RetrieveStatus();
        Assert.Equal(FileStatus.ModifiedInIndex, repo.RetrieveStatus(files[0]));
        Assert.Equal(FileStatus.ModifiedInIndex, repo.RetrieveStatus(files[1]));

        //TODO: These ideally would be "FileStatus.RenamedInIndex" instead of 
        //NewInIndex because this indicates the old file is just being removed
        //and the new one added instead of a true rename
        Assert.Equal(FileStatus.NewInIndex, repo.RetrieveStatus(files[2]));
        Assert.Equal(FileStatus.NewInIndex, repo.RetrieveStatus(files[3]));
    }
    #endregion GitStorageManager
    #region UsingOSStorageManager
    [Fact]
    public void UpdateChapterListingNumbers_ListingsWithinListMissing_ListingsRenumbered()
    {
        List<string> filesToMake = new()
        {
            "Listing01.01.SpecifyingLiteralValues.cs",
            "Listing01.02.cs",
            "Listing01.04.cs",
            "Listing01.06.Something.cs"
        };

        List<string> expectedFiles = new()
        {
            "Listing01.01.SpecifyingLiteralValues.cs",
            "Listing01.02.cs",
            "Listing01.03.cs",
            "Listing01.04.Something.cs"
        };

        List<string> toWrite = new()
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter18.Listing18_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        };
        WriteFiles(TempDirectory, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, null).ToList();

        ListingManager listingManager = new(TempDirectory, new OSStorageManager());
        listingManager.UpdateChapterListingNumbers(TempDirectory, singleDir: true);

        List<string> files = Directory.EnumerateFiles(TempDirectory.FullName)
            .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();
        Assert.Equal(expectedFiles, files);
    }

    [Fact]
    public void UpdateChapterListingNumbers_ListingAtBeginningOfListMissing_ListingsRenumbered()
    {
        ICollection<string> filesToMake = new List<string>
        {
            "Listing01.02.cs",
            "Listing01.04A.Something.cs",
            "Listing01.06.Something.cs"
        };

        ICollection<string> expectedFiles = new List<string>
        {
            "Listing01.01.cs",
            "Listing01.02.Something.cs",
            "Listing01.03.Something.cs"
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
        WriteFiles(TempDirectory, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, null).ToList();

        ListingManager listingManager = new(TempDirectory, new OSStorageManager());
        listingManager.UpdateChapterListingNumbers(TempDirectory, singleDir: true);

        List<string> files = Directory.EnumerateFiles(TempDirectory.FullName)
            .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

        Assert.Equal(expectedFiles, files);
    }

    [Fact]
    public void UpdateChapterListingNumbers_MultipleListingsMissing_ListingsRenumbered()
    {
        ICollection<string> filesToMake = new List<string>
        {
            "Listing09.01.DeclaringAStruct.cs",
            "Listing09.02.ErroneousInitialization.cs",
            "Listing09.03.AccessError.cs",
            "Listing09.05.SubtleBoxAndUnboxInstructions.cs",
            "Listing09.06.UnboxMustBeSameType.cs",
            "Listing09.07.SubtleBoxingIdiosyncrasies.cs",
            "Listing09.08.AvoidingUnboxingAndCopying.cs",
            "Listing09.09.ComparingAnIntegerSwitchToAnEnumSwitch.cs",
            "Listing09.10.DefiningAnEnum.cs",
            "Listing09.11.DefiningAnEnumType.cs",
            "Listing09.12.CastingBetweenArraysOfEnums.cs",
            "Listing09.13.ConvertingAStringToAnEnumUsingEnum.Parse.cs",
            "Listing09.14.ConvertingAStringToAnEnumUsingEnum.TryParse.cs",
            "Listing09.15.UsingEnumsAsFlags.cs",
            "Listing09.16.UsingBitwiseORandANDWithFlagEnums.cs",
            "Listing09.17.DefiningEnumValuesforFrequentCombinations.cs",
            "Listing09.18.UsingFlagsAttribute.cs"
        };

        ICollection<string> expectedFiles = new List<string>
        {
            "Listing09.01.DeclaringAStruct.cs",
            "Listing09.02.ErroneousInitialization.cs",
            "Listing09.03.AccessError.cs",
            "Listing09.04.SubtleBoxAndUnboxInstructions.cs",
            "Listing09.05.UnboxMustBeSameType.cs",
            "Listing09.06.SubtleBoxingIdiosyncrasies.cs",
            "Listing09.07.AvoidingUnboxingAndCopying.cs",
            "Listing09.08.ComparingAnIntegerSwitchToAnEnumSwitch.cs",
            "Listing09.09.DefiningAnEnum.cs",
            "Listing09.10.DefiningAnEnumType.cs",
            "Listing09.11.CastingBetweenArraysOfEnums.cs",
            "Listing09.12.ConvertingAStringToAnEnumUsingEnum.Parse.cs",
            "Listing09.13.ConvertingAStringToAnEnumUsingEnum.TryParse.cs",
            "Listing09.14.UsingEnumsAsFlags.cs",
            "Listing09.15.UsingBitwiseORandANDWithFlagEnums.cs",
            "Listing09.16.DefiningEnumValuesforFrequentCombinations.cs",
            "Listing09.17.UsingFlagsAttribute.cs"
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
        WriteFiles(TempDirectory, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, null).ToList();

        ListingManager listingManager = new(TempDirectory, new OSStorageManager());
        listingManager.UpdateChapterListingNumbers(TempDirectory, singleDir: true);

        List<string> files = Directory.EnumerateFiles(TempDirectory.FullName)
            .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

        Assert.Equal(expectedFiles, files);
    }

    [Fact]
    public void UpdateChapterListingNumbers_AdditionalListings_ListingsRenumbered()
    {
        ICollection<string> filesToMake = new List<string>
        {
            "Listing01.01.cs",
            "Listing01.01A.Some.cs",
            "Listing01.01B.cs",
            "Listing01.01C.cs",
            "Listing01.02.cs",
            "Listing01.02A.Test.cs"
        };

        ICollection<string> expectedFiles = new List<string>
        {
            "Listing01.01.cs",
            "Listing01.02.Some.cs",
            "Listing01.03.cs",
            "Listing01.04.cs",
            "Listing01.05.cs",
            "Listing01.06.Test.cs"
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
        WriteFiles(TempDirectory, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, null).ToList();

        ListingManager listingManager = new(TempDirectory, new OSStorageManager());
        listingManager.UpdateChapterListingNumbers(TempDirectory, singleDir: true);

        List<string> files = Directory.EnumerateFiles(TempDirectory.FullName)
            .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

        Assert.Equal(expectedFiles, files);
    }

    [Fact]
    public void UpdateChapterListingNumbers_UnitTestsAlsoUpdated_ListingsAndTestsUpdated()
    {
        ICollection<string> filesToMake = new List<string>
        {
            "Chapter01/Listing01.01.cs",
            "Chapter01/Listing01.01A.Some.cs",
            "Chapter01/Listing01.01B.cs",
            "Chapter01/Listing01.01C.cs",
            "Chapter01/Listing01.05.cs",
            "Chapter01.Tests/Listing01.01.Tests.cs",
            "Chapter01.Tests/Listing01.01A.Some.Tests.cs",
            "Chapter01.Tests/Listing01.01B.Tests.cs",
            "Chapter01.Tests/Listing01.01C.Tests.cs",
            "Chapter01.Tests/Listing01.05.Tests.cs"
        };

        ICollection<string> expectedFiles = new List<string>
        {
            @"Chapter01\Listing01.01.cs",
            @"Chapter01\Listing01.02.Some.cs",
            @"Chapter01\Listing01.03.cs",
            @"Chapter01\Listing01.04.cs",
            @"Chapter01\Listing01.05.cs",
            @"Chapter01.Tests\Listing01.01.Tests.cs",
            @"Chapter01.Tests\Listing01.02.Some.Tests.cs",
            @"Chapter01.Tests\Listing01.03.Tests.cs",
            @"Chapter01.Tests\Listing01.04.Tests.cs",
            @"Chapter01.Tests\Listing01.05.Tests.cs"
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
        DirectoryInfo tempDir = CreateTempDirectory();
        DirectoryInfo chapterDir = CreateTempDirectory(tempDir, name: "Chapter01");
        CreateTempDirectory(tempDir, name: "Chapter01.Tests");
        WriteFiles(tempDir, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

        ListingManager listingManager = new(TempDirectory, new OSStorageManager());
        listingManager.UpdateChapterListingNumbers(chapterDir);

        List<string> files = FileManager.GetAllFilesAtPath(tempDir.FullName, true)
            .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

        Assert.Equivalent(expectedFiles, files);
    }

    [Fact]
    public void
        UpdateChapterListingNumbersUsingChapterNumberFromFolder_UnitTestsAlsoUpdated_ListingsAndTestsUpdated()
    {
        ICollection<string> filesToMake = new List<string>
        {
            "Chapter42/Listing01.01.cs",
            "Chapter42/Listing01.01A.Some.cs",
            "Chapter42/Listing01.01B.cs",
            "Chapter42/Listing01.01C.cs",
            "Chapter42/Listing01.05.cs",
            "Chapter42.Tests/Listing01.01.Tests.cs",
            "Chapter42.Tests/Listing01.01A.Some.Tests.cs",
            "Chapter42.Tests/Listing01.01B.Tests.cs",
            "Chapter42.Tests/Listing01.01C.Tests.cs",
            "Chapter42.Tests/Listing01.05.Tests.cs"
        };

        ICollection<string> expectedFiles = new List<string>
        {
            @"Chapter42\Listing42.01.cs",
            @"Chapter42\Listing42.02.Some.cs",
            @"Chapter42\Listing42.03.cs",
            @"Chapter42\Listing42.04.cs",
            @"Chapter42\Listing42.05.cs",
            @"Chapter42.Tests\Listing42.01.Tests.cs",
            @"Chapter42.Tests\Listing42.02.Some.Tests.cs",
            @"Chapter42.Tests\Listing42.03.Tests.cs",
            @"Chapter42.Tests\Listing42.04.Tests.cs",
            @"Chapter42.Tests\Listing42.05.Tests.cs"
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
        DirectoryInfo tempDir = CreateTempDirectory();
        DirectoryInfo chapterDir = CreateTempDirectory(tempDir, name: "Chapter42");
        CreateTempDirectory(tempDir, name: "Chapter42.Tests");
        WriteFiles(tempDir, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

        ListingManager listingManager = new(TempDirectory, new OSStorageManager());
        listingManager.UpdateChapterListingNumbers(chapterDir, byFolder: true);

        List<string> files = FileManager.GetAllFilesAtPath(tempDir.FullName, true)
            .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

        // Assert
        Assert.Equivalent(expectedFiles, files);
    }

    [Fact]
    public void
    UpdateChapterListingNumbersUsingChapterNumberFromFolder_UnitTestAndListingPairingIsMaintained_ListingsAndTestsUpdated()
    {
        ICollection<string> filesToMake = new List<string>
        {
            @"Chapter02\Listing01.01.HelloWorldInC#.cs",
            @"Chapter02\Listing01.02.SampleNETCoreConsoleProjectFile.cs",
            @"Chapter02\Listing01.02B.MultipleStatementsOneOneLine.cs",
            @"Chapter02\Listing01.02C.MultipleStatementsOnSeparateLines.cs",
            @"Chapter02\Listing01.02D.SplittingAStatementAcrossMultipleLines.cs",
            @"Chapter02\Listing01.02E.HelloWorldInC#.cs",
            @"Chapter02\Listing01.03.BasicClassDeclaration.cs",
            @"Chapter02\Listing01.04.BreakingApartHelloWorld.cs",
            @"Chapter02\Listing01.05.TheMainMethodWithParametersAndAReturn.cs",
            @"Chapter02\Listing01.08.NoIndentationFormatting.cs",
            @"Chapter02\Listing01.09.RemovingWhitespace.cs",
            @"Chapter02\Listing01.10.DeclaringAndAssigningAVariable.cs",
            @"Chapter02\Listing01.11.DeclaringTwoVariablesWithinOneStatement.cs",
            @"Chapter02\Listing01.12.ChangingTheValueOfAVariable.cs",
            @"Chapter02\Listing01.13.AssignmentReturningAValueThatCanBeassignedAgain.cs",
            @"Chapter02\Listing01.14.UsingSystemConsoleReadLine.cs",
            @"Chapter02\Listing01.15.UsingSystemConsoleRead.cs",
            @"Chapter02\Listing01.16.FormattingUsingStringInterpolation.cs",
            @"Chapter02\Listing01.17.FormattingUsingCompositeFormatting.cs",
            @"Chapter02\Listing01.18.SwappingTheIndexedPlaceholdersAndCorrespondingVariables.cs",
            @"Chapter02\Listing01.19.CommentingYourCode.cs",
            @"Chapter02\Listing01.20.SampleCILOutput.cs",
            @"Chapter02.Tests\Listing01.01.Tests.cs",
            @"Chapter02.Tests\Listing01.02B.Tests.cs",
            @"Chapter02.Tests\Listing01.02C.Tests.cs",
            @"Chapter02.Tests\Listing01.02D.Tests.cs",
            @"Chapter02.Tests\Listing01.02E.Tests.cs",
            @"Chapter02.Tests\Listing01.04.Tests.cs",
            @"Chapter02.Tests\Listing01.05.Tests.cs",
            @"Chapter02.Tests\Listing01.08.Tests.cs",
            @"Chapter02.Tests\Listing01.09.Tests.cs",
            @"Chapter02.Tests\Listing01.10.Tests.cs",
            @"Chapter02.Tests\Listing01.11.Tests.cs",
            @"Chapter02.Tests\Listing01.12.Tests.cs",
            @"Chapter02.Tests\Listing01.13.Tests.cs",
            @"Chapter02.Tests\Listing01.14.Tests.cs",
            @"Chapter02.Tests\Listing01.15.Tests.cs",
            @"Chapter02.Tests\Listing01.16.Tests.cs",
            @"Chapter02.Tests\Listing01.17.Tests.cs",
            @"Chapter02.Tests\Listing01.18.Tests.cs",
            @"Chapter02.Tests\Listing01.19.Tests.cs"
        };

        ICollection<string> expectedFiles = new List<string>
        {
            @"Chapter02\Listing02.01.HelloWorldInC#.cs",
            @"Chapter02\Listing02.02.SampleNETCoreConsoleProjectFile.cs",
            @"Chapter02\Listing02.03.MultipleStatementsOneOneLine.cs",
            @"Chapter02\Listing02.04.MultipleStatementsOnSeparateLines.cs",
            @"Chapter02\Listing02.05.SplittingAStatementAcrossMultipleLines.cs",
            @"Chapter02\Listing02.06.HelloWorldInC#.cs",
            @"Chapter02\Listing02.07.BasicClassDeclaration.cs",
            @"Chapter02\Listing02.08.BreakingApartHelloWorld.cs",
            @"Chapter02\Listing02.09.TheMainMethodWithParametersAndAReturn.cs",
            @"Chapter02\Listing02.10.NoIndentationFormatting.cs",
            @"Chapter02\Listing02.11.RemovingWhitespace.cs",
            @"Chapter02\Listing02.12.DeclaringAndAssigningAVariable.cs",
            @"Chapter02\Listing02.13.DeclaringTwoVariablesWithinOneStatement.cs",
            @"Chapter02\Listing02.14.ChangingTheValueOfAVariable.cs",
            @"Chapter02\Listing02.15.AssignmentReturningAValueThatCanBeassignedAgain.cs",
            @"Chapter02\Listing02.16.UsingSystemConsoleReadLine.cs",
            @"Chapter02\Listing02.17.UsingSystemConsoleRead.cs",
            @"Chapter02\Listing02.18.FormattingUsingStringInterpolation.cs",
            @"Chapter02\Listing02.19.FormattingUsingCompositeFormatting.cs",
            @"Chapter02\Listing02.20.SwappingTheIndexedPlaceholdersAndCorrespondingVariables.cs",
            @"Chapter02\Listing02.21.CommentingYourCode.cs",
            @"Chapter02\Listing02.22.SampleCILOutput.cs",
            @"Chapter02.Tests\Listing02.01.HelloWorldInC#.Tests.cs",
            @"Chapter02.Tests\Listing02.03.MultipleStatementsOneOneLine.Tests.cs",
            @"Chapter02.Tests\Listing02.04.MultipleStatementsOnSeparateLines.Tests.cs",
            @"Chapter02.Tests\Listing02.05.SplittingAStatementAcrossMultipleLines.Tests.cs",
            @"Chapter02.Tests\Listing02.06.HelloWorldInC#.Tests.cs",
            @"Chapter02.Tests\Listing02.08.BreakingApartHelloWorld.Tests.cs",
            @"Chapter02.Tests\Listing02.09.TheMainMethodWithParametersAndAReturn.Tests.cs",
            @"Chapter02.Tests\Listing02.10.NoIndentationFormatting.Tests.cs",
            @"Chapter02.Tests\Listing02.11.RemovingWhitespace.Tests.cs",
            @"Chapter02.Tests\Listing02.12.DeclaringAndAssigningAVariable.Tests.cs",
            @"Chapter02.Tests\Listing02.13.DeclaringTwoVariablesWithinOneStatement.Tests.cs",
            @"Chapter02.Tests\Listing02.14.ChangingTheValueOfAVariable.Tests.cs",
            @"Chapter02.Tests\Listing02.15.AssignmentReturningAValueThatCanBeassignedAgain.Tests.cs",
            @"Chapter02.Tests\Listing02.16.UsingSystemConsoleReadLine.Tests.cs",
            @"Chapter02.Tests\Listing02.17.UsingSystemConsoleRead.Tests.cs",
            @"Chapter02.Tests\Listing02.18.FormattingUsingStringInterpolation.Tests.cs",
            @"Chapter02.Tests\Listing02.19.FormattingUsingCompositeFormatting.Tests.cs",
            @"Chapter02.Tests\Listing02.20.SwappingTheIndexedPlaceholdersAndCorrespondingVariables.Tests.cs",
            @"Chapter02.Tests\Listing02.21.CommentingYourCode.Tests.cs",
        };

        IEnumerable<string> toWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter01.Listing18_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        };
        DirectoryInfo tempDir = CreateTempDirectory();
        DirectoryInfo chapterDir = CreateTempDirectory(tempDir, name: "Chapter02");
        CreateTempDirectory(tempDir, name: "Chapter02.Tests");
        WriteFiles(tempDir, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

        ListingManager listingManager = new(TempDirectory, new OSStorageManager());
        listingManager.UpdateChapterListingNumbers(chapterDir, byFolder: true);

        List<string> files = FileManager.GetAllFilesAtPath(tempDir.FullName, true)
            .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

        // Assert
        Assert.Equivalent(expectedFiles, files);
    }

    [Fact]
    public void RenumberAllFilesIncludingXML_DontChangeFiles_ListingsAndTestsUpdated()
    {
        // Make sure csproj file is created, but doesn't get renumbered (is ignored)
        List<string> filesToMake = new()
        {
            @"Chapter18.csproj",
            @"Chapter18\Listing18.01.UsingTypeGetPropertiesToObtainAnObjectsPublicProperties.cs",
            @"Chapter18\Listing18.02.UsingTypeofToCreateASystem.TypeInstance.cs",
            @"Chapter18\Listing18.03.csproj.xml",
            @"Chapter18\Listing18.04.DeclaringTheStackClass.cs",
            @"Chapter18\Listing18.05.ReflectionWithGenerics.cs",
            @"Chapter18.Tests\Listing18.01.UsingTypeGetPropertiesToObtainAnObjectsPublicProperties.Tests.cs",
            @"Chapter18.Tests\Listing18.02.Tests.cs",
            @"Chapter18.Tests\Listing18.05.ReflectionWithGenerics.Tests.cs",
        };
        List<string> expectedFiles = filesToMake.GetRange(1, filesToMake.Count - 1);
        Assert.Equal(filesToMake.Count - 1, expectedFiles.Count);

        IEnumerable<string> toWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter18.Listing18_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        };

        DirectoryInfo tempDir = CreateTempDirectory();
        DirectoryInfo chapterDir = CreateTempDirectory(tempDir, name: "Chapter18");
        CreateTempDirectory(tempDir, name: "Chapter18.Tests");
        WriteFiles(tempDir, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

        ListingManager listingManager = new(chapterDir, new OSStorageManager());
        listingManager.UpdateChapterListingNumbers(chapterDir);

        List<string> files = FileManager.GetAllFilesAtPath(tempDir.FullName, true)
            .Where(x => ListingInformation.ApprovedFileTypes.Contains(Path.GetExtension(x))).OrderBy(x => x).ToList();

        // Assert
        Assert.Equivalent(expectedFiles, files);
    }

    [Fact]
    public void UpdateChapterListingNumbers_OSStorageManagerByFolder_NamespacesUpdated()
    {
        ICollection<string> filesToMake = new List<string>
        {
            Path.Join("Chapter42","Listing18.06.cs"),
        };

        ICollection<string> expectedFiles = new List<string>
        {
            Path.Join("Chapter42","Listing42.01.cs")
        };

        IEnumerable<string> toWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter18.Listing18_06",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        };

        IEnumerable<string> expectedToWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter42.Listing42_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}",
        };
        DirectoryInfo tempDir = CreateTempDirectory();
        DirectoryInfo chapterDir = CreateTempDirectory(tempDir, name: "Chapter42");
        CreateTempDirectory(tempDir, name: "Chapter42.Tests");
        WriteFiles(tempDir, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

        ListingManager listingManager = new(TempDirectory, new OSStorageManager());
        listingManager.UpdateChapterListingNumbers(chapterDir, byFolder: true);

        List<string> files = FileManager.GetAllFilesAtPath(tempDir.FullName, true)
            .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

        // Assert
        string expectedFile = Assert.Single(files);
        Assert.Equivalent(expectedFiles, files);

        Assert.Equal(string.Join(Environment.NewLine, expectedToWrite) + Environment.NewLine, File.ReadAllText(expectedFile));
    }

    [Fact]
    public void UpdateChapterListingNumbers_OSStorageManager_NamespacesUpdated()
    {
        ICollection<string> filesToMake = new List<string>
        {
            Path.Join("Chapter42","Listing42.06.cs"),
        };

        ICollection<string> expectedFiles = new List<string>
        {
            Path.Join("Chapter42","Listing42.01.cs")
        };

        IEnumerable<string> toWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter18.Listing18_06",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        };

        IEnumerable<string> expectedToWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter42.Listing42_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}",
        };
        DirectoryInfo tempDir = CreateTempDirectory();
        DirectoryInfo chapterDir = CreateTempDirectory(tempDir, name: "Chapter42");
        CreateTempDirectory(tempDir, name: "Chapter42.Tests");
        WriteFiles(tempDir, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

        ListingManager listingManager = new(TempDirectory, new OSStorageManager());
        listingManager.UpdateChapterListingNumbers(chapterDir);

        List<string> files = FileManager.GetAllFilesAtPath(tempDir.FullName, true)
            .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

        // Assert
        string expectedFile = Assert.Single(files);
        Assert.Equivalent(expectedFiles, files);

        Assert.Equal(string.Join(Environment.NewLine, expectedToWrite) + Environment.NewLine, File.ReadAllText(expectedFile));
    }
    #endregion UsingOSStorageManager
    #endregion UpdateChapterListingNumbers

    #region PopulateListingDataFromPath
    [Fact]
    public void PopulateListingDataFromPath_GivenDirectoryOfListings_PopulateListingInformation()
    {
        List<string> filesToMake = new()
        {
            "Listing01.01.SpecifyingLiteralValues.cs",
            "Listing01.02.cs",
            "Listing01.04.cs",
            "Listing01.06.Something.cs"
        };

        List<string> expectedFiles = new()
        {
            "Listing01.01.SpecifyingLiteralValues.cs",
            "Listing01.02.cs",
            "Listing01.03.cs",
            "Listing01.04.Something.cs"
        };

        List<string> toWrite = new()
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter18.Listing18_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        };
        WriteFiles(TempDirectory, filesToMake, toWrite);
        expectedFiles = ConvertFileNamesToFullPath(expectedFiles, null).ToList();

        List<ListingInformation> listingInformation = ListingManager.PopulateListingDataFromPath(TempDirectory.FullName, true);
        Assert.Equal(4, listingInformation.Count);
        Assert.All(listingInformation, item => Assert.Equal(01, item.OriginalChapterNumber));
        Assert.Equal(TempDirectory.FullName + "\\" + filesToMake[0], listingInformation[0].Path);
        Assert.Equal(TempDirectory.FullName + "\\" + filesToMake[0] + ListingInformation.TemporaryExtension, listingInformation[0].TemporaryPath);
    }

    [Fact]
    public void PopulateListingDataFromPath_GivenDirectoryOfListingsAndTests_AssociateTestWithProperListing()
    {
        List<string> filesToMake = new()
        {
            @"Chapter18.csproj",
            @"Chapter18\Listing18.01.UsingTypeGetPropertiesToObtainAnObjectsPublicProperties.cs",
            @"Chapter18\Listing18.02.UsingTypeofToCreateASystem.TypeInstance.cs",
            @"Chapter18\Listing18.03.csproj.xml",
            @"Chapter18\Listing18.04.DeclaringTheStackClass.cs",
            @"Chapter18\Listing18.05.ReflectionWithGenerics.cs",
            @"Chapter18.Tests\Listing18.01.UsingTypeGetPropertiesToObtainAnObjectsPublicProperties.Tests.cs",
            @"Chapter18.Tests\Listing18.02.Tests.cs",
            @"Chapter18.Tests\Listing18.05.ReflectionWithGenerics.Tests.cs",
        };
        List<string> expectedFiles = filesToMake.GetRange(1, filesToMake.Count - 1);
        Assert.Equal(filesToMake.Count - 1, expectedFiles.Count);

        IEnumerable<string> toWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter18.Listing18_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        };

        DirectoryInfo tempDir = CreateTempDirectory();
        DirectoryInfo chapterDir = CreateTempDirectory(tempDir, name: "Chapter18");
        CreateTempDirectory(tempDir, name: "Chapter18.Tests");
        WriteFiles(tempDir, filesToMake, toWrite);

        List<ListingInformation> listingInformation = ListingManager.PopulateListingDataFromPath(tempDir.FullName + $"\\Chapter18", false);
        Assert.Equal(5, listingInformation.Count);
        Assert.All(listingInformation, item => Assert.Equal(18, item.OriginalChapterNumber));
        Assert.Equal(tempDir.FullName + "\\" + filesToMake[1], listingInformation[0].Path);
        Assert.Equal(tempDir.FullName + "\\" + filesToMake[1] + ListingInformation.TemporaryExtension, listingInformation[0].TemporaryPath);

        IReadOnlyList<ListingInformation> listingsWithTests = listingInformation.Where(listing => listing.AssociatedTest is not null).ToList();
        Assert.Equal(3, listingsWithTests.Count);
        Assert.All(listingsWithTests, listing => Assert.NotNull(listing.AssociatedTest));
        Assert.All(listingsWithTests, listing => Assert.Equal(18, listing.AssociatedTest!.OriginalChapterNumber));
        Assert.All(listingsWithTests, listing => Assert.Equal(listing.OriginalListingNumber, listing.AssociatedTest!.OriginalListingNumber));
        Assert.All(listingsWithTests, listing => Assert.Equal(listing.OriginalListingNumberSuffix, listing.AssociatedTest!.OriginalListingNumberSuffix));
        Assert.All(listingsWithTests, listing => Assert.Equal(listing.OriginalChapterNumber, listing.AssociatedTest!.OriginalChapterNumber));
    }

    [Theory]
    [InlineData(@"Chapter18\Listing18.01.UsingTypeGetPropertiesToObtainAnObjectsPublicProperties.cs", @"Chapter18.Tests\Listing18.01.Tests.cs", "UsingTypeGetPropertiesToObtainAnObjectsPublicProperties.Tests")]
    [InlineData(@"Chapter18\Listing18.02.UsingTypeofToCreateASystem.TypeInstance.cs", @"Chapter18.Tests\Listing18.02.Tests.cs", "UsingTypeofToCreateASystem.TypeInstance.Tests")]
    [InlineData(@"Chapter18\Listing18.05.ReflectionWithGenerics.cs", @"Chapter18.Tests\Listing18.05.ReflectionWithGenerics.Tests.cs", "ReflectionWithGenerics.Tests")]
    public void PopulateListingDataFromPath_GivenDirectoryOfListingsAndTests_UpdateTestWithListingCaption(string listingPath, string testPath, string expected)
    {
        List<string> filesToMake = new()
        {
            listingPath,
            testPath
        };
        List<string> expectedFiles = filesToMake.GetRange(1, filesToMake.Count - 1);
        Assert.Equal(filesToMake.Count - 1, expectedFiles.Count);

        IEnumerable<string> toWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter18.Listing18_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        };

        DirectoryInfo tempDir = CreateTempDirectory();
        CreateTempDirectory(tempDir, name: "Chapter18");
        CreateTempDirectory(tempDir, name: "Chapter18.Tests");
        WriteFiles(tempDir, filesToMake, toWrite);

        List<ListingInformation> listingInformation = ListingManager.PopulateListingDataFromPath(tempDir.FullName + $"\\Chapter18", false);
        Assert.Single(listingInformation);
        Assert.NotNull(listingInformation.First().AssociatedTest);
        Assert.Equal(expected, listingInformation.First().AssociatedTest!.Caption);
    }

    [Fact]
    public void PopulateListingDataFromPath_GivenSingleDirectoryOfListingsAndTests_AssociateTestWithProperListing()
    {
        List<string> filesToMake = new()
        {
            @"Chapter18.csproj",
            @"Listing18.01.UsingTypeGetPropertiesToObtainAnObjectsPublicProperties.cs",
            @"Listing18.02.UsingTypeofToCreateASystem.TypeInstance.cs",
            @"Listing18.03.csproj.xml",
            @"Listing18.04.DeclaringTheStackClass.cs",
            @"Listing18.05.ReflectionWithGenerics.cs",
            @"Listing18.01.UsingTypeGetPropertiesToObtainAnObjectsPublicProperties.Tests.cs",
            @"Listing18.02.Tests.cs",
            @"Listing18.05.ReflectionWithGenerics.Tests.cs",
        };
        List<string> expectedFiles = filesToMake.GetRange(1, filesToMake.Count - 1);
        Assert.Equal(filesToMake.Count - 1, expectedFiles.Count);

        IEnumerable<string> toWrite = new List<string>
        {
            "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter18.Listing18_01",
            "{",
            "    using System;",
            "    using System.Reflection;",
            "    public class Program { }",
            "}"
        };

        DirectoryInfo tempDir = CreateTempDirectory();
        WriteFiles(tempDir, filesToMake, toWrite);

        List<ListingInformation> listingInformation = ListingManager.PopulateListingDataFromPath(tempDir.FullName, true);
        Assert.Equal(5, listingInformation.Count);
        Assert.All(listingInformation, item => Assert.Equal(18, item.OriginalChapterNumber));
        Assert.Equal(tempDir.FullName + "\\" + filesToMake[1], listingInformation[0].Path);
        Assert.Equal(tempDir.FullName + "\\" + filesToMake[1] + ListingInformation.TemporaryExtension, listingInformation[0].TemporaryPath);

        IReadOnlyList<ListingInformation> listingsWithTests = listingInformation.Where(listing => listing.AssociatedTest is not null).ToList();
        Assert.Equal(3, listingsWithTests.Count);
        Assert.All(listingsWithTests, listing => Assert.NotNull(listing.AssociatedTest));
        Assert.All(listingsWithTests, listing => Assert.Equal(18, listing.AssociatedTest!.OriginalChapterNumber));
        Assert.All(listingsWithTests, listing => Assert.Equal(listing.OriginalListingNumber, listing.AssociatedTest!.OriginalListingNumber));
        Assert.All(listingsWithTests, listing => Assert.Equal(listing.OriginalListingNumberSuffix, listing.AssociatedTest!.OriginalListingNumberSuffix));
        Assert.All(listingsWithTests, listing => Assert.Equal(listing.OriginalChapterNumber, listing.AssociatedTest!.OriginalChapterNumber));
    }
    #endregion PopulateListingDataFromPath

    [Theory]
    [InlineData("Chapter01", "Listing01.01A.cs")]
    [InlineData("Chapter02", "Listing02.01.cs")]
    [InlineData("Chapter01", "Listing01.01A.Something.cs")]
    [InlineData("Chapter02", "Listing02.01.Something.cs")]
    public void GetPathToAccompanyingUnitTest_GivenListingWithNoTest_CorrectPathReturned(string chapter,
        string listingName)
    {
        bool result =
            ListingManager.GetPathToAccompanyingUnitTest(chapter + Path.DirectorySeparatorChar + listingName,
                out string pathToTest);
        char directorySeparator = Path.DirectorySeparatorChar;
        Assert.False(result);
        Assert.Equal($"{chapter}.Tests{directorySeparator}{listingName}", pathToTest);
    }
}