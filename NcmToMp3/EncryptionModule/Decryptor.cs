using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.VisualBasic;
using NcmToMp3.utils;

namespace NcmToMp3.EncryptionModule;

public static class Decryptor
{
    internal static byte[] CoreKey = "hzHRAmso5kInbaxW"u8.ToArray();

    // ReSharper disable once UseUtf8StringLiteral
    internal static byte[] MetaKey =
    [
        0x23, 0x31, 0x34, 0x6C, 0x6A, 0x6B, 0x5F, 0x21,
        0x5C, 0x5D, 0x26, 0x30, 0x55, 0x3C, 0x27, 0x28
    ];

    public static readonly byte[] MagicNumber = "CTENFDAM"u8.ToArray();

    /// <summary>
    /// 解密启动方法
    /// </summary>
    /// <param name="ncmFileSteam">ncm文件流</param>
    /// <param name="outputStream"></param>
    /// <exception cref="ArgumentException">当传入的文件流不可读时抛出</exception>
    public static void StartDecryption(FileStream ncmFileSteam)
    {
        if (!ncmFileSteam.CanRead) throw new ArgumentException("Ncm File Cannot Read");
        //if (!outputStream.CanWrite) throw new ArgumentException("Output Stream Cannot Write");
        ncmFileSteam.Position = 0;
        DecryptFile(ncmFileSteam);
    }


    private static void DecryptFile(FileStream ncmFileSteam)
    {
        // 文件头校验
        byte[] fileHead = new Byte[8];
        var readNumber = ncmFileSteam.Read(fileHead, 0, 8);
        //Console.WriteLine(Encoding.UTF8.GetString(fileHead));
        if (readNumber != 8)
        {
            Console.WriteLine("读取的文件格式错误,解析已终止:" + Encoding.UTF8.GetString(fileHead));
            return;
        }

        // ReSharper disable once StringLiteralTypo
        if (!fileHead.SequenceEqual(MagicNumber))
        {
            Console.WriteLine("读取的文件格式错误,解析已终止");
            return;
        }

        // 偏移无用的两个字节
        ncmFileSteam.Seek(2, SeekOrigin.Current);

        // 读取并解密KeyData
        DecryptKeyData(ncmFileSteam, out var keyData);


        // 读取并解密MetaData
        DecryptMetaData(ncmFileSteam, out var metaData);

        // 读取CRC与封面
        GetCRC(ncmFileSteam, out var crc);
        GetImage(ncmFileSteam, out var image);
        // 处理CRC和封面

        // 创建输出文件流
        FileStream outputStream = File.Create(".\\output\\"
                                              + (metaData.musicName ?? "NoneName")
                                              + '.' + (metaData.format ?? "mp3"));

        // 解密音频数据
        SpawnKeyBoxByKey(keyData, out var keyBox);
        DecryptCoreDataAsync(ncmFileSteam, outputStream, keyBox, () => Console.WriteLine("核心音频数据解密已完成"));

        // 释放句柄
        ncmFileSteam.Dispose();
        outputStream.Dispose();
    }


    private static void DecryptCoreDataAsync(
        FileStream inputFileStream,
        FileStream outputStream,
        byte[] keyBox,
        Action? callback = null)
    {
        var buffer = new byte[0x8000];
        //var decryptedBuffer = new byte[0x8000];
        //long byteIndex = 1;
        Console.WriteLine(keyBox.Length);
        // 块解密
        while (true)
        {
            // 读取并且判断是否到达文件末尾
            int readCount = inputFileStream.Read(buffer, 0, buffer.Length);
            if (readCount == 0) break;

            for (int i = 1; i <= readCount; i++)
            {
                byte indexKey = (byte)((i) & 0xFF);
                buffer[i - 1] ^= keyBox[(keyBox[indexKey] + keyBox[(keyBox[indexKey] + indexKey) & 0xFF]) & 0xFF];
            }

            // 写入outputStream
            outputStream.Write(buffer, 0, readCount);
        }

        // 触发回调
        callback?.Invoke();
    }

    private static void SpawnKeyBoxByKey(byte[] key, out byte[] keyBox)
    {
        // 初始化KeyBox
        keyBox = new byte[256];
        for (int i = 0; i < 256; i++) keyBox[i] = (byte)(i & 0xFF);

        // 打乱KeyBox
        int c;
        int lastByte = 0;
        int keyOffset = 0; // 初始化循环变量


        for (int i = 0; i < 256; i++)
        {
            byte value = keyBox[i];
            c = (value + lastByte + key[keyOffset]) & 0xFF; //计算出交换目标索引

            keyOffset++;
            if (keyOffset >= key.Length) keyOffset = 0;

            keyBox[i] = keyBox[c];
            keyBox[c] = value;
            lastByte = c;
        }

        //ConsoleExtensions.WriteByteArray(keyBox);
    }

    /// <summary>
    /// 解密KeyData
    /// </summary>
    /// <param name="fileStream">Ncm文件流</param>
    /// <param name="decryptedKeyData">解密后的KeyData</param>
    /// <exception cref="InvalidOperationException">读取文件失败时抛出</exception>
    private static void DecryptKeyData(FileStream fileStream, out byte[] decryptedKeyData)
    {
        Console.WriteLine("正在解析KeyData 密钥数据……");

        // 读取密钥长度
        byte[] keyLenght = new Byte[4];
        int readCount = fileStream.Read(keyLenght, 0, 4);
        if (readCount != 4) throw new InvalidOperationException("读取密钥长度失败：文件流读取不完全");

        // 解析密钥长度
        uint dataKeyLength = BitConverter.ToUInt32(keyLenght, 0);
        //Console.WriteLine("KeyData 长度为" + dataKeyLength);
        //Console.WriteLine($"KeyData 长度原始数据为:" );
        //ConsoleExtensions.WriteByteArray(keyLenght);

        // 读取密钥
        byte[] dataKey = new byte[dataKeyLength];
        int dataKeyReadCount = fileStream.Read(dataKey, 0, (int)dataKeyLength);
        if (dataKeyReadCount != dataKeyLength) throw new InvalidOperationException("解析密钥失败：文件流读取不完全");

        //Console.WriteLine("KeyData 密钥(含掩码)为");
        //ConsoleExtensions.WriteByteArray(dataKey);

        //反掩码密钥
        for (int i = 0; i < dataKeyReadCount; i++)
        {
            byte b = dataKey[i];
            dataKey[i] = (byte)(b ^ 0x64);
        }

        // 解密密钥
        using (Aes aes = Aes.Create())
        {
            // 设置Aes参数
            aes.Key = CoreKey;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using (var decryptor = aes.CreateDecryptor())
            {
                var decryptedData = decryptor.TransformFinalBlock(dataKey, 0, dataKey.Length);
                decryptedKeyData = decryptedData[17..];
            }
        }


        Console.WriteLine("KeyData 密钥解析完成……");
        //ConsoleExtensions.WriteByteArray(decryptedKeyData);
    }

    /// <summary>
    /// 读取解密之后的MetaData
    /// </summary>
    /// <param name="fileStream">Ncm文件流</param>
    /// <param name="decryptedMetaData">解密后的KeyData</param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void DecryptMetaData(FileStream fileStream, out MusicMetaData decryptedMetaData)
    {
        Console.WriteLine("正在解析MetaData 密钥数据……");

        // 读取密钥长度
        byte[] keyLenght = new Byte[4];
        int readCount = fileStream.Read(keyLenght, 0, 4);
        if (readCount != 4) throw new InvalidOperationException("读取密钥长度失败：文件流读取不完全");

        // 解析密钥长度
        uint dataKeyLength = BitConverter.ToUInt32(keyLenght, 0);

        // 读取密钥
        byte[] dataKey = new byte[dataKeyLength];
        int dataKeyReadCount = fileStream.Read(dataKey, 0, (int)dataKeyLength);
        if (dataKeyReadCount != dataKeyLength) throw new InvalidOperationException("解析密钥失败：文件流读取不完全");


        // 反掩码密钥
        for (int i = 0; i < dataKeyReadCount; i++)
        {
            byte b = dataKey[i];
            dataKey[i] = (byte)(b ^ 0x63);
        }

        // Base64解码
        Span<byte> clipKey = dataKey[22..];
        string baseString = Encoding.ASCII.GetString(clipKey);
        byte[] middleKey = Convert.FromBase64String(baseString);

        byte[] decryptedByteData;
        // 解密密钥
        using (Aes aes = Aes.Create())
        {
            // 设置Aes参数
            aes.Key = MetaKey;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using (var decryptor = aes.CreateDecryptor())
            {
                var decryptedData = decryptor.TransformFinalBlock(middleKey, 0, middleKey.Length);
                decryptedByteData = decryptedData[6..];
            }
        }


        // 解析歌曲信息
        string metaData = Encoding.UTF8.GetString(decryptedByteData);
        decryptedMetaData = JsonSerializer.Deserialize<MusicMetaData>(metaData) ?? new MusicMetaData();

        //Debug： 输出格式化后的MetaData
        //Console.WriteLine(metaData);
        metaData = JsonSerializer.Serialize(
            decryptedMetaData,
            new JsonSerializerOptions { WriteIndented = true }
        );
        //Console.WriteLine(metaData);

        Console.WriteLine("MetaData解析完成……");
        //ConsoleExtensions.WriteByteArray(decryptedMetaData);
    }

    private static void GetCRC(FileStream ncmFileStream, out string crc)
    {
        byte[] buffer = new byte[4];
        int readCount = ncmFileStream.Read(buffer, 0, 4);
        if (readCount < 4) throw new InvalidOperationException();
        crc = Encoding.ASCII.GetString(buffer);

        // 跳过没用的 五个字节
        ncmFileStream.Seek(5, SeekOrigin.Current);
    }

    private static void GetImage(FileStream ncmFileStream, out byte[] imageData)
    {
        byte[] buffer = new byte[4];
        int readCount = ncmFileStream.Read(buffer, 0, 4);
        if (readCount < 4) throw new InvalidOperationException("封面长度读取失败");
        uint imageLength = BitConverter.ToUInt32(buffer);
        imageData = new byte[imageLength];
        int imageReadCount = ncmFileStream.Read(imageData, 0, (int)imageLength);
        if (imageReadCount != imageLength) throw new InvalidOperationException("封面数据读取失败");
    }
}