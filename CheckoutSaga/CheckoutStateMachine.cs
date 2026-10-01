using DAL;
using MassTransit;
using Checkout.Contracts.Commands;
using Checkout.Contracts.Events;

namespace CheckoutSaga
{
    public class CheckoutStateMachine : MassTransitStateMachine<CheckoutState>
    {
        public State PaymentPending { get; private set; }

        public Event<OrderCreatedIntegrationEvent> OrderCreatedEvent { get; private set; }

        public Event<PaymentProcessedIntegrationEvent> PaymentProcessedEvent { get; private set; }

        public CheckoutStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(
                () => OrderCreatedEvent, 
                x => x.CorrelateById(context => context.Message.SagaCorrelationId)
            );

            Event(
                () => PaymentProcessedEvent, 
                x => x.CorrelateById(context => context.Message.SagaCorrelationId)
            );
            
            Initially(
                When(OrderCreatedEvent)
                .Then(context => context.Saga.OrderId = context.Message.OrderId)
                .Publish(context => new ProcessPaymentCommand() { SagaCorrelationId = context.Message.SagaCorrelationId, OrderId = context.Message.OrderId })
                .TransitionTo(PaymentPending)
            );

            During(PaymentPending,
                When(PaymentProcessedEvent)
                .IfElse(context => context.Message.Success,
                    binder => binder
                    .Publish(context => new SetOrderPaymentProcessedCommand(context.Saga.OrderId))
                    .Then(context =>
                                {
                                    context.Saga.PaymentResult = PaymentResult.ACCEPTED;
                                })
                    .Finalize(),
                    binder => binder
                    .Publish(context => new CancelOrderCommand(context.Saga.OrderId))
                    .Then(context => 
                                { 
                                    context.Saga.PaymentResult = PaymentResult.REJECTED;
                                })
                    .Finalize()
                )
            );
        }
    }
}