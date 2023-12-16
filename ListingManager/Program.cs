using System.CommandLine;

namespace EssentialCSharp.ListingManager;

public sealed class Program
{
    private static async Task<int> Main(string[] args)
    {
        CliConfiguration configuration = GetConfiguration();
        return await configuration.InvokeAsync(args);
    }

    public static CliConfiguration GetConfiguration()
    {
        // Use the ExistingOnly method to only parse the arguments that are defined in the configuration

        CliArgument<DirectoryInfo> directoryInArgument = new("directoryIn")
        {
            Description = "The directory of the chapter to update listings on.",
        };
        directoryInArgument.AcceptExistingOnly();

        // With proper logging implemented, this option will hopefully be removed
        CliOption<bool> verboseOption = new("--verbose")
        {
            Description = "Displays more detailed messages in the log.",
        };

        CliOption<bool> previewOption = new("--preview")
        {
            Description = "Displays the changes that will be made without actually making them.",
        };

        CliOption<bool> allChaptersOption = new("--all-chapters")
        {
            Description = "The passed in path is the parent directory to many chapter directories rather than a single chapter directory.",
        };

        // TODO: Add better descriptions when their functionality becomes clearer
        CliOption<bool> byFolderOption = new("--by-folder")
        {
            Description = "Updates namespaces and filenames for all listings and accompanying tests within a folder.",
        };

        CliOption<bool> singleDirOption = new("--single-dir")
        {
            Description = "All listings are in a single directory and not separated into chapter and chapter test directories.",
        };

        CliCommand listingUpdating = new("update", "Updates namespaces and filenames for all listings and accompanying tests within a chapter")
        {
            directoryInArgument,
            verboseOption,
            previewOption,
            byFolderOption,
            singleDirOption,
            allChaptersOption
        };

        listingUpdating.SetAction((ParseResult parseResult) =>
        {
            DirectoryInfo directoryIn = parseResult.CommandResult.GetValue(directoryInArgument)!;
            bool verbose = parseResult.CommandResult.GetValue(verboseOption);
            bool preview = parseResult.CommandResult.GetValue(previewOption);
            bool byFolder = parseResult.CommandResult.GetValue(byFolderOption);
            bool singleDir = parseResult.CommandResult.GetValue(singleDirOption);
            bool allChapters = parseResult.CommandResult.GetValue(allChaptersOption);

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
        });

        CliCommand scan = new("scan", "Scans for various things");

        CliCommand listings = new("listings", "Scans for mismatched listings")
        {
            directoryInArgument
        };

        listings.SetAction((ParseResult parseResult) =>
        {
            DirectoryInfo directoryIn = parseResult.CommandResult.GetValue(directoryInArgument)!;
            var extraListings = ListingManager.GetAllExtraListings(directoryIn.FullName).OrderBy(x => x);

            Console.WriteLine("---Extra Listings---");
            foreach (string extraListing in extraListings)
            {
                Console.WriteLine(extraListing);
            }
        });
        scan.Subcommands.Add(listings);

        CliCommand tests = new("tests", "Scans for mismatched tests")
        {
            directoryInArgument,
            allChaptersOption,
            singleDirOption
        };

        tests.SetAction((ParseResult parseResult) =>
        {
            DirectoryInfo directoryIn = parseResult.CommandResult.GetValue(directoryInArgument)!;
            bool allChapters = parseResult.CommandResult.GetValue(allChaptersOption);
            bool singleDir = parseResult.CommandResult.GetValue(singleDirOption);

            Console.WriteLine("---Missing Tests---");
            if (allChapters)
            {
                ScanManager.ScanForAllMissingTests(directoryIn, singleDir);
            }
            else
            {
                ScanManager.ScanForMissingTests(directoryIn, singleDir);
            }
        });

        scan.Subcommands.Add(tests);

        CliRootCommand rootCommand = new("The EssentialCSharp.ListingManager helps to organize and manage the EssentialCSharp source code")
        {
            listingUpdating,
            scan
        };

        return new CliConfiguration(rootCommand);
    }
}
