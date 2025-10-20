using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace Pillar.Event.Test;

public sealed class WeakEventTest
{
    [Test]
    public void TestFireForPrimitiveType()
    {
        WeakEvent<int,EventArgs> weakEvent = new();
        bool called = false;
        weakEvent.Register((sender, args) =>
        {
            Assert.That(called, Is.False);
            called = true;
            Assert.That(sender, Is.EqualTo(1));
            Assert.That(args, Is.EqualTo(EventArgs.Empty));
        });
        weakEvent.Fire(1, EventArgs.Empty);
    }
    
    [Test]
    public void TestFireForUserType()
    {
        WeakEvent<EventArgs,EventArgs> weakEvent = new();
        bool called = false;
        weakEvent.Register((sender, args) =>
        {
            Assert.That(called, Is.False);
            called = true;
            Assert.That(sender, Is.EqualTo(EventArgs.Empty));
            Assert.That(args, Is.EqualTo(EventArgs.Empty));
        });
        weakEvent.Fire(EventArgs.Empty, EventArgs.Empty);
        Assert.Pass();
    }

    static void Fail(EventArgs sender, EventArgs args)
    {
        Assert.Fail();
    }
    
    [Test]
    public void TestRemove()
    {
        WeakEvent<EventArgs,EventArgs> weakEvent = new();
        weakEvent.Register(Fail);
        weakEvent.Register(Fail);
        weakEvent.Unregister(Fail); // should remove all
        weakEvent.Fire(EventArgs.Empty, EventArgs.Empty);
        Assert.Pass();
    }
    
    [Test]
    public void TestClear()
    {
        WeakEvent<EventArgs,EventArgs> weakEvent = new();
        weakEvent.Register(Fail);
        weakEvent.Register(Fail);
        weakEvent.ClearAllHandlers(); // should remove all
        weakEvent.Fire(EventArgs.Empty, EventArgs.Empty);
        Assert.Pass();
    }
}