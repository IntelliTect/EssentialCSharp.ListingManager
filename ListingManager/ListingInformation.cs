using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ListingManager
{
    public partial class ListingInformation
    {
        public static List<string> approvedFileTypes = new() { ".cs", ".xml" };
        public const string TemporaryExtension = ".tmp";
        public int ChapterNumber { get; }
        public int ListingNumber { get; }
        public string ListingSuffix { get; }
        public string ListingDescription { get; }
        public string TemporaryPath { get; }
        public string Path => TemporaryPath.Remove(TemporaryPath.Length - TemporaryExtension.Length, TemporaryExtension.Length);
        public string ListingExtension { get; }

        public ListingInformation(string listingPath, bool onlyCSFiles = false)
        {
            // Only match .cs files regex: regexr.com/7lfhv
            // Match any approved files regex: regexr.com/7lfi2
            Regex regex = onlyCSFiles ? ExtractListingNameFromCSFile() : ExtractListingNameFromAnyApprovedFileTypes();

            var matches = regex.Match(listingPath);

            if (int.TryParse(matches.Groups[1].Value, out int chapterNumber)
                && int.TryParse(matches.Groups[2].Value, out int listingNumber)
                && matches.Success)
            {
                ChapterNumber = chapterNumber;
                ListingNumber = listingNumber;
                ListingSuffix = !string.IsNullOrWhiteSpace(matches.Groups[3].Value) ? matches.Groups[3].Value : "";
                ListingDescription = !string.IsNullOrWhiteSpace(matches.Groups[5].Value) ? matches.Groups[5].Value : "";
                TemporaryPath = listingPath + TemporaryExtension;
                ListingExtension = System.IO.Path.GetExtension(listingPath);
            }
            else
            {
                throw new ArgumentException("Listing information not successfully able to be parsed from listing path.", nameof(listingPath));
            }
        }

        [GeneratedRegex("Listing(\\d{2}).(\\d{2})([A-Za-z]*)(\\.{1}(.*))?[.cs,.xml]$")]
        private static partial Regex ExtractListingNameFromAnyApprovedFileTypes();
        [GeneratedRegex("Listing(\\d{2}).(\\d{2})([A-Za-z]*)(\\.{1}(.*))?.cs$")]
        private static partial Regex ExtractListingNameFromCSFile();
    }
}