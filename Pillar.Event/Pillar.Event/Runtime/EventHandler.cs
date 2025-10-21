namespace Pillar.Event.Runtime;

public delegate void EventHandler<TSender,TEventArgs>(TSender sender, TEventArgs e);

public static class Helper
{
    public static EventHandler<TSender,TEventArgs> ExecuteWhen<TSender,TEventArgs>(
        this EventHandler<TSender,TEventArgs> handler,
        EventStage stage) where TEventArgs : StagedEventArgs
    {
        return (sender, args) =>
        {
            if (args.CurrentStage == stage)
            {
                handler(sender, args);
            }
        };
    }
}