using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ListingManager
{
    /// <summary>
    /// A utility class providing means to rename listings, namespaces, and corresponding unit tests.
    /// </summary>
    public static class ListingManager
    {
        public static IEnumerable<string> GetAllExtraListings(string pathToStartFrom)
        {
            foreach (string file in FileManager.GetAllFilesAtPath(pathToStartFrom, true))
            {
                if (IsExtraListing(file))
                {
                    yield return file;
                }
            }
        }

        private static bool TryGetListing(string listingPath, out ListingInformation? listingData)
        {
            listingData = null;

            if (Path.GetExtension(listingPath) != ".cs") return false;

            try
            {
                listingData = new ListingInformation(listingPath);
            }
            catch (Exception) // don't care about the type of exception here. If things didn't go perfectly, abort
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the namespace, file names, and corresponding test file of the target listing. This has a cascading
        /// effect, resulting in the renaming of subsequent listings in the same chapter.
        /// </summary>
        /// <param name="pathToChapter">Path to the target chapter</param>
        /// <param name="verbose">When true, enables verbose console output</param>
        /// <param name="preview">When true, leaves files in place and only print console output</param>
        /// <param name="byFolder">Changes a listing's chapter based on the chapter number in the chapter's path</param>
        /// <param name="chapterOnly">Changes only the chapter of the listing, leaving the listing number unchanged. Use with <paramref name="byFolder"/></param>
        public static void UpdateChapterListingNumbers(string pathToChapter,
            bool verbose = false, bool preview = false, bool byFolder = false, bool chapterOnly = false)
        {
            var listingData = new List<ListingInformation?>();
            List<string> allListings = FileManager.GetAllFilesAtPath(pathToChapter)
                .OrderBy(x => x)
                .Where(x =>
                {
                    bool result = TryGetListing(x, out var data);
                    if (result) listingData.Add(data);
                    return result;
                }).ToList();
            foreach (string path in allListings)
            {
                File.Copy(path, $"{path}{ListingInformation.TemporaryExtension}", true);
                File.Delete(path);
            }
            allListings = FileManager.GetAllFilesAtPath(pathToChapter)
                .OrderBy(x => x)
                .Where(x => Path.GetExtension(x) == ListingInformation.TemporaryExtension).ToList();

            var testListingData = new List<ListingInformation?>();
            List<string> allTestListings = FileManager.GetAllFilesAtPath($"{pathToChapter}.Tests")
                .OrderBy(x => x)
                .Where(x =>
                {
                    bool result = TryGetListing(x, out var data);
                    if (result) testListingData.Add(data);
                    return result;
                }).ToList();
            foreach (string path in allTestListings)
            {
                File.Copy(path, $"{path}{ListingInformation.TemporaryExtension}", true);
                File.Delete(path);
            }
            allTestListings = FileManager.GetAllFilesAtPath($"{pathToChapter}.Tests")
                .OrderBy(x => x)
                .Where(x => Path.GetExtension(x) == ListingInformation.TemporaryExtension).ToList();

            for (int i = 0, listingNumber = 1; i < allListings.Count; i++, listingNumber++)
            {
                if (allListings.Count != listingData.Count)
                {
                    throw new InvalidOperationException($"The number of listing data and allListings doesn't match, possibly {ListingInformation.TemporaryExtension} files exist in your directory already.");
                }
                string cur = allListings[i];

                ListingInformation curListingData = listingData[i] ?? throw new InvalidOperationException($"Listing data is null for an index of {i}");


                if (curListingData is null || !chapterOnly && !byFolder && listingNumber == curListingData.ListingNumber) { 
                    File.Copy(curListingData?.TemporaryPath, curListingData?.Path, true);
                    if (testListingData.Where(x => x?.ListingNumber == curListingData.ListingNumber && x.ListingSuffix == curListingData.ListingSuffix).FirstOrDefault() is ListingInformation currentTestListingData)
                    {
                        File.Copy(currentTestListingData?.TemporaryPath, currentTestListingData?.Path, true);
                    }
                    continue;
                } //default

                string completeListingNumber = listingNumber + ""; //default
                int listingChapterNumber = curListingData.ChapterNumber; //default

                if (chapterOnly)
                {
                    completeListingNumber = curListingData.ListingNumber + curListingData.ListingSuffix + "";

                }

                if (byFolder)
                {
                    listingChapterNumber = FileManager.GetFolderChapterNumber(pathToChapter);
                }

                UpdateListingNamespace(cur, listingChapterNumber,
                    completeListingNumber,
                    curListingData.ListingDescription, curListingData, verbose, preview);

                if (testListingData.Where(x => x?.ListingNumber == curListingData.ListingNumber && x.ListingSuffix == curListingData.ListingSuffix).FirstOrDefault() is ListingInformation curTestListingData)
                {
                    Console.Write("Updating test. ");
                    UpdateListingNamespace(curTestListingData.TemporaryPath, listingChapterNumber,
                        completeListingNumber,
                        string.IsNullOrEmpty(curListingData.ListingDescription) ? "Tests" : curListingData.ListingDescription + ".Tests", curListingData, verbose, preview);
                }

            }
            foreach (string path in allListings)
            {
                File.Delete(path);
            }
            foreach (string path in allTestListings)
            {
                File.Delete(path);
            }
        }

        public static bool IsExtraListing(string path,
            string regexNamespace = @".*Listing\d{2}\.\d{2}(A|B|C|D).*\.cs$")
        {
            Regex fileNameRegex = new Regex(regexNamespace);

            string directoryNameFull = Path.GetDirectoryName(path) ?? string.Empty;
            string directoryName = Path.GetFileName(directoryNameFull);

            return fileNameRegex.IsMatch(path) && !directoryName.Contains(".Tests");
        }

        /// <summary>
        /// Updates the namespace and file name of the listing at <paramref name="path"/>
        /// </summary>
        /// <param name="path">The path to the target listing</param>
        /// <param name="chapterNumber">The chapter the listing belongs to</param>
        /// <param name="listingNumber">The updated listing number</param>
        /// <param name="listingData">The name of the listing to be included in the namespace/path</param>
        /// <param name="curListingData"></param>
        /// <param name="verbose">When true, enables verbose console output</param>
        /// <param name="preview">When true, leaves files in place and only print console output</param>
        private static void UpdateTestListingNamespace(string path, int chapterNumber, string listingNumber,
            string listingData, ListingInformation curListingData, bool verbose = false, bool preview = false)
        {
            string paddedChapterNumber = chapterNumber.ToString("00");

            string regexSingleDigitListingWithSuffix = @"\d{1}[A-Za-z]";
            string paddedListingNumber = "";
            if (Regex.IsMatch(listingNumber, regexSingleDigitListingWithSuffix))
            { //allows for keeping the original listing number with a suffix. e.g. "01A"   
                paddedListingNumber = listingNumber.PadLeft(3, '0');
            }
            else
            {
                paddedListingNumber = listingNumber.PadLeft(2, '0'); //default
            }

            string newFileNameTemplate = "Listing{0}.{1}{2}.cs";
            string newNamespace = "AddisonWesley.Michaelis.EssentialCSharp" +
                                  $".Chapter{paddedChapterNumber}" +
                                  $".Listing{paddedChapterNumber}_" +
                                  $"{paddedListingNumber}.Tests";
            string newFileName = string.Format(newFileNameTemplate,
                paddedChapterNumber,
                paddedListingNumber,
                string.IsNullOrWhiteSpace(listingData) || string.IsNullOrEmpty(listingData) ? "" : $".{listingData}");

            if (verbose)
            {
                Console.WriteLine($"Corrective action. {Path.GetFileName(path)} rename to {newFileName}");
            }

            if (!preview)
            {
                UpdateNamespaceOfPath(path, newNamespace, newFileName);
            }
        }

        /// <summary>
        /// Updates the namespace and file name of the listing at <paramref name="path"/>
        /// </summary>
        /// <param name="path">The path to the target listing</param>
        /// <param name="chapterNumber">The chapter the listing belongs to</param>
        /// <param name="listingNumber">The updated listing number</param>
        /// <param name="listingData">The name of the listing to be included in the namespace/path</param>
        /// <param name="verbose">When true, enables verbose console output</param>
        /// <param name="preview">When true, leaves files in place and only print console output</param>
        /// <param name="curListingData">An instance of ListingInformation for the current listing.</param>
        private static void UpdateListingNamespace(string path, int chapterNumber, string listingNumber,
            string listingData, ListingInformation curListingData, bool verbose = false, bool preview = false)
        {
            string paddedChapterNumber = chapterNumber.ToString("00");

            const string regexSingleDigitListingWithSuffix = @"\d{1}[A-Za-z]";
            string paddedListingNumber;
            if (Regex.IsMatch(listingNumber, regexSingleDigitListingWithSuffix))
            {
            //allows for keeping the original listing number with a suffix. e.g. "01A"   
                paddedListingNumber = listingNumber.PadLeft(3, '0');
            }
            else
            {
                paddedListingNumber = listingNumber.PadLeft(2, '0'); //default
            }

            string newFileNameTemplate = "Listing{0}.{1}{2}.cs";
            string newNamespace = "AddisonWesley.Michaelis.EssentialCSharp" +
                                  $".Chapter{paddedChapterNumber}" +
                                  $".Listing{paddedChapterNumber}_" +
                                  paddedListingNumber;
            string newFileName = string.Format(newFileNameTemplate,
                paddedChapterNumber,
                paddedListingNumber,
                string.IsNullOrWhiteSpace(listingData) || string.IsNullOrEmpty(listingData) ? "" : $".{listingData}");

            if (verbose)
            {
                Console.WriteLine($"Corrective action. {Path.GetFileName(path)} rename to {newFileName}");
            }

            if (!preview) UpdateNamespaceOfPath(path, newNamespace, newFileName);
        }

        private static void UpdateNamespaceOfPath(string path, string newNamespace, string newFileName = "")
        {
            if (Path.GetExtension(path) != ".tmp")
            {
                return;
            }

            // read file into memory
            string[] allLinesInFile = File.ReadAllLines(path);

            string targetPath = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, newFileName) ?? path;

            using (TextWriter textWriter = new StreamWriter(targetPath, true))
            {
                foreach (string line in allLinesInFile)
                {
                    if (line.StartsWith("namespace"))
                    {
                        textWriter.WriteLine("namespace " + newNamespace);
                    }
                    else
                    {
                        textWriter.WriteLine(line);
                    }
                }
            }
        }

        public static bool GetPathToAccompanyingUnitTest(string listingPath, out string pathToTest)
        {
            string testDirectory = $"{Path.GetDirectoryName(listingPath)}.Tests";

            Regex regex = new Regex(@"((Listing\d{2}\.\d{2})([A-Z]?)((\.Tests)?)).*\.cs.tmp$");

            Match fileNameMatch = regex.Match(listingPath);

            string testFileName = fileNameMatch.Success ? regex.Match(listingPath).Groups[1].Value : "";

            Regex pathToTestRegex =
                new Regex(Regex.Escape($"{testDirectory}{Path.DirectorySeparatorChar}{testFileName}")
                          + @".*\.cs");

            if (Directory.Exists(testDirectory))
            {
                foreach (var s in FileManager.GetAllFilesAtPath(testDirectory))
                {
                    if (pathToTestRegex.IsMatch(s))
                    {
                        pathToTest = s;
                        return true;
                    }
                }
            }

            pathToTest = $"{testDirectory}{Path.DirectorySeparatorChar}{Path.GetFileName(listingPath)}";

            return false;
        }

        private static string GetTestLayout(string chapterNumber, string listingNumber)
        {
            return string.Format(TestHeaderLayout,
                       chapterNumber.PadLeft(2, '0'),
                       listingNumber.PadLeft(2, '0')) + TestBodyLayout;
        }

        private static string TestHeaderLayout =
@"using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AddisonWesley.Michaelis.EssentialCSharp.Chapter{0}.Listing{0}_{1}.Tests";

        private static string TestBodyLayout =
@"
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void UnitTest1()
        {
            Assert.Fail();
        }
    }
}";

        private static IEnumerable<string> GetAllMissingUnitTests(string pathToChapter)
        {
            foreach (string file in FileManager.GetAllFilesAtPath(pathToChapter).OrderBy(x => x))
            {
                if (!GetPathToAccompanyingUnitTest(file, out string testFileName))
                {
                    yield return testFileName;
                }
            }
        }

        public static ICollection<string> GenerateUnitTests(string pathToChapter, Func<string, bool>? action = null,
            bool verbose = false)
        {
            var toReturn = new List<string>();

            var missingTests = GetAllMissingUnitTests(pathToChapter);

            foreach (string missingTestName in missingTests)
            {
                if (GenerateUnitTest(missingTestName))
                {
                    toReturn.Add(missingTestName);
                    if (verbose)
                    {
                        Console.WriteLine($"Test generated: {missingTestName}");
                    }
                }

                if (action == null) continue;
                bool shouldContinue = action.Invoke(missingTestName);

                if (!shouldContinue)
                {
                    break;
                }
            }

            return toReturn;
        }

        private static bool GenerateUnitTest(string pathToTest)
        {
            if (File.Exists(pathToTest))
            {
                return false;
            }

            Regex getListingData = new Regex(@"Listing(\d{2})\.(\d{2})");

            var match = getListingData.Match(pathToTest);

            string chapterNumber = match.Groups[1].Value;
            string listingNumber = match.Groups[2].Value;

            string testDirectory = Path.GetDirectoryName(pathToTest);

            Directory.CreateDirectory(testDirectory);

            using (var writer = File.CreateText(pathToTest))
            {
                writer.WriteLine(GetTestLayout(chapterNumber, listingNumber));
            }

            return true;
        }

        public static string ExecuteBashCommand(string command)
        {
            // according to: https://stackoverflow.com/a/15262019/637142
            // thanks to this we will pass everything as one command
            command = command.Replace("\"", "\"\"");

            string fileName = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "CMD.exe"
                : "/bin/bash";

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = "-c \"" + command + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();

            return proc.StandardOutput.ReadToEnd();
        }
    }
}
