using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EssentialCSharp.ListingManager;

public partial class ListingInformation
{
    public static IReadOnlyList<string> ApprovedFileTypes { get; } = new[] { ".cs", ".xml" };
    public const string TemporaryExtension = ".tmp";
    private int newListingNumber;
    private int newChapterNumber;

    public bool Changed { get; private set; }
    public int OriginalChapterNumber { get; }
    public int OriginalListingNumber { get; }
    public int NewChapterNumber
    {
        get => newChapterNumber; 
        set
        {
            if (value != OriginalChapterNumber)
            {
                Changed = true;
            }
            newChapterNumber = value;
        }
    }
    public int NewListingNumber {
        get => newListingNumber; 
        set
        {
            if (value != OriginalListingNumber)
            {
                Changed = true;
            }
            newListingNumber = value;
        }
    }
    public string ListingSuffix { get; }
    public string ListingDescription { get; }
    public string TemporaryPath => Path + TemporaryExtension;
    public string Path { get; }
    public string ListingExtension { get; }
    public string FileContents { get; set; }
    public ListingInformation? AssociatedTest { get; set; }

    public ListingInformation(string listingPath)
    {
        Regex regex = ExtractListingNameFromAnyApprovedFileTypes();

        var matches = regex.Match(listingPath);

        if (ApprovedFileTypes.Contains(matches.Groups[6].Value.ToLower()) is false) throw new ArgumentException("Listing path is not of an approved file type.", nameof(listingPath));

        if (int.TryParse(matches.Groups[1].Value, out int chapterNumber)
            && int.TryParse(matches.Groups[2].Value, out int listingNumber)
            && matches.Success)
        {
            OriginalChapterNumber = NewChapterNumber = chapterNumber;
            OriginalListingNumber = NewListingNumber = listingNumber;
            ListingSuffix = !string.IsNullOrWhiteSpace(matches.Groups[3].Value) ? matches.Groups[3].Value : "";
            ListingDescription = !string.IsNullOrWhiteSpace(matches.Groups[5].Value) ? matches.Groups[5].Value : "";
            Path = listingPath;
            ListingExtension = matches.Groups[6].Value;
            FileContents = System.IO.File.ReadAllText(listingPath);
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

