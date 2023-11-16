using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace EssentialCSharp.ListingManager;

public static partial class ListingManagerHelpers
{
    private static bool TryGetListing(string listingPath, [NotNullWhen(true)] out ListingInformation? listingData)
    {
        return TryGetListing(listingPath, out listingData, false);
    }

    private static bool TryGetListing(string listingPath, [NotNullWhen(true)] out ListingInformation? listingData, bool isTest)
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
    public static bool IsExtraListing(string path, string regexNamespace = @".*Listing\d{2}\.\d{2}(A|B|C|D).*\.cs$")
    {
        Regex fileNameRegex = new(regexNamespace);

        string directoryNameFull = Path.GetDirectoryName(path) ?? string.Empty;
        string directoryName = Path.GetFileName(directoryNameFull);

        return fileNameRegex.IsMatch(path) && !directoryName.Contains(".Tests");
    }

    public static List<ListingInformation> PopulateListingDataFromPath(string pathToChapter, bool singleDir)
    {
        List<ListingInformation> listingData = new();
        List<ListingInformation> testListingData = new();
        var listingFiles = FileManager.GetAllFilesAtPath(pathToChapter)
            .OrderBy(x => x);
        foreach (string fileName in listingFiles)
        {
            if (TryGetListing(fileName, out ListingInformation? data))
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
        }

        if (!singleDir)
        {
            var listingTestFiles = FileManager.GetAllFilesAtPath($"{pathToChapter}.Tests")
    .OrderBy(x => x);
            foreach (string fileName in listingTestFiles)
            {
                if (TryGetListing(fileName, out ListingInformation? data, true))
                {
                    testListingData.Add(data);
                }
            }
        }

        foreach (ListingInformation testListingInformation in testListingData)
        {
            ListingInformation listingInformation = listingData.First(x => x.OriginalListingNumber == testListingInformation.OriginalListingNumber
                                                                        && x.OriginalChapterNumber == testListingInformation.OriginalChapterNumber
                                                                        && x.OriginalListingNumberSuffix == testListingInformation.OriginalListingNumberSuffix);

            if (string.Equals(testListingInformation.Caption, "Tests", StringComparison.InvariantCultureIgnoreCase) && listingInformation.Caption != string.Empty)
            {
                testListingInformation.Caption = listingInformation.Caption + ".Tests";
            }
            listingInformation.AssociatedTest = testListingInformation;
        }

        return listingData;
    }

    [GeneratedRegex("^Chapter\\d{2}$")]
    public static partial Regex ChapterDir();
}