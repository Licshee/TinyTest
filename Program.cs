
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        TinyTest.Test(DriveInfo.GetDrives().Last().RootDirectory.FullName);
    }
}
