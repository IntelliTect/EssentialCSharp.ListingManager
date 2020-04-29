using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ListingManager.Tests
{
    [TestClass]
    public class FileManagerTests
    {

        [TestMethod]
        [DataRow("Chapter01", 1)]
        [DataRow("asdasdsad/asd/asd/asd/Chapter42", 42)]
        public void GetChapterNumber(string chapterFilePath, int expectedChapterNum)
        {
            Assert.AreEqual(expectedChapterNum, FileManager.GetFolderChapterNumber(chapterFilePath));
        }
    }
}