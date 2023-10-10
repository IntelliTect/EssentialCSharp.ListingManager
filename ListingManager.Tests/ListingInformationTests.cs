using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ListingManager.Tests
{
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
            ListingInformation listingInformation = new(listing, onlyCSFiles: true);

            Assert.AreEqual(chapterNumber, listingInformation.ChapterNumber);
            Assert.AreEqual(listingNumber, listingInformation.ListingNumber);

            Assert.AreEqual(suffix ?? "", listingInformation.ListingSuffix);

            Assert.AreEqual(description ?? "", listingInformation.ListingDescription);
        }
    }
}