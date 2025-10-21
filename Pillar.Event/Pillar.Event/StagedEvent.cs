using Pillar.Event.Runtime;

namespace Pillar.Event;

public sealed class StagedEvent<TSender,TEventArgs> : IEventSource<TSender,TEventArgs> where TEventArgs : StagedEventArgs
{
    public IEventSource<TSender,TEventArgs> Source { get; }

    public StagedEvent(IEventSource<TSender, TEventArgs> source)
    {
        Source = source;
    }

    public void ClearHandlers()
    {
        Source.ClearHandlers();
    }

    public void Register(EventHandler<TSender, TEventArgs> handler)
    {
        Source.Register(handler);
    }

    public void Unregister(EventHandler<TSender, TEventArgs> handler)
    {
        Source.Unregister(handler);
    }

    public IEnumerable<Exception>? Fire(TSender source, TEventArgs @event, bool ignoreError = false)
    {
        if (@event.CurrentStage != EventStage.Before)
        {
            throw new ArgumentException("SortedEventArgs.CurrentStage != Expected EventStage.Before");
        }
        IEnumerable<Exception>? exceptions = ignoreError ? [] : null!;
        
        var errs = Source.Fire(source, @event, ignoreError);
        exceptions = exceptions?.Concat(errs!);
        @event.NextEventStage();
        
        errs = Source.Fire(source, @event, ignoreError);
        exceptions = exceptions?.Concat(errs!);
        @event.NextEventStage();
        
        errs = Source.Fire(source, @event, ignoreError);
        exceptions = exceptions?.Concat(errs!);

        return exceptions;
    }
}