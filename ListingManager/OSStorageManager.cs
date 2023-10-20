namespace EssentialCSharp.ListingManager;

public class OSStorageManager : IStorageManager
{
    public void Move(string oldPath, string newPath)
    {
        File.Copy(oldPath, newPath, false);
        File.Delete(oldPath);
    }
}
