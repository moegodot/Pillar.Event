namespace Pillar.Event.Runtime;

public class StagedEventArgs : EventArgs
{
    public new static readonly StagedEventArgs Empty = new();

    public EventStage CurrentStage { get; protected set; } = EventStage.Before;

    internal void NextEventStage()
    {
        if (CurrentStage == EventStage.Before)
        {
            CurrentStage = EventStage.Core;
        }
        else if (CurrentStage == EventStage.Core)
        {
            CurrentStage = EventStage.After;
        }
        else
        {
            throw new InvalidOperationException("The SortedEventArgs has been last sorted");
        }
    }
}
