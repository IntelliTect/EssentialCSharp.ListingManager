using LibGit2Sharp;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace EssentialCSharp.ListingManager;

/// <summary>
/// A utility class providing means to rename listings, namespaces, and corresponding unit tests.
/// </summary>
public partial class ListingManager
{
    public IStorageManager StorageManager { get; }

    public ListingManager(DirectoryInfo pathToChapter)
    {
        StorageManager = Repository.IsValid(pathToChapter.FullName) ? new GitStorageManager(pathToChapter.FullName) : new OSStorageManager();
    }

    public ListingManager(DirectoryInfo pathToChapter, bool useGit)
    {
        if (useGit)
        {
            if (!Repository.IsValid(pathToChapter.FullName))
            {
                throw new InvalidOperationException($"The --git option was specified, but the directory '{pathToChapter.FullName}' is not a valid git repository.");
            }
            StorageManager = new GitStorageManager(pathToChapter.FullName);
        }
        else
        {
            StorageManager = new OSStorageManager();
        }
    }

    public ListingManager(DirectoryInfo pathToChapter, IStorageManager storageManager)
        : this(pathToChapter)
    {
        StorageManager = storageManager;
    }

    public static IEnumerable<string> GetAllExtraListings(string pathToStartFrom)
    {
        foreach (string file in FileManager.GetAllFilesAtPath(pathToStartFrom, true))
        {
            if (ListingManagerHelpers.IsExtraListing(file))
            {
                yield return file;
            }
        }
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
    public void UpdateChapterListingNumbers(DirectoryInfo pathToChapter,
        bool verbose = false, bool preview = false, bool byFolder = false, bool singleDir = false)
    {
        List<ListingInformation> listingData = ListingManagerHelpers.PopulateListingDataFromPath(pathToChapter.FullName, singleDir);
        for (int i = 0, listingNumber = 1; i < listingData.Count; i++, listingNumber++)
        {
            ListingInformation curListingData = listingData[i] ?? throw new InvalidOperationException($"Listing data is null for an index of {i}");
            curListingData.NewListingNumber = listingNumber;
            curListingData.NewListingNumberSuffix = string.Empty;

            if (byFolder)
            {
                curListingData.NewChapterNumber = FileManager.GetFolderChapterNumber(pathToChapter.FullName);
            }

            string newNamespace = curListingData.GetNewNamespace();
            string newFileName = curListingData.GetNewFileName();

            Console.WriteLine($"Corrective action. {Path.GetFileName(curListingData.Path)} rename to {newFileName}");
            curListingData.UpdateNamespaceInFileContents();

            if (listingData.Where(item => item.AssociatedTest is not null).FirstOrDefault(x => x?.OriginalListingNumber == curListingData.OriginalListingNumber && x.OriginalListingNumberSuffix == curListingData.OriginalListingNumberSuffix) is ListingInformation curTestListingData)
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
                        curListingData.AssociatedTest?.UpdateNamespaceInFileContents();
                    }
                }
            }
        }

        listingData.ForEach(item => item.UpdateReferencesInFileAndTest(listingData));
        MoveListing(listingData);
        UpdateFileContents(listingData);
    }

    private void UpdateFileContents(IEnumerable<ListingInformation> listingData)
    {
        foreach (ListingInformation listingInformation in listingData)
        {
            UpdateFileContents(listingInformation);
        }
    }

    private void UpdateFileContents(ListingInformation listingInformation)
    {
        if (listingInformation.FileContentsChanged)
        {
            File.WriteAllLines(Path.Combine(listingInformation.ParentDir, listingInformation.Path), listingInformation.FileContents);

            if (listingInformation.AssociatedTest is ListingInformation listingTest && listingTest.Changed)
            {
                File.WriteAllLines(Path.Combine(listingTest.ParentDir, listingTest.Path), listingTest.FileContents);
            }
        }
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
            string listingInformationFileName = listingInformation.GetNewFileName();
            StorageManager.Move(listingInformation.Path, Path.Combine(listingInformation.ParentDir, listingInformationFileName));
            listingInformation.Path = listingInformationFileName;

            if (listingInformation.AssociatedTest is ListingInformation listingTest && listingTest.Changed)
            {
                string listingTestInformationFileName = listingTest.GetNewFileName();
                StorageManager.Move(listingTest.Path, Path.Combine(listingTest.ParentDir, listingTestInformationFileName));
                listingTest.Path = listingTestInformationFileName;
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

    [GeneratedRegex("((Listing\\d{2}\\.\\d{2})([A-Z]?)((\\.Tests)?)).*\\.cs.tmp$")]
    private static partial Regex TemporaryListingTestFile();

    public void UpdateAllChapterListingNumbers(DirectoryInfo pathToDirectoryOfChapters,
        bool verbose = false, bool preview = false, bool byFolder = false, bool singleDir = false)
    {
        IEnumerable<DirectoryInfo> directoryInfos = Directory.EnumerateDirectories(pathToDirectoryOfChapters.FullName)
            .Select(x => new DirectoryInfo(x))
            .Where(x => ListingManagerHelpers.ChapterDir().IsMatch(x.Name));

        foreach (DirectoryInfo directoryInfo in directoryInfos)
        {
            UpdateChapterListingNumbers(directoryInfo, verbose, preview, byFolder, singleDir);
        }
    }
}