using System;
using System.CommandLine;
using System.IO;
using System.Linq;

namespace EssentialCSharp.ListingManager;

public class Program
{
    private const string IntelliTect =
@" _____       _       _ _ _ _______        _   
|_   _|     | |     | | (_)__   __|      | |  
  | |  _ __ | |_ ___| | |_   | | ___  ___| |_ 
  | | | '_ \| __/ _ \ | | |  | |/ _ \/ __| __|
 _| |_| | | | ||  __/ | | |  | |  __/ (__| |_ 
|_____|_| |_|\__\___|_|_|_|  |_|\___|\___|\__|";
    private static int Main(string[] args)
    {
        var directoryIn = new Option<DirectoryInfo?>(
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

        // TODO: Add better descriptions when their functionality becomes clearer
        var byFolderOption = new Option<bool>(
            name: "--byfolder",
            description: "");

        // TODO: Add better descriptions when their functionality becomes clearer
        var chapterOnlyOption = new Option<bool>(
            name: "--chapteronly",
            description: "");

        var listingUpdating = new Command("update", "Updates namespaces and filenames for all lisitngs and accompanying tests within a chapter")
        {
            directoryIn,
            verboseOption,
            previewOption,
            byFolderOption,
            chapterOnlyOption
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

        listingUpdating.SetHandler((directoryIn, verbose, preview, byFolder, chapterOnly) =>
        {
            Console.WriteLine(IntelliTect);
            Console.WriteLine($"Updating listing namespaces of: {directoryIn}");
            // TODO: Add option for last parameter
            // TODO: Change parameter to take a DirectoryInfo instead of a string: https://github.com/IntelliTect/ListingManager/issues/26
            ListingManager listingManager = new(directoryIn!.FullName);
            listingManager.UpdateChapterListingNumbers(directoryIn!.FullName, verbose, preview, byFolder, chapterOnly, false);
        }, directoryIn, verboseOption, previewOption, byFolderOption, chapterOnlyOption);

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
