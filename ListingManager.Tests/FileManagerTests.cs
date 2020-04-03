using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ListingManager.Tests
{
    [TestClass]
    public class FileManagerTests
    {

        [TestMethod]
        public void GetChapterNumber()
        {
            string chapter1 = "Chapter01";
            string chapter42 = "asdasdsad/asd/asd/asd/Chapter42";

            Assert.IsTrue(FileManager.GetFolderChapterNumber(chapter1) == 1);
            Assert.IsTrue(FileManager.GetFolderChapterNumber(chapter42) == 42);

        }
    }
}