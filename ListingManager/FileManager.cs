using System;
using System.Collections.Generic;
using System.IO;

namespace ListingManager
{
    /// <summary>
    /// A path utility class
    /// </summary>
    public static class FileManager
    {
        /// <summary>
        /// Retrieves all files in the target directory <paramref name="pathToSearch"/> that match the specified pattern <paramref name="searchPattern"/>
        /// </summary>
        /// <param name="pathToSearch">The target directory</param>
        /// <param name="recursive">Whether to recursively descend from the target directory</param>
        /// <param name="searchPattern">The search string to match against the names of files in the target path</param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllFilesAtPath(string pathToSearch, bool recursive = false,
            string searchPattern = "*")
        {
            return Directory.EnumerateFiles(pathToSearch,
                searchPattern,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Parses the numerical value of the chapter number from the specified path <paramref name="pathToChapter"/>.
        /// </summary>
        /// <param name="pathToChapter">The target chapter</param>
        /// <returns></returns>
        public static int GetFolderChapterNumber(string pathToChapter)
        {
            string chapterText = "Chapter";
            int startOfChapterNumber =
                pathToChapter.IndexOf(chapterText, StringComparison.Ordinal) + chapterText.Length;

            return int.Parse(pathToChapter.Substring(startOfChapterNumber, 2));
        }
    }
}