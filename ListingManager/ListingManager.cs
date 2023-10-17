using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace EssentialCSharp.ListingManager;

/// <summary>
/// A utility class providing means to rename listings, namespaces, and corresponding unit tests.
/// </summary>
public partial class ListingManager
{
    public IStorageManager StorageManager { get; }
    public bool ChapterOnly { get; }

    public ListingManager(string pathToChapter, bool chapterOnly = false)
    {
        StorageManager = Repository.IsValid(pathToChapter) ? new GitStorageManager(pathToChapter) : new OSStorageManager();
        ChapterOnly = chapterOnly;
    }

    public ListingManager(string pathToChapter, IStorageManager storageManager, bool chapterOnly = false)
    {
        StorageManager = storageManager;
        ChapterOnly = chapterOnly;
    }

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

    private static bool TryGetListing(string listingPath, out ListingInformation? listingData, bool isTest = false)
    {
        listingData = null;

        if (!ListingInformation.ApprovedFileTypes.Contains(Path.GetExtension(listingPath))) return false;

        try
        {
            listingData = new ListingInformation(listingPath, isTest);
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
    /// <param name="singleDir">Indicates whether the listing and test files are in a single directory under <paramref name="pathToChapter"/> (true) or if they are in separate dirs for listing and tests (false)</param>
    /// 
    public void UpdateChapterListingNumbers(string pathToChapter,
        bool verbose = false, bool preview = false, bool byFolder = false, bool singleDir = false)
    {
        List<ListingInformation> listingData = PopulateListingDataFromPath(pathToChapter, singleDir);
        //List<string> allListings = new();
        //List<ListingInformation> testListingData = new();
        //List<string> allTestListings = new();
        for (int i = 0, listingNumber = 1; i < listingData.Count; i++, listingNumber++)
        {
            //string cur = allListings[i];

            ListingInformation curListingData = listingData[i] ?? throw new InvalidOperationException($"Listing data is null for an index of {i}");

            if (!ChapterOnly && !byFolder && listingNumber == curListingData.OriginalListingNumber)
            {
                // TODO: redo renaming logic to handle using StorageManager.Move
                //File.Copy(curListingData.TemporaryPath, curListingData.Path, true);
                //.Move(curListingData.TemporaryPath, curListingData.Path);
                //if (testListingData.FirstOrDefault(x => x?.ListingNumber == curListingData.ListingNumber && x.ListingSuffix == curListingData.ListingSuffix) is ListingInformation currentTestListingData)
                //{
                    //StorageManager.Move(currentTestListingData.TemporaryPath, currentTestListingData.Path);
                //}
                continue;
            } //default

            if (!ChapterOnly)
            {
                curListingData.NewListingNumber = listingNumber;
                curListingData.NewListingNumberSuffix = string.Empty;
            }

            if (byFolder)
            {
                curListingData.NewChapterNumber = FileManager.GetFolderChapterNumber(pathToChapter);
            }

            string newNamespace = curListingData.GetNewNamespace(ChapterOnly);
            string newFileName = curListingData.GetNewFileName(ChapterOnly);

            Console.WriteLine($"Corrective action. {Path.GetFileName(curListingData.Path)} rename to {newFileName}");

            if (!preview) UpdateNamespaceOfPath(curListingData.Path, newNamespace, newFileName);

            if (listingData.Where(item => item.AssociatedTest is not null).Where(x => x?.OriginalListingNumber == curListingData.OriginalListingNumber && x.OriginalListingNumberSuffix == curListingData.OriginalListingNumberSuffix).FirstOrDefault() is ListingInformation curTestListingData)
            {
                if (verbose)
                {
                    Console.WriteLine($"Updating namespace for test {curTestListingData.OriginalChapterNumber}.{curTestListingData.OriginalListingNumber}");
                }
                if (!preview)
                {
                    Console.WriteLine($"Corrective action. {Path.GetFileName(curListingData.Path)} rename to {newFileName}");

                    if (!preview)
                    {
                        UpdateNamespaceOfPath(curListingData.Path, newNamespace, newFileName);
                        if (curListingData.AssociatedTest is not null)
                        {
                            UpdateNamespaceOfPath(curListingData.AssociatedTest.Path, curListingData.AssociatedTest.GetNewNamespace(ChapterOnly), curListingData.AssociatedTest.GetNewFileName(ChapterOnly));
                        }
                    }
                }
            }
        }

        MoveListing(listingData);
        //foreach (string path in allListings)
        //{
        //    File.Delete(path);
        //}
        //if (!singleDir)
        //{
        //    foreach (string path in allTestListings)
        //    {
        //        File.Delete(path);
        //    }
        //}
    }

    public void MoveListing(IEnumerable<ListingInformation> listingData)
    {
        foreach (ListingInformation listingInformation in listingData.OrderByDescending(x => x.NewListingNumber))
        {
            MoveListing(listingInformation);
        }
    }

    public void MoveListing(ListingInformation listingInformation)
    {
        if (listingInformation.Changed)
        {
            StorageManager.Move(listingInformation.Path, Path.Combine(listingInformation.ParentDir, listingInformation.GetNewFileName(ChapterOnly)));
            if (listingInformation.AssociatedTest is ListingInformation listingTest && listingTest is not null)
            {
                if (listingTest.Changed)
                {
                    StorageManager.Move(listingTest.Path, Path.Combine(listingTest.ParentDir, listingTest.GetNewFileName(ChapterOnly)));
                }
            }
        }
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

        using TextWriter textWriter = new StreamWriter(targetPath, true);
        foreach (string line in allLinesInFile)
        {
            if (line.StartsWith("namespace"))
            {
                if (line.TrimEnd().EndsWith(";"))
                {
                    textWriter.WriteLine("namespace " + newNamespace + ";");
                }
                else
                {
                    textWriter.WriteLine("namespace " + newNamespace);
                }
            }
            else
            {
                textWriter.WriteLine(line);
            }
        }
    }

    public static bool GetPathToAccompanyingUnitTest(string listingPath, out string pathToTest)
    {
        string testDirectory = $"{Path.GetDirectoryName(listingPath)}.Tests";

        Regex regex = TemporaryListingTestFile();

        Match fileNameMatch = regex.Match(listingPath);

        string testFileName = fileNameMatch.Success ? regex.Match(listingPath).Groups[1].Value : "";

        Regex pathToTestRegex =
            new(Regex.Escape($"{testDirectory}{Path.DirectorySeparatorChar}{testFileName}")
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

    public static List<ListingInformation> PopulateListingDataFromPath(string pathToChapter, bool singleDir)
    {
        List<ListingInformation> listingData = new();
        List<ListingInformation> testListingData = new();
        var listingFiles = FileManager.GetAllFilesAtPath(pathToChapter)
            .OrderBy(x => x).ToList();
        foreach (string fileName in listingFiles)
        {
            bool result = TryGetListing(fileName, out ListingInformation? data);
            if (result)
            {
                if (data is not null)
                {
                    if (data.IsTest)
                    {
                        testListingData.Add(data);
                    }
                    else
                    {
                        listingData.Add(data);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Listing data is unexpectedly null with a successful result");
                }
            }
        }

        if (!singleDir)
        {
            var listingTestFiles = FileManager.GetAllFilesAtPath($"{pathToChapter}.Tests")
    .OrderBy(x => x).ToList();
            foreach (string fileName in listingTestFiles)
            {
                bool result = TryGetListing(fileName, out ListingInformation? data, true);
                if (result)
                {
                    if (data is not null)
                    {
                        testListingData.Add(data);
                    }
                    else
                    {
                        throw new InvalidOperationException("Listing data is unexpectedly null with a successful result");
                    }
                }
            }
        }
        foreach (ListingInformation testListingInformation in testListingData)
        {
            ListingInformation listingInformation = listingData.Where(x => x.OriginalListingNumber == testListingInformation.OriginalListingNumber && x.OriginalChapterNumber == testListingInformation.OriginalChapterNumber && x.OriginalListingNumberSuffix == testListingInformation.OriginalListingNumberSuffix).First();
            if (testListingInformation.Caption.ToLowerInvariant() == "Tests".ToLowerInvariant() && listingInformation.Caption != string.Empty)
            {
                testListingInformation.Caption = listingInformation.Caption + ".Tests";
            }
            listingInformation.AssociatedTest = testListingInformation;
        }
        return listingData;
    }
    public static bool IsExtraListing(string path,
    string regexNamespace = @".*Listing\d{2}\.\d{2}(A|B|C|D).*\.cs$")
    {
        Regex fileNameRegex = new(regexNamespace);

        string directoryNameFull = Path.GetDirectoryName(path) ?? string.Empty;
        string directoryName = Path.GetFileName(directoryNameFull);

        return fileNameRegex.IsMatch(path) && !directoryName.Contains(".Tests");
    }

    [GeneratedRegex("((Listing\\d{2}\\.\\d{2})([A-Z]?)((\\.Tests)?)).*\\.cs.tmp$")]
    private static partial Regex TemporaryListingTestFile();
}