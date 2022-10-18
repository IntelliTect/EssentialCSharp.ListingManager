using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ListingManager.Tests
{
    [TestClass]
    public class ListingManagerTests : TempFileTestBase
    {
        [TestMethod]
        [DataRow("Listing02.01.SpecifyingLiteralValues.cs", false)]
        [DataRow("Listing02.01A.SpecifyingLiteralValues.cs", true)]
        [DataRow("Listing02.01.cs", false)]
        public void IsIncorrectListingFromPath_FindsIncorrectListing(string fileName, bool expectedResult)
        {
            string path = Path.Combine(TempDirectory.ToString(), fileName);

            bool actualResult = ListingManager.IsExtraListing(path);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        [DataRow("/Chapter02.Tests", "Listing02.01A.SpecifyingLiteralValues.cs", false)]
        public void ListingsInTestDirectories_AreNotCountedAsExtraListings(string parentDirectory, string fileName,
            bool expectedResult)
        {
            var directory = CreateTempDirectory(name: parentDirectory);
            var fileInfo = CreateTempFile(directory, fileName, fileName);

            string directoryName = fileInfo.DirectoryName ?? string.Empty;

            string path = Path.Combine(directory.FullName, fileName);

            bool actualResult = ListingManager.IsExtraListing(path);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        public void GetAllExtraListings_ExtraListingsReturned()
        {
            ICollection<string> filesToMake = new List<string>
            {
                @"Listing02.01B.Something.cs",
                @"Listing02.02.cs",
                @"Listing02.03C.cs"
            };

            var tempDir = CreateTempDirectory(name: "Chapter02");
            ICollection<string> expectedFiles = filesToMake;
            expectedFiles.Remove(@"Listing02.02.cs");
            expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

            WriteFiles(tempDir, filesToMake, null);

            var extraListings = ListingManager.GetAllExtraListings(tempDir.FullName).ToList();

            CollectionAssert.AreEquivalent((ICollection)expectedFiles, extraListings);
        }

        [TestMethod]
        [Ignore]
        public void UpdateChapterListingNumbers_ListingsWithinListMissing_ListingsRenumbered()
        {
            List<string> filesToMake = new List<string>
            {
                "Listing01.01.SpecifyingLiteralValues.cs",
                "Listing01.02.cs",
                "Listing01.04.cs",
                "Listing01.06.Something.cs"
            };

            List<string> expectedFiles = new List<string>
            {
                "Listing01.01.SpecifyingLiteralValues.cs",
                "Listing01.02.cs",
                "Listing01.03.cs",
                "Listing01.04.Something.cs"
            };

            List<string> toWrite = new List<string>
            {
                "namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter18.Listing18_01",
                "{",
                "    using System;",
                "    using System.Reflection;",
                "    public class Program { }",
                "}"
            };
            WriteFiles(TempDirectory, filesToMake, toWrite);
            expectedFiles = (List<string>)ConvertFileNamesToFullPath(expectedFiles, null);

            ListingManager.UpdateChapterListingNumbers(TempDirectory.FullName);

            var files = Directory.EnumerateFiles(TempDirectory.FullName)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();
            CollectionAssert.AreEquivalent(expectedFiles, files);
        }

        [TestMethod]
        [Ignore]
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

            ListingManager.UpdateChapterListingNumbers(TempDirectory.FullName);

            var files = Directory.EnumerateFiles(TempDirectory.FullName)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

            CollectionAssert.AreEquivalent((ICollection)expectedFiles, files);
        }

        [TestMethod]
        [Ignore]
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

            ListingManager.UpdateChapterListingNumbers(TempDirectory.FullName);

            var files = Directory.EnumerateFiles(TempDirectory.FullName)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

            CollectionAssert.AreEquivalent((ICollection)expectedFiles, files);
        }

        [TestMethod]
        [Ignore]
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

            ListingManager.UpdateChapterListingNumbers(TempDirectory.FullName);

            var files = Directory.EnumerateFiles(TempDirectory.FullName)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

            CollectionAssert.AreEquivalent((ICollection)expectedFiles, files);
        }

        [TestMethod]
        [Ignore]
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
            var tempDir = CreateTempDirectory();
            var chapterDir = CreateTempDirectory(tempDir, name: "Chapter01");
            CreateTempDirectory(tempDir, name: "Chapter01.Tests");
            WriteFiles(tempDir, filesToMake, toWrite);
            expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

            ListingManager.UpdateChapterListingNumbers(chapterDir.FullName);

            var files = FileManager.GetAllFilesAtPath(tempDir.FullName, true)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

            CollectionAssert.AreEquivalent((ICollection)expectedFiles, files);
        }


        [TestMethod]
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
            var tempDir = CreateTempDirectory();
            var chapterDir = CreateTempDirectory(tempDir, name: "Chapter42");
            CreateTempDirectory(tempDir, name: "Chapter42.Tests");
            WriteFiles(tempDir, filesToMake, toWrite);
            expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

            ListingManager.UpdateChapterListingNumbers(chapterDir.FullName, byFolder: true);

            var files = FileManager.GetAllFilesAtPath(tempDir.FullName, true)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

            //Assert
            CollectionAssert.AreEquivalent((ICollection)expectedFiles, files);
        }

        [TestMethod]
        [Ignore]
        public void
        UpdateChapterListingNumbersUsingChapterNumberFromFolder_UnitTestAndListingPairingIsMaintained_ListingsAndTestsUpdated()
        {
            ICollection<string> filesToMake = new List<string>
            {
                @"Chapter01\Listing01.01.HelloWorldInC#.cs",
                @"Chapter01\Listing01.02.SampleNETCoreConsoleProjectFile.cs",
                @"Chapter01\Listing01.02B.MultipleStatementsOneOneLine.cs",
                @"Chapter01\Listing01.02C.MultipleStatementsOnSeparateLines.cs",
                @"Chapter01\Listing01.02D.SplittingAStatementAcrossMultipleLines.cs",
                @"Chapter01\Listing01.02E.HelloWorldInC#.cs",
                @"Chapter01\Listing01.03.BasicClassDeclaration.cs",
                @"Chapter01\Listing01.04.BreakingApartHelloWorld.cs",
                @"Chapter01\Listing01.05.TheMainMethodWithParametersAndAReturn.cs",
                @"Chapter01\Listing01.08.NoIndentationFormatting.cs",
                @"Chapter01\Listing01.09.RemovingWhitespace.cs",
                @"Chapter01\Listing01.10.DeclaringAndAssigningAVariable.cs",
                @"Chapter01\Listing01.11.DeclaringTwoVariablesWithinOneStatement.cs",
                @"Chapter01\Listing01.12.ChangingTheValueOfAVariable.cs",
                @"Chapter01\Listing01.13.AssignmentReturningAValueThatCanBeassignedAgain.cs",
                @"Chapter01\Listing01.14.UsingSystemConsoleReadLine.cs",
                @"Chapter01\Listing01.15.UsingSystemConsoleRead.cs",
                @"Chapter01\Listing01.16.FormattingUsingStringInterpolation.cs",
                @"Chapter01\Listing01.17.FormattingUsingCompositeFormatting.cs",
                @"Chapter01\Listing01.18.SwappingTheIndexedPlaceholdersAndCorrespondingVariables.cs",
                @"Chapter01\Listing01.19.CommentingYourCode.cs",
                @"Chapter01\Listing01.20.SampleCILOutput.cs",
                @"Chapter01.Tests\Listing01.01.Tests.cs",
                @"Chapter01.Tests\Listing01.02B.Tests.cs",
                @"Chapter01.Tests\Listing01.02C.Tests.cs",
                @"Chapter01.Tests\Listing01.02D.Tests.cs",
                @"Chapter01.Tests\Listing01.02E.Tests.cs",
                @"Chapter01.Tests\Listing01.04.Tests.cs",
                @"Chapter01.Tests\Listing01.05.Tests.cs",
                @"Chapter01.Tests\Listing01.08.Tests.cs",
                @"Chapter01.Tests\Listing01.09.Tests.cs",
                @"Chapter01.Tests\Listing01.10.Tests.cs",
                @"Chapter01.Tests\Listing01.11.Tests.cs",
                @"Chapter01.Tests\Listing01.12.Tests.cs",
                @"Chapter01.Tests\Listing01.13.Tests.cs",
                @"Chapter01.Tests\Listing01.14.Tests.cs",
                @"Chapter01.Tests\Listing01.15.Tests.cs",
                @"Chapter01.Tests\Listing01.16.Tests.cs",
                @"Chapter01.Tests\Listing01.17.Tests.cs",
                @"Chapter01.Tests\Listing01.18.Tests.cs",
                @"Chapter01.Tests\Listing01.19.Tests.cs"
            };

            ICollection<string> expectedFiles = new List<string>
            {
                @"Chapter01\Listing01.01.HelloWorldInC#.cs",
                @"Chapter01\Listing01.02.SampleNETCoreConsoleProjectFile.cs",
                @"Chapter01\Listing01.03.MultipleStatementsOneOneLine.cs",
                @"Chapter01\Listing01.04.MultipleStatementsOnSeparateLines.cs",
                @"Chapter01\Listing01.05.SplittingAStatementAcrossMultipleLines.cs",
                @"Chapter01\Listing01.06.HelloWorldInC#.cs",
                @"Chapter01\Listing01.07.BasicClassDeclaration.cs",
                @"Chapter01\Listing01.08.BreakingApartHelloWorld.cs",
                @"Chapter01\Listing01.09.TheMainMethodWithParametersAndAReturn.cs",
                @"Chapter01\Listing01.10.NoIndentationFormatting.cs",
                @"Chapter01\Listing01.11.RemovingWhitespace.cs",
                @"Chapter01\Listing01.12.DeclaringAndAssigningAVariable.cs",
                @"Chapter01\Listing01.13.DeclaringTwoVariablesWithinOneStatement.cs",
                @"Chapter01\Listing01.14.ChangingTheValueOfAVariable.cs",
                @"Chapter01\Listing01.15.AssignmentReturningAValueThatCanBeassignedAgain.cs",
                @"Chapter01\Listing01.16.UsingSystemConsoleReadLine.cs",
                @"Chapter01\Listing01.17.UsingSystemConsoleRead.cs",
                @"Chapter01\Listing01.18.FormattingUsingStringInterpolation.cs",
                @"Chapter01\Listing01.19.FormattingUsingCompositeFormatting.cs",
                @"Chapter01\Listing01.20.SwappingTheIndexedPlaceholdersAndCorrespondingVariables.cs",
                @"Chapter01\Listing01.21.CommentingYourCode.cs",
                @"Chapter01\Listing01.22.SampleCILOutput.cs",
                @"Chapter01.Tests\Listing01.01.Tests.cs",
                @"Chapter01.Tests\Listing01.03.Tests.cs",
                @"Chapter01.Tests\Listing01.04.Tests.cs",
                @"Chapter01.Tests\Listing01.05.Tests.cs",
                @"Chapter01.Tests\Listing01.06.Tests.cs",
                @"Chapter01.Tests\Listing01.08.Tests.cs",
                @"Chapter01.Tests\Listing01.09.Tests.cs",
                @"Chapter01.Tests\Listing01.10.Tests.cs",
                @"Chapter01.Tests\Listing01.11.Tests.cs",
                @"Chapter01.Tests\Listing01.12.Tests.cs",
                @"Chapter01.Tests\Listing01.13.Tests.cs",
                @"Chapter01.Tests\Listing01.14.Tests.cs",
                @"Chapter01.Tests\Listing01.15.Tests.cs",
                @"Chapter01.Tests\Listing01.16.Tests.cs",
                @"Chapter01.Tests\Listing01.17.Tests.cs",
                @"Chapter01.Tests\Listing01.18.Tests.cs",
                @"Chapter01.Tests\Listing01.19.Tests.cs",
                @"Chapter01.Tests\Listing01.20.Tests.cs",
                @"Chapter01.Tests\Listing01.21.Tests.cs"
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
            var tempDir = CreateTempDirectory();
            var chapterDir = CreateTempDirectory(tempDir, name: "Chapter01");
            CreateTempDirectory(tempDir, name: "Chapter01.Tests");
            WriteFiles(tempDir, filesToMake, toWrite);
            expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

            ListingManager.UpdateChapterListingNumbers(chapterDir.FullName, byFolder: true);

            var files = FileManager.GetAllFilesAtPath(tempDir.FullName, true)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

            //Assert
            CollectionAssert.AreEquivalent((ICollection)expectedFiles, files);
        }


        [TestMethod]
        public void
            UpdateOnlyChapterNumberOfListingUsingChapterNumberFromFolder_UnitTestsAlsoUpdated_ListingsAndTestsUpdated()
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
                @"Chapter42\Listing42.01A.Some.cs",
                @"Chapter42\Listing42.01B.cs",
                @"Chapter42\Listing42.01C.cs",
                @"Chapter42\Listing42.05.cs",
                @"Chapter42.Tests\Listing42.01.Tests.cs",
                @"Chapter42.Tests\Listing42.01A.Some.Tests.cs",
                @"Chapter42.Tests\Listing42.01B.Tests.cs",
                @"Chapter42.Tests\Listing42.01C.Tests.cs",
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
            var tempDir = CreateTempDirectory();
            var chapterDir = CreateTempDirectory(tempDir, name: "Chapter42");
            CreateTempDirectory(tempDir, name: "Chapter42.Tests");
            WriteFiles(tempDir, filesToMake, toWrite);
            expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

            ListingManager.UpdateChapterListingNumbers(chapterDir.FullName,
                byFolder: true, chapterOnly: true);

            var files = FileManager.GetAllFilesAtPath(tempDir.FullName, true)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();

            //Assert
            CollectionAssert.AreEquivalent((ICollection)expectedFiles, files);
        }


        [TestMethod]
        [DataRow("Chapter01", "Listing01.01A.cs")]
        [DataRow("Chapter02", "Listing02.01.cs")]
        [DataRow("Chapter01", "Listing01.01A.Something.cs")]
        [DataRow("Chapter02", "Listing02.01.Something.cs")]
        public void GetPathToAccompanyingUnitTest_GivenListingWithNoTest_CorrectPathReturned(string chapter,
            string listingName)
        {
            bool result =
                ListingManager.GetPathToAccompanyingUnitTest(chapter + Path.DirectorySeparatorChar + listingName,
                    out string pathToTest);
            char directorySeparator = Path.DirectorySeparatorChar;
            Assert.IsFalse(result);
            Assert.AreEqual($"{chapter}.Tests{directorySeparator}{listingName}", pathToTest);
        }

        [TestMethod]
        [Ignore]
        public void GenerateUnitTests_TestsGenerated()
        {
            string chapter = "Chapter01";

            List<string> filesToCreate = new List<string>
            {
                @"Listing01.01.Something.cs",
                @"Listing01.02A.cs",
                @"Listing01.03B.Other.cs"
            };

            var tempDir = CreateTempDirectory();
            var chapterDir = CreateTempDirectory(tempDir, name: chapter);
            var expectedFilesList = new List<string>();
            foreach (string file in filesToCreate)
            {
                expectedFilesList.Add(@"Chapter01.Tests\" + file);
            }

            var expectedFiles = (ICollection<string>)expectedFilesList;
            expectedFiles = ConvertFileNamesToFullPath(expectedFiles, tempDir).ToList();

            WriteFiles(chapterDir, filesToCreate, null);

            var generatedTests = ListingManager.GenerateUnitTests(
                chapterDir.FullName);

            CollectionAssert.AreEquivalent((ICollection)expectedFiles, (ICollection)generatedTests);
        }


        private IEnumerable<string> ConvertFileNamesToFullPath(IEnumerable<string> fileNamesToConvert,
            DirectoryInfo? targetDirectory)
        {
            var fullPaths = new List<string>();

            foreach (string fileName in fileNamesToConvert)
            {
                fullPaths.Add(Path.Combine(targetDirectory?.FullName ?? TempDirectory.FullName, fileName));
            }

            return fullPaths;
        }

        private FileInfo WriteFile(DirectoryInfo targetDirectory, string fileName, List<string> toWrite)
        {
            var ret = CreateTempFile(targetDirectory, name: fileName, contents: toWrite.ToString());
            return ret;
        }

        private List<FileInfo> WriteFiles(DirectoryInfo targetDirectory, IEnumerable<string> fileNames,
            IEnumerable<string>? toWrite)
        {
            List<string> filesToWrite = toWrite?.ToList() ?? new List<string>();
            List<FileInfo> ret = new List<FileInfo>();
            foreach (string file in fileNames)
            {
                ret.Add(WriteFile(targetDirectory, file, filesToWrite));
            }

            return ret;
        }
    }
}