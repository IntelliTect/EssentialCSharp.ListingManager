using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ListingManager
{
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

        private static bool TryGetListing(string listingPath, out ListingInformation listingData)
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

        public static void UpdateChapterListingNumbers(string pathToChapter,
            bool verboseMode = false, bool preview = false, bool changeChapterNumberBasedOnFolderName = false, bool onlyChangeChapterNumber = false)
        {
            var listingData = new List<ListingInformation>();

            int FolderChapterNumber = FileManager.GetFolderChapterNumber(pathToChapter);


            List<string> allListings = FileManager.GetAllFilesAtPath(pathToChapter)
                .OrderBy(x => x)
                .Where(x =>
                {
                    bool result = TryGetListing(x, out var data);
                    if (result) listingData.Add(data);
                    return result;
                }).ToList();

            for (int i = 0, listingNumber = 1; i < allListings.Count; i++, listingNumber++)
            {
                string cur = allListings[i];

                var curListingData = listingData[i];

                if (listingNumber == curListingData.ListingNumber && onlyChangeChapterNumber == false)
                {
                    continue;
                }

                string completeListingNumber = listingNumber + ""; //default
                int listingChapterNumber = curListingData.ChapterNumber; //default

                if (onlyChangeChapterNumber)
                {
                    completeListingNumber = curListingData.ListingNumber + curListingData.ListingSuffix;
                }

                if (changeChapterNumberBasedOnFolderName)
                {
                    listingChapterNumber = FolderChapterNumber;
                }


                UpdateListingNamespace(cur, listingChapterNumber,
                    completeListingNumber,
                    curListingData.ListingDescription, verboseMode, preview);

                if (GetPathToAccompanyingUnitTest(cur, out string pathToTest))
                {
                    Console.Write("Updating test. ");
                    UpdateListingNamespace(pathToTest, listingChapterNumber,
                        completeListingNumber,
                        curListingData.ListingDescription, verboseMode, preview);
                }




            }
        }

        public static bool IsExtraListing(string path,
            string regexNamespace = @".*Listing\d{2}\.\d{2}(A|B|C|D).*\.cs$")
        {
            Regex fileNameRegex = new Regex(regexNamespace);

            string directoryNameFull = Path.GetDirectoryName(path);

            string directoryName = Path.GetFileName(directoryNameFull);

            return fileNameRegex.IsMatch(path) && !directoryName.Contains(".Tests");
        }

        public static void UpdateListingNamespace(string path, int chapterNumber, string listingNumber,
            string listingData, bool verbose = false, bool preview = false)
        {
            string paddedChapterNumber = chapterNumber.ToString("00");
            string paddedListingNumber = listingNumber.PadLeft(2, '0');

            string newFileNameTemplate = "Listing{0}.{1}{2}.cs";
            string newNamespace = "AddisonWesley.Michaelis.EssentialCSharp" +
                                  $".Chapter{paddedChapterNumber}" +
                                  $".Listing{paddedChapterNumber}_" +
                                  paddedListingNumber;
            string newFileName = string.Format(newFileNameTemplate,
                paddedChapterNumber,
                paddedListingNumber,
                string.IsNullOrWhiteSpace(listingData) ? "" : $".{listingData}");

            if (verbose)
            {
                Console.WriteLine($"Corrective action. {Path.GetFileName(path)} rename to {newFileName}");
            }

            if (!preview) UpdateNamespaceOfPath(path, newNamespace, newFileName);
        }

        public static bool UpdateNamespaceOfPath(string path, string newNamespace, string newFileName = "")
        {
            if (Path.GetExtension(path) != ".cs")
            {
                return false;
            }

            // read file into memory
            string[] allLinesInFile = File.ReadAllLines(path);

            File.Delete(path);

            string targetPath = Path.Combine(Path.GetDirectoryName(path), newFileName) ?? path;

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

            return true;
        }

        public static bool GetPathToAccompanyingUnitTest(string listingPath, out string pathToTest)
        {
            string testDirectory = $"{Path.GetDirectoryName(listingPath)}.Tests";

            Regex regex = new Regex(@"((Listing\d{2}\.\d{2})([A-Z]?)((\.Tests)?)).*\.cs$");

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

        public static string GetTestLayout(string chapterNumber, string listingNumber)
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

        public static IEnumerable<string> GetAllMissingUnitTests(string pathToChapter)
        {
            foreach (string file in FileManager.GetAllFilesAtPath(pathToChapter).OrderBy(x => x))
            {
                if (!GetPathToAccompanyingUnitTest(file, out string testFileName))
                {
                    yield return testFileName;
                }
            }
        }

        public static ICollection<string> GenerateUnitTests(string pathToChapter, Func<string, bool> action = null,
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

        public static bool GenerateUnitTest(string pathToTest)
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
