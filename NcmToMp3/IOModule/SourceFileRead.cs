namespace NcmToMp3.IOModule;

public class SourceFileRead
{
    public static List<FileStream>? getNCMFileStreams(String filePath)
    {
        if (Directory.Exists(filePath) == false) return null;
        var files = Directory.GetFiles(filePath,"*.ncm");
        var fileStreams = new List<FileStream>();
        foreach (var file in files)
        {
            try
            {
                fileStreams.Add(File.Create(file));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        return fileStreams;
    } 
}