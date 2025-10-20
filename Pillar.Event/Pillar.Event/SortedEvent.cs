using Pillar.Event.Runtime;

namespace Pillar.Event;

public class SortedEvent<TSender,TEventArgs> : IEventSource<TSender,TEventArgs> where TEventArgs : SortedEventArgs
{
    public IEventSource<TSender,TEventArgs> Source { get; }

    public SortedEvent(IEventSource<TSender, TEventArgs> source)
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
        if (@event.CurrentSort != EventSort.Before)
        {
            throw new InvalidOperationException("SortedEventArgs.CurrentSort != Expected EventSort");
        }
        IEnumerable<Exception>? exceptions = ignoreError ? [] : null!;
        
        var errs = Source.Fire(source, @event, ignoreError);
        exceptions = exceptions?.Concat(errs!);
        @event.NextEventSort();
        
        errs = Source.Fire(source, @event, ignoreError);
        exceptions = exceptions?.Concat(errs!);
        @event.NextEventSort();

        return exceptions;
    }
}