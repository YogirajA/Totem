using System;

namespace MicroservicesMessagingDemo
{
    public interface ITotemTemplate
    {
        Guid ContractId { get; set; }
        IMessageDetails Message { get; set; }
    }

    public interface IMessageDetails
    {
        Guid? Id { get; set; }
        DateTime Timestamp { get; set; }
    }
}