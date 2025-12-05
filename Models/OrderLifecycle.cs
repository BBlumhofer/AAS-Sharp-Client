using System.Collections.Generic;

namespace AasSharpClient.Models;

internal enum OrderState
{
    Created,
    Planned,
    Executing,
    Suspended,
    Completed,
    Aborted,
    Error
}

internal enum OrderTransition
{
    Reset,
    Schedule,
    StartProduction,
    Suspend,
    Resume,
    EndProduction,
    Abort,
    Error,
    ReturnToCreated,
    ReturnToPlanned,
    ReturnToExecuting,
    ReturnToSuspended,
    ReturnToCompleted
}

internal static class OrderStateMapper
{
    public static OrderState FromStep(StepStatusEnum status) => status switch
    {
        StepStatusEnum.OPEN => OrderState.Created,
        StepStatusEnum.PLANNED => OrderState.Planned,
        StepStatusEnum.EXECUTING => OrderState.Executing,
        StepStatusEnum.SUSPENDED => OrderState.Suspended,
        StepStatusEnum.DONE => OrderState.Completed,
        StepStatusEnum.ABORTED => OrderState.Aborted,
        StepStatusEnum.ERROR => OrderState.Error,
        _ => OrderState.Created
    };

    public static StepStatusEnum ToStep(OrderState state) => state switch
    {
        OrderState.Created => StepStatusEnum.OPEN,
        OrderState.Planned => StepStatusEnum.PLANNED,
        OrderState.Executing => StepStatusEnum.EXECUTING,
        OrderState.Suspended => StepStatusEnum.SUSPENDED,
        OrderState.Completed => StepStatusEnum.DONE,
        OrderState.Aborted => StepStatusEnum.ABORTED,
        OrderState.Error => StepStatusEnum.ERROR,
        _ => StepStatusEnum.OPEN
    };

    public static OrderState FromAction(ActionStatusEnum status) => status switch
    {
        ActionStatusEnum.OPEN => OrderState.Created,
        ActionStatusEnum.PLANNED => OrderState.Planned,
        ActionStatusEnum.EXECUTING => OrderState.Executing,
        ActionStatusEnum.SUSPENDED => OrderState.Suspended,
        ActionStatusEnum.DONE => OrderState.Completed,
        ActionStatusEnum.ABORTED => OrderState.Aborted,
        ActionStatusEnum.ERROR => OrderState.Error,
        _ => OrderState.Created
    };

    public static ActionStatusEnum ToAction(OrderState state) => state switch
    {
        OrderState.Created => ActionStatusEnum.OPEN,
        OrderState.Planned => ActionStatusEnum.PLANNED,
        OrderState.Executing => ActionStatusEnum.EXECUTING,
        OrderState.Suspended => ActionStatusEnum.SUSPENDED,
        OrderState.Completed => ActionStatusEnum.DONE,
        OrderState.Aborted => ActionStatusEnum.ABORTED,
        OrderState.Error => ActionStatusEnum.ERROR,
        _ => ActionStatusEnum.OPEN
    };
}

internal sealed class OrderStateMachine
{
    private static readonly IReadOnlyDictionary<(OrderState State, OrderTransition Transition), OrderState> TransitionTable =
        new Dictionary<(OrderState, OrderTransition), OrderState>
        {
            {(OrderState.Created, OrderTransition.Schedule), OrderState.Planned},
            {(OrderState.Planned, OrderTransition.Reset), OrderState.Created},
            {(OrderState.Planned, OrderTransition.StartProduction), OrderState.Executing},
            {(OrderState.Executing, OrderTransition.Reset), OrderState.Planned},
            {(OrderState.Executing, OrderTransition.Suspend), OrderState.Suspended},
            {(OrderState.Suspended, OrderTransition.Resume), OrderState.Executing},
            {(OrderState.Executing, OrderTransition.EndProduction), OrderState.Completed},

            {(OrderState.Created, OrderTransition.Error), OrderState.Error},
            {(OrderState.Planned, OrderTransition.Error), OrderState.Error},
            {(OrderState.Executing, OrderTransition.Error), OrderState.Error},
            {(OrderState.Suspended, OrderTransition.Error), OrderState.Error},

            {(OrderState.Created, OrderTransition.Abort), OrderState.Aborted},
            {(OrderState.Planned, OrderTransition.Abort), OrderState.Aborted},
            {(OrderState.Executing, OrderTransition.Abort), OrderState.Aborted},
            {(OrderState.Suspended, OrderTransition.Abort), OrderState.Aborted},
            {(OrderState.Error, OrderTransition.Abort), OrderState.Aborted},

            {(OrderState.Error, OrderTransition.ReturnToCreated), OrderState.Created},
            {(OrderState.Error, OrderTransition.ReturnToPlanned), OrderState.Planned},
            {(OrderState.Error, OrderTransition.ReturnToExecuting), OrderState.Executing},
            {(OrderState.Error, OrderTransition.ReturnToSuspended), OrderState.Suspended},
            {(OrderState.Error, OrderTransition.ReturnToCompleted), OrderState.Completed}
        };

    private OrderState _state;

    public OrderStateMachine(OrderState initialState)
    {
        _state = initialState;
    }

    public OrderState State => _state;

    public bool TryApply(OrderTransition transition)
    {
        if (!CanApply(transition))
        {
            return false;
        }

        _state = TransitionTable[(_state, transition)];
        return true;
    }

    public bool CanApply(OrderTransition transition) => TransitionTable.ContainsKey((_state, transition));

    public void ForceSet(OrderState state) => _state = state;

    public static OrderTransition? FindTransition(OrderState current, OrderState target)
    {
        foreach (var entry in TransitionTable)
        {
            if (entry.Key.State == current && entry.Value == target)
            {
                return entry.Key.Transition;
            }
        }

        return null;
    }
}
