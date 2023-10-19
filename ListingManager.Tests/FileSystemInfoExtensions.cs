using System.IO;

namespace EssentialCSharp.ListingManager.Tests;

internal static class FileSystemInfoExtensions
{
    public static void DeleteReadOnly(this FileSystemInfo fileSystemInfo)
    {
        DirectoryInfo? directoryInfo = fileSystemInfo as DirectoryInfo;
        if (directoryInfo is not null)
        {
            foreach (FileSystemInfo childInfo in directoryInfo.GetFileSystemInfos())
            {
                childInfo.DeleteReadOnly();
            }
        }

        fileSystemInfo.Attributes = FileAttributes.Normal;
        fileSystemInfo.Refresh();
        fileSystemInfo.Delete();
    }
}