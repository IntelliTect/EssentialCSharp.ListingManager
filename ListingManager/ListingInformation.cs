using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ListingManager
{
    public partial class ListingInformation
    {
        public static IReadOnlyList<string> ApprovedFileTypes { get; } = new[] { ".cs", ".xml" };
        public const string TemporaryExtension = ".tmp";
        public int ChapterNumber { get; }
        public int ListingNumber { get; }
        public string ListingSuffix { get; }
        public string ListingDescription { get; }
        public string TemporaryPath { get; }
        public string Path => TemporaryPath.Remove(TemporaryPath.Length - TemporaryExtension.Length, TemporaryExtension.Length);
        public string ListingExtension { get; }

        public ListingInformation(string listingPath)
        {
            Regex regex = ExtractListingNameFromAnyApprovedFileTypes();

            var matches = regex.Match(listingPath);

            if (ApprovedFileTypes.Contains(matches.Groups[6].Value.ToLower()) is false) throw new ArgumentException("Listing path is not of an approved file type.", nameof(listingPath));

            if (int.TryParse(matches.Groups[1].Value, out int chapterNumber)
                && int.TryParse(matches.Groups[2].Value, out int listingNumber)
                && matches.Success)
            {
                ChapterNumber = chapterNumber;
                ListingNumber = listingNumber;
                ListingSuffix = !string.IsNullOrWhiteSpace(matches.Groups[3].Value) ? matches.Groups[3].Value : "";
                ListingDescription = !string.IsNullOrWhiteSpace(matches.Groups[5].Value) ? matches.Groups[5].Value : "";
                TemporaryPath = listingPath + TemporaryExtension;
                ListingExtension = matches.Groups[6].Value;
            }
            else
            {
                throw new ArgumentException("Listing information not successfully able to be parsed from listing path.", nameof(listingPath));
            }
        }

        // Match any approved files regex: regexr.com/7lfi2
        [GeneratedRegex("Listing(\\d{2}).(\\d{2})([A-Za-z]*)(\\.{1}(.*))*(\\.(\\w+))$")]
        private static partial Regex ExtractListingNameFromAnyApprovedFileTypes();
    }
}