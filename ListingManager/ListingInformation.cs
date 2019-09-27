using System;
using System.Text.RegularExpressions;

namespace ListingUpdater
{
    public class ListingInformation
    {
        public int ChapterNumber { get; set; }
        public int ListingNumber { get; set; }
        public string ListingSuffix { get; set; }
        public string ListingDescription { get; set; }

        public ListingInformation(string listingPath)
        {
            Regex regex = new Regex(@"Listing(\d{2}).(\d{2})([A-Za-z]*)(\.{1}(\w*))?.cs$");

            var matches = regex.Match(listingPath);

            if (int.TryParse(matches.Groups[1].Value, out int chapterNumber)
                && int.TryParse(matches.Groups[2].Value, out int listingNumber)
                && matches.Success)
            {
                ChapterNumber = chapterNumber;
                ListingNumber = listingNumber;
                ListingSuffix = !string.IsNullOrWhiteSpace(matches.Groups[3].Value) ? matches.Groups[3].Value : "";
                ListingDescription = !string.IsNullOrWhiteSpace(matches.Groups[5].Value) ? matches.Groups[5].Value : "";
            }
            else
            {
                throw new ArgumentException(nameof(listingPath));
            }
        }
    }
}