namespace Pillar.Event.Test;

public partial class ListEventTest
{
    [EmitEvent]
    private readonly ListEvent<EventArgs, EventArgs> _weakEvent = new();

    [EmitEvent]
    private readonly ListEvent<int?, EventArgs> _a = new(), _b = new();

    [Test]
    public static void Pass()
    {
        Assert.Pass();
    }
    
    [Test]
    public void TestFireForPrimitiveType()
    {
        bool called = false;
        A += (sender, args) =>
        {
            Assert.That(called, Is.False);
            called = true;
            Assert.That(sender, Is.EqualTo(1));
            Assert.That(args, Is.EqualTo(EventArgs.Empty));
        };
        _a.Fire(1, EventArgs.Empty);
    }
    
    [Test]
    public void TestFireForUserType()
    {
        bool called = false;
        _weakEvent.Fire(EventArgs.Empty, EventArgs.Empty);
    }
}