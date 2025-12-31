// See https://aka.ms/new-console-template for more information

using System.Text;
using NcmToMp3;
using NcmToMp3.EncryptionModule;
using NcmToMp3.IOModule;

NCMFileReader? nfr = null;

Console.WriteLine("Hi,蔡梓盛");

const string outputPath = @"C:\Users\HHHHCHYK\Downloads\output";
const string endOfFile = ".mp3";
Directory.CreateDirectory(outputPath);
while (true) // 主循环
{
    // 读取用户输入
    Console.WriteLine("输入ncm批处理路径");
    var ip = Console.ReadLine()?.Trim();
    ip = @"C:\Users\HHHHCHYK\Downloads";
    if (string.IsNullOrEmpty(ip)) continue;
    if (ip.ToLower().Equals("exit")) break;

    Console.WriteLine("开始处理……");
    // 创建nfr实例
    try
    {
        nfr = new NCMFileReader(ip);
    }
    catch (DirectoryNotFoundException de)
    {
        NoticePublisher.Publish("目标路径不存在", null, MessageLevel.Warning);
        NoticePublisher.Publish(de.Message, null, MessageLevel.Warning);
        NoticePublisher.Publish(de.StackTrace ?? "NullStackTrace", null, MessageLevel.Warning);
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
        
        // 开始文件解密
        Decryptor.StartDecryption(ncmFileSteam);
    }
    
    
}
