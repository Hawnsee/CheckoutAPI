using DAL;
using MassTransit;
using Checkout.Contracts;
using Checkout.Contracts.Events;

namespace CheckoutSaga
{
    public class CheckoutStateMachine : MassTransitStateMachine<CheckoutState>
    {
        public State PaymentProcessed { get; private set; }
        
        public State PaymentPending { get; private set; }
        
        public State Faulted { get; private set; }

        public Event<OrderCreatedIntegrationEvent> OrderCreatedEvent { get; private set; }

        public Event<PaymentProcessedIntegrationEvent> PaymentProcessedEvent { get; private set; }

        public CheckoutStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(
                () => OrderCreatedEvent, 
                x => x.CorrelateById(context => context.Message.Id)
            );

            Event(
                () => PaymentProcessedEvent, 
                x => x.CorrelateById(context => context.Message.Id)
            );
            
            Initially(
                When(OrderCreatedEvent)
                .Then(context => context.Saga.OrderId = context.Message.OrderId)
                .Publish(context => new ProcessPaymentCommand() { Id = context.Message.Id, OrderId = context.Message.OrderId })
                .TransitionTo(PaymentPending)
            );

            During(PaymentPending,
                When(PaymentProcessedEvent)
                .IfElse(context => context.Message.Success,
                    binder => binder
                    .TransitionTo(PaymentProcessed),
                    binder => binder
                    .Publish(context => new CancelOrderCommand(context.Saga.OrderId))
                    .TransitionTo(Faulted)
                )
            );
        }
    }
}