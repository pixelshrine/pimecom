using Shared.Messaging;

namespace Pim.Application.Messaging;

public interface IEventBus
{
    Task Publish(EventEnvelope evt);
}
