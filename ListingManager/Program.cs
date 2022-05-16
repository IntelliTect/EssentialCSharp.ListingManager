using System;
using System.IO;
using System.Linq;

namespace ListingManager
{
    public class Program
    {
        private const string IntelliTect =
@" _____       _       _ _ _ _______        _   
|_   _|     | |     | | (_)__   __|      | |  
  | |  _ __ | |_ ___| | |_   | | ___  ___| |_ 
  | | | '_ \| __/ _ \ | | |  | |/ _ \/ __| __|
 _| |_| | | | ||  __/ | | |  | |  __/ (__| |_ 
|_____|_| |_|\__\___|_|_|_|  |_|\___|\___|\__|";

        private const string InteractivePromptPrefix = "INTL {0} ({1})>";

        public static void Main(string path = "", ListingModes mode = ListingModes.ListingUpdating,
            bool verbose = false,
            bool preview = false,
            bool byFolder = false,
            bool chapterOnly = false)
        {
            Console.WriteLine(IntelliTect);

            if (preview)
            {
                Console.WriteLine("Preview mode. Actions will not be taken");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                ConsoleColor foregroundColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                path = Environment.CurrentDirectory;
                Console.WriteLine($"Path not specified. Using working directory: {path}");
                Console.ForegroundColor = foregroundColor;
            }

            switch (mode)
            {
                case ListingModes.ListingUpdating:
                    Console.WriteLine($"Updating listing namespaces of: {path}");
                    ListingManager.UpdateChapterListingNumbers(path, verbose, preview, byFolder, chapterOnly);
                    break;
                case ListingModes.TestGeneration:
                    var generatedTests
                        = ListingManager.GenerateUnitTests(path, TestGeneration_Interactive, true);
                    if (verbose)
                    {
                        Console.WriteLine($"{generatedTests.Count} tests generated");
                    }
                    break;
                case ListingModes.ScanForMismatchedListings:
                    var extraListings = ListingManager.GetAllExtraListings(path).OrderBy(x => x);

                    Console.WriteLine("---Extra Listings---");
                    foreach (string extraListing in extraListings)
                    {
                        Console.WriteLine(extraListing);
                    }
                    break;
                default:
                    Console.WriteLine($"Mode ({mode}) does not exist. Exiting");
                    break;
            }
        }

        private static bool TestGeneration_Interactive(string missingTest)
        {
            InteractiveConsoleWrite("Choose an option", "d - delete, q - quit, enter - continue");
            string input = Console.ReadLine();

            switch (input)
            {
                case "d":
                    Console.WriteLine("Deleting test");
                    File.Delete(missingTest);
                    break;
                case "q":
                    Console.WriteLine("Quitting");
                    return false;
            }

            return true;
        }

        private static void InteractiveConsoleWrite(string toWrite, string userOptions)
        {
            Console.Write(InteractivePromptPrefix, toWrite, userOptions);
        }
    }
}
