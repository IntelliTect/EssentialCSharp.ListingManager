using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ListingUpdater
{
    public class Program
    {
        public const string IntelliTect =
@" _____       _       _ _ _ _______        _   
|_   _|     | |     | | (_)__   __|      | |  
  | |  _ __ | |_ ___| | |_   | | ___  ___| |_ 
  | | | '_ \| __/ _ \ | | |  | |/ _ \/ __| __|
 _| |_| | | | ||  __/ | | |  | |  __/ (__| |_ 
|_____|_| |_|\__\___|_|_|_|  |_|\___|\___|\__|";

        public const string InteractivePromptPrefix = "INTL {0} ({1})>";

        public static void Main(string path = "", int mode = 0, bool verbose = false, 
            bool preview = false)
        {
            /*var colorList = new List<ConsoleColor>{ConsoleColor.Blue, ConsoleColor.Green, ConsoleColor.Yellow, 
                ConsoleColor.DarkCyan, ConsoleColor.DarkRed, ConsoleColor.Cyan};

            var intelliTectSplit = IntelliTect.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            for (var index = 0; index < intelliTectSplit.Length; index++)
            {
                string line = intelliTectSplit[index];
                ConsoleColor prevColor = Console.ForegroundColor;
                Console.ForegroundColor = colorList[index];
                Console.WriteLine(line);
                Console.ForegroundColor = prevColor;
            }*/
            
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
                case 0: // listing updating
                    Console.WriteLine($"Updating listing namespaces of: {path}");
                    ListingManager.UpdateChapterListingNumbers(path, verbose, preview);
                    break;
                case 1: // unit test generation
                    var generatedTests 
                        = ListingManager.GenerateUnitTests(path, TestGeneration_Interactive, true);
                    if (verbose)
                    {
                        Console.WriteLine($"{generatedTests.Count} tests generated");
                    }
                    break;
                case 2: // scan everywhere for mismatched listings
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

        private static void TestGeneration_Interactive(string missingTest)
        {
            InteractiveConsoleWrite("Choose an option", "d - delete, enter - continue");
            string input = Console.ReadLine();

            switch (input)
            {
                case "d":
                    Console.WriteLine("Deleting test");
                    File.Delete(missingTest);
                    break;
            }
        }

        private static void InteractiveConsoleWrite(string toWrite, string userOptions)
        {
            Console.Write(InteractivePromptPrefix, toWrite, userOptions);
        }
    }
}
