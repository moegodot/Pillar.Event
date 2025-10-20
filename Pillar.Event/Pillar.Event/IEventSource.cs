namespace Pillar.Event;

public interface IEventSource<TSender,TEventArgs>
{
    void ClearHandlers();
    void Register(Runtime.EventHandler<TSender, TEventArgs> handler);
    void Unregister(Runtime.EventHandler<TSender, TEventArgs> handler);
    IEnumerable<Exception>? Fire(TSender source, TEventArgs @event, bool ignoreError = false);
}