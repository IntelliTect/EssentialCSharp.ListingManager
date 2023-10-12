using System;
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

        public static void Main(string path = "", ListingModes mode = ListingModes.ListingUpdating,
            bool verbose = false,
            bool preview = false,
            bool byFolder = false,
            bool chapterOnly = false)
        {
            while (path.EndsWith("\\"))
            {
                path = path.Remove(path.Length - 1, 1);
            }
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
                    ListingManager.UpdateChapterListingNumbers(path, verbose, preview, byFolder, chapterOnly, false);
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
    }
}
