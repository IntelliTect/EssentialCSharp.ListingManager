namespace EssentialCSharp.ListingManager;

public static class ScanManager
{
    public static void ScanForMissingTests(DirectoryInfo pathToChapter, bool singleDir)
    {
        List<ListingInformation> listingData = ListingManagerHelpers.PopulateListingDataFromPath(pathToChapter.FullName, singleDir);
        foreach (ListingInformation listingInformation in listingData)
        {
            if (listingInformation.AssociatedTest is null)
            {
                Console.WriteLine($"Missing test for {listingInformation.OriginalChapterNumber}.{listingInformation.OriginalListingNumber}");
            }
        }
    }

    public static void ScanForAllMissingTests(DirectoryInfo pathToDirectoryOfChapters, bool singleDir)
    {
        IEnumerable<DirectoryInfo> directoryInfos = Directory.EnumerateDirectories(pathToDirectoryOfChapters.FullName)
    .Select(x => new DirectoryInfo(x))
    .Where(x => ListingManagerHelpers.ChapterDir().IsMatch(x.Name));

        foreach (DirectoryInfo directoryInfo in directoryInfos)
        {
            ScanForMissingTests(directoryInfo, singleDir);
        }
    }
}
