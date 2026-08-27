using MassTransit;

namespace DAL
{
    public class CheckoutState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }

        public string OrderId { get; set; }

        public string? FaultMessage { get; set; }
    }
}