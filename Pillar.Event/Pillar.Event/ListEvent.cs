using System.Diagnostics.CodeAnalysis;

namespace Pillar.Event;

/// <summary>
///     非线程安全的,使用List的事件源.
/// </summary>
/// <typeparam name="TEventArgs">事件参数类型</typeparam>
/// <typeparam name="TSender">事件发送者类型</typeparam>
public sealed class ListEvent<TSender,TEventArgs> : IEventSource<TSender,TEventArgs> where TEventArgs : EventArgs
{
    private readonly List<Runtime.EventHandler<TSender,TEventArgs>> _handlers = [];

    public void ClearHandlers()
    {
        _handlers.Clear();
    }

    public IEnumerable<Exception>? Fire(TSender source, TEventArgs @event, bool ignoreError = false)
    {
        List<Exception>? exceptions = ignoreError ? [] : null;
        foreach (var t in _handlers)
        {
            try
            {
                t(source, @event);
            }
            catch (Exception ex)
            {
                if (!ignoreError)
                    throw;
                exceptions!.Add(ex);
            }
        }

        return exceptions;
    }

    public void Register(Runtime.EventHandler<TSender,TEventArgs> handler)
    {
        _handlers.Add(handler);
    }

    public void Unregister(Runtime.EventHandler<TSender,TEventArgs> handler)
    {
        _handlers.RemoveAll(e => ReferenceEquals(e, handler));
    }
}
