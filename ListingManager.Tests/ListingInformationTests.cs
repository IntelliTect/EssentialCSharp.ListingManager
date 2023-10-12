using EssentialCSharp.ListingManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EssentialCSharp.ListingManager.Tests;

[TestClass]
public class ListingInformationTests
{
    [TestMethod]
    [DataRow("Listing01.01.cs", 1, 1, null, null)]
    [DataRow("Listing01.02A.cs", 1, 2, "A", null)]
    [DataRow("Listing01.02.something.cs", 1, 2, null, "something")]
    [DataRow("Listing05.04.Something.cs", 5, 4, null, "Something")]
    [DataRow("Listing09.13.Some.Parse.cs", 9, 13, null, "Some.Parse")]
    public void Constructor_GivenValidListings_PropertiesPopulatedSuccessfully(string listing,
        int chapterNumber, int listingNumber, string suffix, string description)
    {
        ListingInformation listingInformation = new(listing);

        Assert.AreEqual(chapterNumber, listingInformation.ChapterNumber);
        Assert.AreEqual(listingNumber, listingInformation.ListingNumber);

        Assert.AreEqual(suffix ?? "", listingInformation.ListingSuffix);

        Assert.AreEqual(description ?? "", listingInformation.ListingDescription);
    }

    [TestMethod]
    [DataRow("Listing01.01.cs")]
    [DataRow("Listing05.04.Something.xml")]
    [DataRow("Listing05.04.Something.XML")]
    public void Constructor_GivenValidListingFileTypes_CreatesNewListingInformation(string listing)
    {
        ListingInformation listingInformation = new(listing);
        Assert.IsNotNull(listingInformation);
        Assert.AreEqual(System.IO.Path.GetExtension(listing), listingInformation.ListingExtension);
    }

    [TestMethod]
    [DataRow("Listing01.02.something.txt")]
    [DataRow("Listing01.02A.csproj")]
    public void Constructor_GivenInvalidListingFileTypes_ThrowsArgumentException(string listing)
    {
        Assert.ThrowsException<System.ArgumentException>(() => new ListingInformation(listing));
    }
}