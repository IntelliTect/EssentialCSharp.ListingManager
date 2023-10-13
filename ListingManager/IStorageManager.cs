using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EssentialCSharp.ListingManager
{
    public interface IStorageManager
    {
        public void Move(string oldPath, string newPath);
    }
}
