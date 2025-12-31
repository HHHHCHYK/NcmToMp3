using System.Text;

namespace NcmToMp3.utils;

public static class ConsoleExtensions
{
    private static StringBuilder builder = new StringBuilder();
    public static void WriteByteArray(byte[] array)
    {
        builder.Clear();
        builder.Append('[');
        for (int i = 0; i < array.Length; i++)
        {
            if(i != 0) builder.Append(',');
            builder.Append(array[i].ToString("X2"));
        }
        builder.Append(']');
        Console.WriteLine(builder);
    }


    public static void WriteByteToAscii(byte[] array)
    {
        builder.Clear();
        builder.Append('[');
        builder.Append(Encoding.ASCII.GetString(array));
        builder.Append(']');
        Console.WriteLine(builder);
    }
}