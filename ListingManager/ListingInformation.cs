using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EssentialCSharp.ListingManager;

public partial class ListingInformation
{
    public static IReadOnlyList<string> ApprovedFileTypes { get; } = new[] { ".cs", ".xml" };
    public const string TemporaryExtension = ".tmp";

    public bool Changed { get; private set; }

    public int OriginalChapterNumber { get; }
    private int newChapterNumber;
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
            if (AssociatedTest is not null)
            {
                AssociatedTest.NewChapterNumber = value;
            }
        }
    }

    public int OriginalListingNumber { get; }
    private int newListingNumber;
    public int NewListingNumber
    {
        get => newListingNumber;
        set
        {
            if (value != OriginalListingNumber)
            {
                Changed = true;
            }
            newListingNumber = value;
            if (AssociatedTest is not null)
            {
                AssociatedTest.NewListingNumber = value;
            }
        }
    }

    public string OriginalListingNumberSuffix { get; }
    private string newListingNumberSuffix;
    public string NewListingNumberSuffix
    {
        get => newListingNumberSuffix;
        set
        {
            if (value != OriginalListingNumberSuffix)
            {
                Changed = true;
            }
            newListingNumberSuffix = value;
            if (AssociatedTest is not null)
            {
                AssociatedTest.NewListingNumberSuffix = value;
            }
        }
    }
    public string Caption { get; }
    public string TemporaryPath => Path + TemporaryExtension;
    public string Path { get; }
    public string ParentDir { get; }
    public string NamespacePrefix => "AddisonWesley.Michaelis.EssentialCSharp";
    public string ListingExtension { get; }
    public string FileContents { get; set; }
    public ListingInformation? AssociatedTest { get; set; }
    public bool IsTest { get; }
    private string FullCaption { get; }

    public ListingInformation(string listingPath, bool isTest = false)
    {
        Regex regex = ExtractListingNameFromAnyApprovedFileTypes();

        var matches = regex.Match(listingPath);

        if (ApprovedFileTypes.Contains(matches.Groups[6].Value.ToLower()) is false) throw new ArgumentException("Listing path is not of an approved file type.", nameof(listingPath));

        if (int.TryParse(matches.Groups[1].Value, out int chapterNumber)
            && int.TryParse(matches.Groups[2].Value, out int listingNumber)
            && matches.Success)
        {
            OriginalChapterNumber = newChapterNumber = chapterNumber;
            OriginalListingNumber = newListingNumber = listingNumber;
            OriginalListingNumberSuffix = newListingNumberSuffix = !string.IsNullOrWhiteSpace(matches.Groups[3].Value) ? matches.Groups[3].Value : string.Empty;
            Caption = !string.IsNullOrWhiteSpace(matches.Groups[5].Value) ? matches.Groups[5].Value : string.Empty;
            FullCaption = matches.Groups[4].Value;
            IsTest = isTest || (!string.IsNullOrWhiteSpace(FullCaption) ? FullCaption : string.Empty).EndsWith(".Tests");
            Path = listingPath;
            ListingExtension = matches.Groups[6].Value;
            FileContents = System.IO.File.ReadAllText(listingPath);
            ParentDir = new FileInfo(listingPath).Directory?.FullName ?? throw new InvalidOperationException("Path is unexpectedly null");
        }
        else
        {
            throw new ArgumentException("Listing information not successfully able to be parsed from listing path.", nameof(listingPath));
        }
    }
    // Match any approved files regex: regexr.com/7lfi2
    [GeneratedRegex("Listing(\\d{2}).(\\d{2})([A-Za-z]*)(\\.{1}(.*))*(\\.(\\w+))$")]
    private static partial Regex ExtractListingNameFromAnyApprovedFileTypes();

    public string GetPaddedListingNumberWithSuffix(bool originalListingNumber = false)
    {
        if (!originalListingNumber) return NewListingNumber.ToString("D2") + NewListingNumberSuffix;
        else return (OriginalListingNumber.ToString("D2") + NewListingNumberSuffix);

    }
    public string GetNewNamespace(bool chapterOnly)
    {
        string paddedChapterNumber = NewChapterNumber.ToString("D2");
        string paddedListingNumber = GetPaddedListingNumberWithSuffix(chapterOnly);

        return NamespacePrefix
               + $".Chapter{paddedChapterNumber}"
               + $".Listing{paddedChapterNumber}_"
               + paddedListingNumber + (IsTest ? ".Tests" : string.Empty);
    }

    public string GetNewFileName(bool chapterOnly)
    {
        string newFileNameTemplate = "Listing{0}.{1}{2}" + (IsTest && !FullCaption.EndsWith(".Tests") ? ".Tests" : string.Empty) + ListingExtension;
        string paddedChapterNumber = NewChapterNumber.ToString("00");
        string paddedListingNumber = GetPaddedListingNumberWithSuffix();

        return string.Format(newFileNameTemplate,
            paddedChapterNumber,
            paddedListingNumber,
            string.IsNullOrWhiteSpace(Caption) ? "" : $".{Caption}");
    }

    [GeneratedRegex(@"\d{1}[A-Za-z]")]
    private static partial Regex SingleDigitListingWithSuffix();
}

