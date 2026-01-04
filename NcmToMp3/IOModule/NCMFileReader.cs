namespace NcmToMp3.IOModule;

public class NCMFileReader
{
    private readonly Queue<string> files = new();

    public NCMFileReader(string rootPath)
    {
        if (!Directory.Exists(rootPath)) throw new DirectoryNotFoundException();
        var fileList = Directory.GetFiles(rootPath, "*.ncm", SearchOption.AllDirectories);
        if (fileList.Length == 0x00)
        {
            Console.WriteLine("路径下没有NCM文件");
            return;
        }

        foreach (var file in fileList)
            files.Enqueue(file);
    }

    public FileStream GetNewFile()
    {
        if (files.Count <= 0) throw new InvalidOperationException();
        var file = files.Dequeue();
        return new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public bool HasNewFile => files.Count > 0;
}