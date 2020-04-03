using System.Collections.Generic;
using System.IO;

namespace ListingManager
{
    public static class FileManager
    {
        public static IEnumerable<string> GetAllFilesAtPath(string pathToSearch, bool recursive = false, string searchPattern = "*")
        {
            return Directory.EnumerateFiles(pathToSearch,
                searchPattern,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public static int GetFolderChapterNumber(string pathToChapter)
        {
            string chapterText = "Chapter";
            int startOfChapterNumber = pathToChapter.IndexOf(chapterText) + chapterText.Length;

            return int.Parse(pathToChapter.Substring(startOfChapterNumber, 2));
        }
    }
}