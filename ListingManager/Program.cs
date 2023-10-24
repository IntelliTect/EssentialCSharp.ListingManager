using System.CommandLine;

namespace EssentialCSharp.ListingManager;

public class Program
{
    private const string IntelliTect =
@" _____       _       _ _    _______        _   
  |_   _|     | |     | | ( )|__   __|      | |  
    | |  _ __ | |_ ___| | |_    | | ___  ___| |_ 
    | | | '_ \| __/ _ \ | | |   | |/ _ \/ __| __|
   _| |_| | | | ||  __/ | | |   | |  __/ (__| |_ 
  |_____|_| |_|\__\___|_|_|_|   |_|\___|\___|\__|";
    private static int Main(string[] args)
    {
        var directoryIn = new Option<DirectoryInfo>(
            name: "--path",
            description: "The directory of the chapter to update listings on.")
        { IsRequired = true };

        // With proper logging implemented, this option will hopefully be removed
        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Displays more detailed messages in the log");

        var previewOption = new Option<bool>(
            name: "--preview",
            description: "Writes all logs to console as if changes will be made without actually making changes.");

        var allChaptersOption = new Option<bool>(
           name: "--allChapters",
           description: "The passed in path is the parent directory to many chapter directories rather than a single chapter directory.");

        // TODO: Add better descriptions when their functionality becomes clearer
        var byFolderOption = new Option<bool>(
            name: "--byfolder",
            description: "");

        var singleDirOption = new Option<bool>(
            name: "--singleDir",
            description: "All listings are in a single directory and not separated into chapter and chapter test directories");

        var listingUpdating = new Command("update", "Updates namespaces and filenames for all listings and accompanying tests within a chapter")
        {
            directoryIn,
            verboseOption,
            previewOption,
            byFolderOption,
            singleDirOption,
            allChaptersOption
        };

        // Give better description when intent and functionality becomes more flushed out
        var scanForMismatchedListings = new Command("scan", "Scans for mismatched listings")
        {
            directoryIn
        };

        var rootCommand = new RootCommand()
        {
            listingUpdating,
            scanForMismatchedListings
        };

        listingUpdating.SetHandler((directoryIn, verbose, preview, byFolder, singleDir, allChapters) =>
        {
            Console.WriteLine(IntelliTect);
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
        }, directoryIn, verboseOption, previewOption, byFolderOption, singleDirOption, allChaptersOption);

        scanForMismatchedListings.SetHandler((directoryIn) =>
        {
            Console.WriteLine(IntelliTect);
            var extraListings = ListingManager.GetAllExtraListings(directoryIn!.FullName).OrderBy(x => x);

            Console.WriteLine("---Extra Listings---");
            foreach (string extraListing in extraListings)
            {
                Console.WriteLine(extraListing);
            }
        }, directoryIn);

        return rootCommand.Invoke(args);
    }
}
