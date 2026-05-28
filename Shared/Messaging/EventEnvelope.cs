namespace Shared.Messaging;

public class EventEnvelope
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = "";
    public int Version { get; set; }
    public string Payload { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}