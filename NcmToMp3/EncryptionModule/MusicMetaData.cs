using System.Diagnostics.CodeAnalysis;
// ReSharper disable ClassNeverInstantiated.Global

namespace NcmToMp3.EncryptionModule;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class MusicMetaData
{
    public string? musicId { get; set; }
    public string? musicName { get; set; }
    public List<List<string>>? artist { get; set; }
    public string? albumId { get; set; }
    public string? album { get; set; }
    public string? albumPicDocId { get; set; }
    public string? albumPic { get; set; }
    public long? bitrate { get; set; }
    public string? mp3DocId { get; set; }
    public long? duration { get; set; }
    public string? mvId { get; set; }
    public string[]? alias { get; set; }
    public string[]? transNames { get; set; }
    public string? format { get; set; }
    public long? fee { get; set; }
    public double? volumeDelta { get; set; }
    public Privilege? privilege { get; set; }
}


public class Privilege
{
    public long flag {get; set;}
}