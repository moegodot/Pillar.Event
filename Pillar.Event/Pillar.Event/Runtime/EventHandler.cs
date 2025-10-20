namespace Pillar.Event.Runtime;

public delegate void EventHandler<TSender,TEventArgs>(TSender sender, TEventArgs e);
