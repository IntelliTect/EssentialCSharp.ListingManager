using System.CommandLine;

namespace EssentialCSharp.ListingManager;

public sealed class Program
{
    private static async Task<int> Main(string[] args)
    {
        RootCommand rootCommand = GetRootCommand();
        return await rootCommand.Parse(args).InvokeAsync();
    }

    public static RootCommand GetRootCommand()
    {
        // Use the ExistingOnly method to only parse the arguments that are defined in the configuration

        Argument<DirectoryInfo> directoryInArgument = new("directoryIn")
        {
            Description = "The directory of the chapter to update listings on.",
        };
        directoryInArgument.AcceptExistingOnly();

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

        Option<bool> gitOption = new("--git")
        {
            Description = "Use git mv for moving files instead of OS file operations. Requires the directory to be a valid git repository.",
        };

        Command listingUpdating = new("update", "Updates namespaces and filenames for all listings and accompanying tests within a chapter");
        listingUpdating.Arguments.Add(directoryInArgument);
        listingUpdating.Options.Add(verboseOption);
        listingUpdating.Options.Add(previewOption);
        listingUpdating.Options.Add(byFolderOption);
        listingUpdating.Options.Add(singleDirOption);
        listingUpdating.Options.Add(allChaptersOption);
        listingUpdating.Options.Add(gitOption);

        listingUpdating.SetAction((ParseResult parseResult) =>
        {
            DirectoryInfo directoryIn = parseResult.GetValue(directoryInArgument)!;
            bool verbose = parseResult.GetValue(verboseOption);
            bool preview = parseResult.GetValue(previewOption);
            bool byFolder = parseResult.GetValue(byFolderOption);
            bool singleDir = parseResult.GetValue(singleDirOption);
            bool allChapters = parseResult.GetValue(allChaptersOption);
            bool useGit = parseResult.GetValue(gitOption);

            Console.WriteLine($"Updating listings within: {directoryIn}");
            ListingManager listingManager = new(directoryIn, useGit);
            if (allChapters)
            {
                listingManager.UpdateAllChapterListingNumbers(directoryIn, verbose, preview, byFolder, singleDir);
            }
            else
            {
                listingManager.UpdateChapterListingNumbers(directoryIn, verbose, preview, byFolder, singleDir);
            }
        });

        Command scan = new("scan", "Scans for various things");

        Command listings = new("listings", "Scans for mismatched listings");
        listings.Arguments.Add(directoryInArgument);

        listings.SetAction((ParseResult parseResult) =>
        {
            DirectoryInfo directoryIn = parseResult.GetValue(directoryInArgument)!;
            var extraListings = ListingManager.GetAllExtraListings(directoryIn.FullName).OrderBy(x => x);

            Console.WriteLine("---Extra Listings---");
            foreach (string extraListing in extraListings)
            {
                Console.WriteLine(extraListing);
            }
        });
        
        scan.Subcommands.Add(listings);

        Command tests = new("tests", "Scans for mismatched tests");
        tests.Arguments.Add(directoryInArgument);
        tests.Options.Add(allChaptersOption);
        tests.Options.Add(singleDirOption);

        tests.SetAction((ParseResult parseResult) =>
        {
            DirectoryInfo directoryIn = parseResult.GetValue(directoryInArgument)!;
            bool allChapters = parseResult.GetValue(allChaptersOption);
            bool singleDir = parseResult.GetValue(singleDirOption);

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

        RootCommand rootCommand = new("The EssentialCSharp.ListingManager helps to organize and manage the EssentialCSharp source code");
        rootCommand.Subcommands.Add(listingUpdating);
        rootCommand.Subcommands.Add(scan);

        return rootCommand;
    }
}