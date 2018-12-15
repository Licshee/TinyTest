using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class TinyTest
{
    const int SANE_BUFFER_SIZE = 0x1000;

    public static void Test(string root, ushort size = SANE_BUFFER_SIZE)
    {
        new TinyTest(root, size).Test();
    }

    ushort size;
    byte[] data;
    byte[] read;
    string root;
    Queue<FileInfo> list;

    public TinyTest(string root, ushort size = SANE_BUFFER_SIZE)
    {
        Init(root, size);
    }

    public void Init(string root, ushort size = SANE_BUFFER_SIZE)
    {
        this.root = root;
        list = new Queue<FileInfo>();
        data = new byte[size];
        read = new byte[size];
        this.size = size;
    }

    Random rand = new Random();
    int seed;

    public void Test()
    {

        do
        {
            var fi = new FileInfo(Path.Combine(root, "!~" + Guid.NewGuid().ToString()));

            seed = Environment.TickCount;

            var left = CreateFile(fi);

            if (left > 0)
                list.Enqueue(fi);
            else
            {
                if (left == 0) list.Enqueue(fi);
                break;
            }

            VerifyFile(fi, left);
        } while (true);

        while (list.Count > 0)
            list.Dequeue().Delete();
    }

    long CreateFile(FileInfo info)
    {
        rand = new Random(seed);
        using (var fs = new FileStream(info.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None, size))
        {
            try
            {
                while (true)
                {
                    rand.NextBytes(data);
                    fs.Write(data, 0, size);
                }
            }
            catch (IOException ex) when (ex.HResult == -2147024784)
            { }

            fs.Flush();
            return fs.Length;
        }
    }

    void VerifyFile(FileInfo info, long left)
    {
        int sum = 0;
        rand = new Random(seed);

        using (var fs = new FileStream(info.FullName, FileMode.Open, FileAccess.Read, FileShare.None, size))
        {
            if (fs.Length == left)
            {
                while ((left -= fs.Read(read, 0, size)) > size)
                {
                    rand.NextBytes(data);

                    for (var i = 0; i < size; i++)
                        sum |= data[i] - read[i];

                    if (sum != 0) throw new InvalidDataException();
                }
            }
        }
    }
}
