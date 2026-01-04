// See https://aka.ms/new-console-template for more information

using NcmToMp3.EncryptionModule;
using NcmToMp3.IOModule;

Console.WriteLine("Hi,NcmDecryptor!");

Console.WriteLine("请输入输出地址");
string? outputPath = null;
while(outputPath?.Trim() == null)outputPath = Console.ReadLine();   // 非空读取
Directory.CreateDirectory(outputPath);
while (true) // 主循环
{
    // 读取用户输入
    Console.WriteLine("输入ncm批处理路径");
    var ip = Console.ReadLine()?.Trim();
    //ip = @"C:\Users\HHHHCHYK\Downloads";
    if (string.IsNullOrEmpty(ip)) continue;
    if (ip.ToLower().Equals("exit")) break;

    Console.WriteLine("开始处理……");
    // 创建nfr实例
    NCMFileReader? nfr;
    try
    {
        nfr = new NCMFileReader(ip);
    }
    catch (DirectoryNotFoundException de)
    {
        Console.WriteLine(de.Message);
        continue;
    }

    // 启动文件解密操作
    while (nfr.HasNewFile)
    {
        // 获取ncm文件流
        FileStream ncmFileSteam = nfr.GetNewFile();
        String fileName = Path.GetFileName(ncmFileSteam.Name);
        Console.WriteLine("正在解密文件：" + fileName);
        
        // 创建输出文件，并且获取输出文件流                
        //FileStream outputStream = File.Create(Path.Combine(outputPath, fileName+endOfFile));

        List<KeyValuePair<string,string>> wrongFileName = new();
        if (wrongFileName == null) throw new ArgumentNullException(nameof(wrongFileName));

        // 开始文件解密
        try
        {
            Decryptor.StartDecryption(ncmFileSteam);
        }
        catch (Exception e)
        {
            wrongFileName.Add(new  KeyValuePair<string, string>(fileName, e.Message));
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }
    
    
}
