namespace NcmToMp3;

public class NoticePublisher
{
    public delegate NoticeMessage NoticePublishEventHandler(NoticeMessage noticeMessage);

    public static event NoticePublishEventHandler NoticePublished;

    public static void Publish(string message, object? sender,MessageLevel level = MessageLevel.Info)
    {
        NoticePublished(new NoticeMessage(message, sender, level));
    }
}

public record NoticeMessage(string Message, object? Publisher, MessageLevel Level)
{
    public MessageLevel Level { get; init; } = Level;
    public string Message { get; init; } = Message;
    public object? Publisher { get; init; } = Publisher;
}

public enum MessageLevel
{
    Info,
    Warning,
    Error
}