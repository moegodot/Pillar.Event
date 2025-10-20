namespace Pillar.Event.Test;

public class WeakEventTest
{

    private readonly WeakEvent<int, EventArgs> _weakEvent = new();

    private readonly WeakEvent<int, EventArgs> _a = new(), _n = new();
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}