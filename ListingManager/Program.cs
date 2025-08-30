using System.CommandLine;

namespace EssentialCSharp.ListingManager;

public sealed class Program
{
    private static Task<int> Main(string[] args)
    {
        RootCommand rootCommand = GetConfiguration();
        return rootCommand.InvokeAsync(args);
    }

    public static RootCommand GetConfiguration()
    {
        // Use the ExistingOnly method to only parse the arguments that are defined in the configuration

        Argument<DirectoryInfo> directoryInArgument = new("directoryIn")
        {
            Description = "The directory of the chapter to update listings on.",
        };
        directoryInArgument.ExistingOnly();

        // With proper logging implemented, this option will hopefully be removed
        Option<bool> verboseOption = new("--verbose")
        {
            Description = "Displays more detailed messages in the log.",
        };

        Option<bool> previewOption = new("--preview")
        {
            Description = "Displays the changes that will be made without actually making them.",
        };

        Option<bool> allChaptersOption = new("--all-chapters")
        {
            Description = "The passed in path is the parent directory to many chapter directories rather than a single chapter directory.",
        };

        // TODO: Add better descriptions when their functionality becomes clearer
        Option<bool> byFolderOption = new("--by-folder")
        {
            Description = "Updates namespaces and filenames for all listings and accompanying tests within a folder.",
        };

        Option<bool> singleDirOption = new("--single-dir")
        {
            Description = "All listings are in a single directory and not separated into chapter and chapter test directories.",
        };

        Command listingUpdating = new("update", "Updates namespaces and filenames for all listings and accompanying tests within a chapter")
        {
            directoryInArgument,
            verboseOption,
            previewOption,
            byFolderOption,
            singleDirOption,
            allChaptersOption
        };

        listingUpdating.SetHandler((DirectoryInfo directoryIn, bool verbose, bool preview, bool byFolder, bool singleDir, bool allChapters) =>
        {
            Console.WriteLine($"Updating listings within: {directoryIn}");
            ListingManager listingManager = new(directoryIn);
            if (allChapters)
            {
                listingManager.UpdateAllChapterListingNumbers(directoryIn, verbose, preview, byFolder, singleDir);
            }
            else
            {
                listingManager.UpdateChapterListingNumbers(directoryIn, verbose, preview, byFolder, singleDir);
            }
        }, directoryInArgument, verboseOption, previewOption, byFolderOption, singleDirOption, allChaptersOption);

        Command listings = new("listings", "Scans for mismatched listings")
        {
            directoryInArgument
        };

        listings.SetHandler((DirectoryInfo directoryIn) =>
        {
            var extraListings = ListingManager.GetAllExtraListings(directoryIn.FullName).OrderBy(x => x);

            Console.WriteLine("---Extra Listings---");
            foreach (string extraListing in extraListings)
            {
                Console.WriteLine(extraListing);
            }
        }, directoryInArgument);

        Command tests = new("tests", "Scans for mismatched tests")
        {
            directoryInArgument,
            allChaptersOption,
            singleDirOption
        };

        tests.SetHandler((DirectoryInfo directoryIn, bool allChapters, bool singleDir) =>
        {
            Console.WriteLine("---Missing Tests---");
            if (allChapters)
            {
                ScanManager.ScanForAllMissingTests(directoryIn, singleDir);
            }
            else
            {
                ScanManager.ScanForMissingTests(directoryIn, singleDir);
            }
        }, directoryInArgument, allChaptersOption, singleDirOption);

        Command scan = new("scan", "Scans for various things")
        {
            listings,
            tests
        };

        RootCommand rootCommand = new("The EssentialCSharp.ListingManager helps to organize and manage the EssentialCSharp source code")
        {
            listingUpdating,
            scan
        };

        return rootCommand;
    }
}
