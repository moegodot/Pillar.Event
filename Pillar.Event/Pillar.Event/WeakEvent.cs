using System.Diagnostics.CodeAnalysis;

namespace Pillar.Event;

/// <summary>
///     非线程安全的,使用弱引用的事件源.
/// </summary>
/// <typeparam name="TEventArgs">事件参数类型</typeparam>
/// <typeparam name="TSender">事件发送者类型</typeparam>
public sealed class WeakEvent<TSender,TEventArgs>() : IEventSource<TSender,TEventArgs> where TEventArgs : EventArgs
{
    private readonly List<WeakReference<Runtime.EventHandler<TSender,TEventArgs>>> _handlers = [];

    public void ClearHandlers()
    {
        _handlers.Clear();
    }
    
    public IEnumerable<Exception>? Fire(TSender source, TEventArgs @event, bool ignoreError = false)
    {
        List<Exception>? exceptions = ignoreError ? [] : null;
        for (var index = 0; index < _handlers.Count; index++)
        {
            try
            {
                var handler = _handlers[index];

                if (handler.TryGetTarget(out var target))
                {
                    target.Invoke(source, @event);
                    continue;
                }

                _handlers.RemoveAt(index);
                index--;
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
        _handlers.Add(new WeakReference<Runtime.EventHandler<TSender,TEventArgs>>(handler));
    }

    public void Unregister(Runtime.EventHandler<TSender,TEventArgs> handler)
    {
        _handlers.RemoveAll(e =>
            !e.TryGetTarget(out var obj) || ReferenceEquals(obj, handler));
    }
}