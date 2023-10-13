using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EssentialCSharp.ListingManager
{
    public class OSStorageManager : IStorageManager
    {
        public void Move(string oldPath, string newPath)
        {
            File.Copy(oldPath, newPath, false);
            File.Delete(oldPath);
        }
    }
}
