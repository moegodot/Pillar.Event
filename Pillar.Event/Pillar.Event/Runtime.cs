namespace Pillar.Event;

public class Runtime
{
    public delegate void EventHandler<TSender,TEventArgs>(TSender sender, TEventArgs e);
}
