using System.Text.RegularExpressions;

namespace EssentialCSharp.ListingManager;

public partial class ListingInformation
{
    public static IReadOnlyList<string> ApprovedFileTypes { get; } = new[] { ".cs", ".xml" };
    public const string TemporaryExtension = ".tmp";

    public bool Changed { get; private set; }
    public bool FileContentsChanged { get; private set; }

    public int OriginalChapterNumber { get; }
    private int _NewChapterNumber;
    public int NewChapterNumber
    {
        get => _NewChapterNumber;
        set
        {
            if (value != OriginalChapterNumber)
            {
                Changed = true;
            }
            _NewChapterNumber = value;
            if (AssociatedTest is not null)
            {
                AssociatedTest.NewChapterNumber = value;
            }
        }
    }

    public int OriginalListingNumber { get; }
    private int _NewListingNumber;
    public int NewListingNumber
    {
        get => _NewListingNumber;
        set
        {
            if (value != OriginalListingNumber)
            {
                Changed = true;
            }
            _NewListingNumber = value;
            if (AssociatedTest is not null)
            {
                AssociatedTest.NewListingNumber = value;
            }
        }
    }

    public string OriginalListingNumberSuffix { get; }
    private string _NewListingNumberSuffix;
    public string NewListingNumberSuffix
    {
        get => _NewListingNumberSuffix;
        set
        {
            if (value != OriginalListingNumberSuffix)
            {
                Changed = true;
            }
            _NewListingNumberSuffix = value;
            if (AssociatedTest is not null)
            {
                AssociatedTest.NewListingNumberSuffix = value;
            }
        }
    }
    public string Caption { get; set; }
    public string TemporaryPath => Path + TemporaryExtension;
    public string Path { get; set; }
    public string ParentDir { get; }
    public string NamespacePrefix => "AddisonWesley.Michaelis.EssentialCSharp";
    public string ListingExtension { get; }
    public List<string> FileContents { get; set; }
    public ListingInformation? AssociatedTest { get; set; }
    public bool IsTest { get; }
    private string FullCaption { get; }

    public ListingInformation(string listingPath, bool isTest = false)
    {
        Match matches = ExtractListingNameFromAnyApprovedFileTypes().Match(listingPath);

        if (ApprovedFileTypes.Contains(matches.Groups[6].Value.ToLower()) is false) throw new ArgumentException("Listing path is not of an approved file type.", nameof(listingPath));

        if (int.TryParse(matches.Groups[1].Value, out int chapterNumber)
            && int.TryParse(matches.Groups[2].Value, out int listingNumber)
            && matches.Success)
        {
            OriginalChapterNumber = _NewChapterNumber = chapterNumber;
            OriginalListingNumber = _NewListingNumber = listingNumber;
            OriginalListingNumberSuffix = _NewListingNumberSuffix = !string.IsNullOrWhiteSpace(matches.Groups[3].Value) ? matches.Groups[3].Value : string.Empty;
            Caption = !string.IsNullOrWhiteSpace(matches.Groups[5].Value) ? matches.Groups[5].Value : string.Empty;
            FullCaption = matches.Groups[4].Value;
            IsTest = isTest || (!string.IsNullOrWhiteSpace(FullCaption) ? FullCaption : string.Empty).EndsWith(".Tests");
            Path = listingPath;
            ListingExtension = matches.Groups[6].Value;
            FileContents = File.ReadAllLines(listingPath).ToList();
            ParentDir = new FileInfo(listingPath).Directory?.FullName ?? throw new InvalidOperationException("Path is unexpectedly null");
        }
        else
        {
            throw new ArgumentException("Listing information not successfully able to be parsed from listing path.", nameof(listingPath));
        }
    }

    public string GetPaddedListingNumberWithSuffix(bool originalListingNumber = false)
    {
        if (!originalListingNumber) return NewListingNumber.ToString("D2") + NewListingNumberSuffix;
        else return (OriginalListingNumber.ToString("D2") + NewListingNumberSuffix);
    }

    public string GetNewNamespace()
    {
        string paddedChapterNumber = NewChapterNumber.ToString("D2");
        string paddedListingNumber = GetPaddedListingNumberWithSuffix();

        return NamespacePrefix
               + $".Chapter{paddedChapterNumber}"
               + $".Listing{paddedChapterNumber}_"
               + paddedListingNumber + (IsTest ? ".Tests" : string.Empty);
    }

    public bool UpdateNamespaceInFileContents()
    {
        return UpdateNamespaceInFileContents(GetNewNamespace());
    }

    public bool UpdateNamespaceInFileContents(string newNamespace)
    {
        bool updated = false;
        for (int i = 0; i < FileContents.Count; i++)
        {
            if (FileContents[i].TrimStart().StartsWith("namespace"))
            {
                FileContents[i] = $"namespace {newNamespace}";
                updated = true;
                FileContentsChanged = true;
            }
        }
        return updated;
    }

    public void UpdateReferencesInFile(List<ListingInformation> listingData)
    {
        for (int i = 0; i < FileContents.Count; i++)
        {
            if (ListingReference().IsMatch(FileContents[i]))
            {
                MatchCollection matches = ListingReference().Matches(FileContents[i]);
                for (int j = 0; j < matches.Count; j++)
                {
                    int chapterNumber = int.Parse(matches[j].Groups[1].Value);
                    string chapterListingDeliminator = matches[j].Groups[2].Value;
                    int listingNumber = int.Parse(matches[j].Groups[3].Value);
                    ListingInformation? referencedListingInformation = listingData.FirstOrDefault(item => item.OriginalChapterNumber == chapterNumber && item.OriginalListingNumber == listingNumber);
                    if (referencedListingInformation is not null)
                    {
                        string replacementListingReference = matches[j].Groups[0].Value.Replace($"{chapterNumber:D2}{matches[j].Groups[2].Value}{listingNumber:D2}", $"{referencedListingInformation.NewChapterNumber:D2}{matches[j].Groups[2].Value}{referencedListingInformation.NewListingNumber:D2}");
                        FileContents[i] = FileContents[i].Replace(matches[j].Groups[0].Value, replacementListingReference);
                    }
                }
                FileContentsChanged = true;
            }
        }
    }

    public string GetNewFileName()
    {
        string newFileNameTemplate = "Listing{0}.{1}{2}" + (IsTest && !FullCaption.EndsWith(".Tests") ? ".Tests" : string.Empty) + ListingExtension;
        string paddedChapterNumber = NewChapterNumber.ToString("00");
        string paddedListingNumber = GetPaddedListingNumberWithSuffix();

        return string.Format(newFileNameTemplate,
            paddedChapterNumber,
            paddedListingNumber,
            string.IsNullOrWhiteSpace(Caption) ? "" : $".{Caption}");
    }

    // Match any approved files regex: regexr.com/7lfi2
    [GeneratedRegex("Listing(\\d{2}).(\\d{2})([A-Za-z]*)(\\.{1}(.*))*(\\.(\\w+))$")]
    private static partial Regex ExtractListingNameFromAnyApprovedFileTypes();
    [GeneratedRegex(@"\d{1}[A-Za-z]")]
    private static partial Regex SingleDigitListingWithSuffix();
    [GeneratedRegex("Listing(\\d{2})([_.])(\\d{2})")]
    private static partial Regex ListingReference();
}