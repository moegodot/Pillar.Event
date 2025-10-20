using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace Pillar.Event.Test;

public partial class WeakEventTest
{
    [EmitEvent] protected readonly WeakEvent<EventArgs?, EventArgs> Test = new();
    
    [Test]
    public void GeneratorTest()
    {
        // 看看我们的生成器还工作吗？
        _Event += (_,_) => { };
        TestEvent += (_,_) => { };
        intEvent += (_,_) => { };
    }
    
    [EmitEvent]
    private readonly WeakEvent<EventArgs, EventArgs> _weakEvent = new();

    [EmitEvent] private readonly WeakEvent<int, EventArgs> _a = new();
    
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
        WeakEvent += (sender, args) =>
        {
            Assert.That(called, Is.False);
            called = true;
            Assert.That(sender, Is.EqualTo(EventArgs.Empty));
            Assert.That(args, Is.EqualTo(EventArgs.Empty));
        };
        _weakEvent.Fire(EventArgs.Empty, EventArgs.Empty);
    }

    static void Fail(EventArgs sender, EventArgs args)
    {
        Assert.Fail();
    }
    
    [Test]
    public void TestRemove()
    {
        bool called = false;
        Action<EventArgs,EventArgs> @raef = (EventArgs sender,EventArgs args) =>
        {
            Assert.That(called, Is.False);
            called = true;
            Assert.Multiple(() =>
            {
                Assert.That(sender, Is.EqualTo(EventArgs.Empty));
                Assert.That(args, Is.EqualTo(EventArgs.Empty));
            });
        };
        WeakEvent += Fail;
        _weakEvent.Fire(EventArgs.Empty, EventArgs.Empty);
    }
}