namespace Pillar.Event;

/// <summary>
/// The `Runtime` is from <see cref="System.EventHandler"/>
/// </summary>
public sealed class Runtime
{
    public delegate void EventHandler<TSender,TEventArgs>(TSender sender, TEventArgs e);
}
