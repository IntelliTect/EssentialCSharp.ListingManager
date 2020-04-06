using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ListingManager.Tests
{
    [TestClass]
    public class ListingManagerTests
    {
        private List<string> CreatedFiles { get; } = new List<string>();
        private List<string> CreatedDirectories { get; } = new List<string>();

        [TestMethod]
        [DataRow("Listing02.01.SpecifyingLiteralValues.cs", false)]
        [DataRow("Listing02.01A.SpecifyingLiteralValues.cs", true)]
        [DataRow("Chapter02.Tests/Listing02.01A.SpecifyingLiteralValues.cs", false)]
        [DataRow("Listing02.01.cs", false)]
        public void IsIncorrectListingFromPath_FindsIncorrectListing(string fileName, bool expectedResult)
        {
            WriteFile(fileName);

            string directoryName = Path.GetDirectoryName(fileName);

            if (!string.IsNullOrWhiteSpace(directoryName))
            {
                CreatedDirectories.Add(directoryName);
            }

            string path = Path.Combine(Environment.CurrentDirectory, fileName);
            
            bool actualResult = ListingManager.IsExtraListing(path);
            
            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        public void GetAllExtraListings_ExtraListingsReturned()
        {
            ICollection<string> filesToMake = new List<string>
            {
                @"Chapter01\Listing01.01A.SpecifyingLiteralValues.cs",
                @"Chapter02\Listing02.01B.Something.cs",
                @"Chapter02\Listing02.02.cs",
                @"Chapter02\Listing02.03C.cs"
            };
            
            CreatedDirectories.Add("Chapter01");
            CreatedDirectories.Add("Chapter02");

            ICollection<string> expectedFiles = filesToMake;
            expectedFiles.Remove(@"Chapter02\Listing02.02.cs");
            expectedFiles = ConvertFilenamesToFullPath(expectedFiles);
            
            WriteFiles(filesToMake);

            var extraListings = ListingManager.GetAllExtraListings(Environment.CurrentDirectory).ToList();
            
            CollectionAssert.AreEquivalent((ICollection) expectedFiles, (ICollection) extraListings);
        }

        [TestMethod]
        public void UpdateChapterListingNumbers_ListingsWithinListMissing_ListingsRenumbered()
        {
            ICollection<string> filesToMake = new List<string>
            {
                "Listing01.01.SpecifyingLiteralValues.cs",
                "Listing01.02.cs",
                "Listing01.04.cs",
                "Listing01.06.Something.cs"
            };
            
            ICollection<string> expectedFiles = new List<string>
            {
                "Listing01.01.SpecifyingLiteralValues.cs",
                "Listing01.02.cs",
                "Listing01.03.cs",
                "Listing01.04.Something.cs"
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
            WriteFiles(filesToMake, toWrite);
            expectedFiles = ConvertFilenamesToFullPath(expectedFiles);
            foreach (string file in filesToMake)
            {
                CreatedFiles.Remove(file);
            }
            CreatedFiles.AddRange(expectedFiles);
            
            ListingManager.UpdateChapterListingNumbers(Environment.CurrentDirectory);

            var files = Directory.EnumerateFiles(Environment.CurrentDirectory)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();
            CollectionAssert.AreEquivalent((ICollection) expectedFiles, files);
        }
        
        [TestMethod]
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
            WriteFiles(filesToMake, toWrite);
            expectedFiles = ConvertFilenamesToFullPath(expectedFiles);
            foreach (string file in filesToMake)
            {
                CreatedFiles.Remove(file);
            }
            CreatedFiles.AddRange(expectedFiles);
            
            ListingManager.UpdateChapterListingNumbers(Environment.CurrentDirectory);

            var files = Directory.EnumerateFiles(Environment.CurrentDirectory)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();
            
            CollectionAssert.AreEquivalent((ICollection) expectedFiles, files);
        }
        
        [TestMethod]
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
            WriteFiles(filesToMake, toWrite);
            expectedFiles = ConvertFilenamesToFullPath(expectedFiles);
            foreach (string file in filesToMake)
            {
                CreatedFiles.Remove(file);
            }
            CreatedFiles.AddRange(expectedFiles);
            
            ListingManager.UpdateChapterListingNumbers(Environment.CurrentDirectory);

            var files = Directory.EnumerateFiles(Environment.CurrentDirectory)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();
            
            CollectionAssert.AreEquivalent((ICollection) expectedFiles, files);
        }
        
        [TestMethod]
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
            WriteFiles(filesToMake, toWrite);
            expectedFiles = ConvertFilenamesToFullPath(expectedFiles);
            foreach (string file in filesToMake)
            {
                CreatedFiles.Remove(file);
            }
            CreatedFiles.AddRange(expectedFiles);
            
            ListingManager.UpdateChapterListingNumbers(Environment.CurrentDirectory);

            var files = Directory.EnumerateFiles(Environment.CurrentDirectory)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();
            
            CollectionAssert.AreEquivalent((ICollection) expectedFiles, files);
        }

        [TestMethod]
        public void UpdateChapterListingNumbers_UnitTestsAlsoUpdated_ListingsAndTestsUpdated()
        {
            ICollection<string> filesToMake = new List<string>
            {
                "Chapter01/Listing01.01.cs",
                "Chapter01/Listing01.01A.Some.cs",
                "Chapter01/Listing01.01B.cs",
                "Chapter01/Listing01.01C.cs",
                "Chapter01/Listing01.05.cs",
                "Chapter01.Tests/Listing01.01.cs",
                "Chapter01.Tests/Listing01.01A.Some.cs",
                "Chapter01.Tests/Listing01.01B.cs",
                "Chapter01.Tests/Listing01.01C.cs",
                "Chapter01.Tests/Listing01.05.cs"
            };

            ICollection<string> expectedFiles = new List<string>
            {
                @"Chapter01\Listing01.01.cs",
                @"Chapter01\Listing01.02.Some.cs",
                @"Chapter01\Listing01.03.cs",
                @"Chapter01\Listing01.04.cs",
                @"Chapter01\Listing01.05.cs",
                @"Chapter01.Tests\Listing01.01.cs",
                @"Chapter01.Tests\Listing01.02.Some.cs",
                @"Chapter01.Tests\Listing01.03.cs",
                @"Chapter01.Tests\Listing01.04.cs",
                @"Chapter01.Tests\Listing01.05.cs"
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
            
            WriteFiles(filesToMake, toWrite);
            expectedFiles = ConvertFilenamesToFullPath(expectedFiles);
            foreach (string file in filesToMake)
            {
                CreatedFiles.Remove(file);
            }
            CreatedFiles.AddRange(expectedFiles);
            
            CreatedDirectories.Add("Chapter01");
            CreatedDirectories.Add("Chapter01.Tests");
            
            ListingManager.UpdateChapterListingNumbers(Path.Combine(Environment.CurrentDirectory, "Chapter01"));

            var files = FileManager.GetAllFilesAtPath(Environment.CurrentDirectory, true)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();
            
            CollectionAssert.AreEquivalent((ICollection) expectedFiles, files);
        }


        [TestMethod]
        public void UpdateChapterListingNumbersUsingChapterNumberFromFolder_UnitTestsAlsoUpdated_ListingsAndTestsUpdated()
        {
            ICollection<string> filesToMake = new List<string>
            {
                "Chapter42/Listing01.01.cs",
                "Chapter42/Listing01.01A.Some.cs",
                "Chapter42/Listing01.01B.cs",
                "Chapter42/Listing01.01C.cs",
                "Chapter42/Listing01.05.cs",
                "Chapter42.Tests/Listing01.01.cs",
                "Chapter42.Tests/Listing01.01A.Some.cs",
                "Chapter42.Tests/Listing01.01B.cs",
                "Chapter42.Tests/Listing01.01C.cs",
                "Chapter42.Tests/Listing01.05.cs"
            };

            ICollection<string> expectedFiles = new List<string>
            {
                @"Chapter42\Listing42.01.cs",
                @"Chapter42\Listing42.02.Some.cs",
                @"Chapter42\Listing42.03.cs",
                @"Chapter42\Listing42.04.cs",
                @"Chapter42\Listing42.05.cs",
                @"Chapter42.Tests\Listing42.01.cs",
                @"Chapter42.Tests\Listing42.02.Some.cs",
                @"Chapter42.Tests\Listing42.03.cs",
                @"Chapter42.Tests\Listing42.04.cs",
                @"Chapter42.Tests\Listing42.05.cs"
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

            WriteFiles(filesToMake, toWrite);
            expectedFiles = ConvertFilenamesToFullPath(expectedFiles);
            foreach (string file in filesToMake)
            {
                CreatedFiles.Remove(file);
            }
            CreatedFiles.AddRange(expectedFiles);

            CreatedDirectories.Add("Chapter42");
            CreatedDirectories.Add("Chapter42.Tests");

            ListingManager.UpdateChapterListingNumbers(Path.Combine(Environment.CurrentDirectory, "Chapter42"), byfolder:true);

            var files = FileManager.GetAllFilesAtPath(Environment.CurrentDirectory, true)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();
            
            Console.WriteLine();
            foreach (string s in files) {
                Console.WriteLine(s);
            }
            //Assert
            CollectionAssert.AreEquivalent((ICollection)expectedFiles, files);
        }


        [TestMethod]
        public void UpdateOnlyChapterNumberOfListingUsingChapterNumberFromFolder_UnitTestsAlsoUpdated_ListingsAndTestsUpdated()
        {
            ICollection<string> filesToMake = new List<string>
            {
                "Chapter42/Listing01.01.cs",
                "Chapter42/Listing01.01A.Some.cs",
                "Chapter42/Listing01.01B.cs",
                "Chapter42/Listing01.01C.cs",
                "Chapter42/Listing01.05.cs",
                "Chapter42.Tests/Listing01.01.cs",
                "Chapter42.Tests/Listing01.01A.Some.cs",
                "Chapter42.Tests/Listing01.01B.cs",
                "Chapter42.Tests/Listing01.01C.cs",
                "Chapter42.Tests/Listing01.05.cs"
            };

            ICollection<string> expectedFiles = new List<string>
            {
                @"Chapter42\Listing42.01.cs",
                @"Chapter42\Listing42.01A.Some.cs",
                @"Chapter42\Listing42.01B.cs",
                @"Chapter42\Listing42.01C.cs",
                @"Chapter42\Listing42.05.cs",
                @"Chapter42.Tests\Listing42.01.cs",
                @"Chapter42.Tests\Listing42.01A.Some.cs",
                @"Chapter42.Tests\Listing42.01B.cs",
                @"Chapter42.Tests\Listing42.01C.cs",
                @"Chapter42.Tests\Listing42.05.cs"
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

            WriteFiles(filesToMake, toWrite);
            expectedFiles = ConvertFilenamesToFullPath(expectedFiles);
            foreach (string file in filesToMake)
            {
                CreatedFiles.Remove(file);
            }
            CreatedFiles.AddRange(expectedFiles);

            CreatedDirectories.Add("Chapter42");
            CreatedDirectories.Add("Chapter42.Tests");

            ListingManager.UpdateChapterListingNumbers(Path.Combine(Environment.CurrentDirectory, "Chapter42"), byfolder: true, chapteronly:true);

            var files = FileManager.GetAllFilesAtPath(Environment.CurrentDirectory, true)
                .Where(x => Path.GetExtension(x) == ".cs").OrderBy(x => x).ToList();
            Console.WriteLine();
            foreach (string s in files)
            {
                Console.WriteLine(s);
            }

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
            bool result = ListingManager.GetPathToAccompanyingUnitTest(chapter+Path.DirectorySeparatorChar+listingName, out string pathToTest);
            char directorySeperator = Path.DirectorySeparatorChar;
            Assert.IsFalse(result);
            Assert.AreEqual($"{chapter}.Tests{directorySeperator}{listingName}", pathToTest);
        }

        [TestMethod]
        public void GenerateUnitTests_TestsGenerated()
        {
            string chapter = "Chapter01";
            
            ICollection<string> filesToCreate = new List<string>
            {
               @"Chapter01\Listing01.01.Something.cs",
               @"Chapter01\Listing01.02A.cs",
               @"Chapter01\Listing01.03B.Other.cs"
            };

            var expectedFilesList = new List<string>();
            foreach (string file in filesToCreate)
            {
                expectedFilesList.Add(file.Replace(chapter, chapter+".Tests"));
            }
            var expectedFiles = (ICollection<string>) expectedFilesList;
            expectedFiles = ConvertFilenamesToFullPath(expectedFiles);
            
            WriteFiles(filesToCreate);
            CreatedDirectories.Add(chapter);
            CreatedDirectories.Add(chapter + ".Tests");

            var generatedTests = ListingManager.GenerateUnitTests(
                Path.Combine(Environment.CurrentDirectory, "Chapter01"));
            
            CollectionAssert.AreEquivalent((ICollection) expectedFiles, (ICollection) generatedTests);
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var cur in CreatedFiles)
            {
                try
                {
                    File.Delete(cur);
                }
                catch(Exception)
                {
                    // ignore
                }
            }

            foreach (var cur in CreatedDirectories)
            {
                try
                {
                    Directory.Delete(cur, true);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        private ICollection<string> ConvertFilenamesToFullPath(ICollection<string> fileNamesToConvert)
        {
            var fullPaths = new List<string>();

            foreach (string fileName in fileNamesToConvert)
            {
                fullPaths.Add(Path.Combine(Environment.CurrentDirectory, fileName));
            }

            return fullPaths;
        }

        private void WriteFile(string fileName, IEnumerable<string> toWrite = null)
        {
            FileInfo file = new FileInfo(fileName);
            file.Directory?.Create();
            
            File.WriteAllLines(fileName, toWrite ?? new List<string>());
            
            CreatedFiles.Add(fileName);
        }

        private void WriteFiles(IEnumerable<string> fileNames, IEnumerable<string> toWrite = null)
        {
            foreach (string file in fileNames)
            {
                WriteFile(file, toWrite);
            }
        }
    }
}