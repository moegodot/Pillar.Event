namespace Pillar.Event.Runtime;

public class SortedEventArgs : EventArgs
{
    public new static readonly SortedEventArgs Empty = new()
    {
        CurrentSort = EventSort.Before
    };

    public EventSort CurrentSort { get; private set; } = EventSort.Before;

    internal void NextEventSort()
    {
        if (CurrentSort == EventSort.Before)
        {
            CurrentSort = EventSort.Core;
        }
        else if (CurrentSort == EventSort.Core)
        {
            CurrentSort = EventSort.After;
        }
        else
        {
            throw new InvalidOperationException("The SortedEventArgs has been last sorted");
        }
    }
}
