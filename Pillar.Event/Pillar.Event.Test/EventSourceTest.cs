using Pillar.Event.Runtime;

namespace Pillar.Event.Test;

public class EventSourceTest
{
    public static object[] TestedEventSource()
    {
        return
        [
            new ListEvent<object, SortedEventArgs>(),
            new WeakEvent<object, SortedEventArgs>(),
        ];
    }
    
    public static object[] TestedSortedEventSource()
    {
        return
        [
            new SortedEvent<object, TestEventArgs>(new ListEvent<object, TestEventArgs>()),
            new SortedEvent<object, TestEventArgs>(new WeakEvent<object, TestEventArgs>()),
        ];
    }

    [Test]
    [TestCaseSource(nameof(TestedEventSource))]
    public void TestFireForPrimitiveType(IEventSource<object,SortedEventArgs> source)
    {
        bool called = false;
        source.Register((sender, args) =>
        {
            Assert.That(called, Is.False);
            called = true;
            Assert.That(sender, Is.EqualTo(1));
            Assert.That(args, Is.EqualTo(SortedEventArgs.Empty));
        });
        source.Fire(1, SortedEventArgs.Empty);
        Assert.That(called,Is.True);
    }
    
    [Test]
    [TestCaseSource(nameof(TestedEventSource))]
    public void TestFireForUserType(IEventSource<object,SortedEventArgs> source)
    {
        bool called = false;
        source.Register((sender, args) =>
        {
            Assert.That(called, Is.False);
            called = true;
            Assert.That(sender, Is.EqualTo(SortedEventArgs.Empty));
            Assert.That(args, Is.EqualTo(SortedEventArgs.Empty));
        });
        source.Fire(SortedEventArgs.Empty, SortedEventArgs.Empty);
        Assert.Pass();
    }

    private static void Fail(object a,SortedEventArgs b)
    {
        Assert.Fail();
    }
    
    [Test]
    [TestCaseSource(nameof(TestedEventSource))]
    public void TestRemove(IEventSource<object,SortedEventArgs> source)
    {
        source.Register(Fail);
        source.Register(Fail);
        source.Unregister(Fail); // should remove all
        source.Fire(EventArgs.Empty, SortedEventArgs.Empty);
        Assert.Pass();
    }
    
    [Test]
    [TestCaseSource(nameof(TestedEventSource))]
    public void TestClear(IEventSource<object,SortedEventArgs> source)
    {
        source.Register(Fail);
        source.Register(Fail);
        source.ClearHandlers(); // should remove all
        source.Fire(EventArgs.Empty, SortedEventArgs.Empty);
        Assert.Pass();
    }

    private static void Throw(object obj, SortedEventArgs arg)
    {
        throw new InvalidOperationException("Thrown");
    }

    [Test]
    [TestCaseSource(nameof(TestedEventSource))]
    public void TestIgnoreError(IEventSource<object, SortedEventArgs> source)
    {
        source.Register(Throw);
        source.Register(Throw);
        var got = source.Fire(EventArgs.Empty, SortedEventArgs.Empty, true).ToArray();
        Assert.That(got, Has.Length.EqualTo(2));
        Assert.That(got[0], Has.Message.EqualTo("Thrown"));
        Assert.That(got[1], Has.Message.EqualTo("Thrown"));
    }

    [Test]
    [TestCaseSource(nameof(TestedEventSource))]
    public void TestNotIgnoreError(IEventSource<object,SortedEventArgs> source)
    {
        source.Register(Throw);
        source.Register(Throw);
        Assert.Throws<InvalidOperationException>(() =>
        {
            source.Fire(EventArgs.Empty, SortedEventArgs.Empty, false);
        });
    }

    public class TestEventArgs : SortedEventArgs
    {
        public int Value { get; set; }
    }

    [Test]
    [TestCaseSource(nameof(TestedSortedEventSource))]
    public void TestSortedEvent(IEventSource<object, TestEventArgs> source)
    {
        source.Register(((sender, args) =>
        {
            args.Value += 1;
        }));
        var args = new TestEventArgs
        {
            Value = 0
        };
        source.Fire(this,args);
        Assert.That(args.CurrentSort, Is.EqualTo(EventSort.After));
        Assert.That(args.Value, Is.EqualTo(3));
    }
}
