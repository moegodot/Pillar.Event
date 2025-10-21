using Pillar.Event.Runtime;

namespace Pillar.Event.Test;

public class EventSourceTest
{
    public static object[] TestedEventSource()
    {
        return
        [
            new ListEvent<object, StagedEventArgs>(),
            new WeakEvent<object, StagedEventArgs>(),
        ];
    }
    
    public static object[] TestedPrimitiveEventSource()
    {
        return
        [
            new ListEvent<int, StagedEventArgs>(),
            new WeakEvent<int, StagedEventArgs>(),
        ];
    }
    
    public static object[] TestedSortedEventSource()
    {
        return
        [
            new StagedEvent<object, TestEventArgs>(new ListEvent<object, TestEventArgs>()),
            new StagedEvent<object, TestEventArgs>(new WeakEvent<object, TestEventArgs>()),
        ];
    }

    [Test]
    [TestCaseSource(nameof(TestedEventSource))]
    public void TestFireForPrimitiveType(IEventSource<object,StagedEventArgs> source)
    {
        bool called = false;
        source.Register((sender, args) =>
        {
            Assert.That(called, Is.False);
            called = true;
            Assert.That(sender, Is.EqualTo(this));
            Assert.That(args, Is.EqualTo(StagedEventArgs.Empty));
        });
        var errs = source.Fire(this, StagedEventArgs.Empty, false);
        Assert.That(called,Is.True);
        Assert.That(errs, Is.Null);
    }
    
    [Test]
    [TestCaseSource(nameof(TestedPrimitiveEventSource))]
    public void TestFireForUserType(IEventSource<int,StagedEventArgs> source)
    {
        bool called = false;
        source.Register((sender, args) =>
        {
            Assert.That(called, Is.False);
            called = true;
            Assert.That(sender, Is.EqualTo(1));
            Assert.That(args, Is.EqualTo(StagedEventArgs.Empty));
        });
        var errs = source.Fire(1, StagedEventArgs.Empty, false);
        Assert.That(called,Is.True);
        Assert.That(errs, Is.Null);
    }

    private static void Fail(object a,StagedEventArgs b)
    {
        Assert.Fail();
    }
    
    [Test]
    [TestCaseSource(nameof(TestedEventSource))]
    public void TestRemove(IEventSource<object,StagedEventArgs> source)
    {
        source.Register(Fail);
        source.Register(Fail);
        source.Unregister(Fail); // should remove all
        source.Fire(EventArgs.Empty, StagedEventArgs.Empty,false);
        Assert.Pass();
    }
    
    [Test]
    [TestCaseSource(nameof(TestedEventSource))]
    public void TestClear(IEventSource<object,StagedEventArgs> source)
    {
        source.Register(Fail);
        source.Register(Fail);
        source.ClearHandlers(); // should remove all
        source.Fire(EventArgs.Empty, StagedEventArgs.Empty,false);
        Assert.Pass();
    }

    private static void Throw(object obj, StagedEventArgs arg)
    {
        throw new InvalidOperationException("Thrown");
    }

    [Test]
    [TestCaseSource(nameof(TestedEventSource))]
    public void TestIgnoreError(IEventSource<object, StagedEventArgs> source)
    {
        source.Register(Throw);
        source.Register(Throw);
        var got = source.Fire(EventArgs.Empty, StagedEventArgs.Empty, true)?.ToArray();
        Assert.That(got, Is.Not.Null);
        Assert.That(got, Has.Length.EqualTo(2));
        Assert.That(got[0], Has.Message.EqualTo("Thrown"));
        Assert.That(got[1], Has.Message.EqualTo("Thrown"));
    }

    [Test]
    [TestCaseSource(nameof(TestedEventSource))]
    public void TestNotIgnoreError(IEventSource<object,StagedEventArgs> source)
    {
        source.Register(Throw);
        source.Register(Throw);
        Assert.Throws<InvalidOperationException>(() =>
        {
            source.Fire(EventArgs.Empty, StagedEventArgs.Empty, false);
        });
    }

    public class TestEventArgs : StagedEventArgs
    {
        public int Value { get; set; }
    }

    [Test]
    [TestCaseSource(nameof(TestedSortedEventSource))]
    public void TestStagedEvent(IEventSource<object, TestEventArgs> source)
    {
        source.Register(((sender, args) =>
        {
            args.Value += 1;
        }));
        var args = new TestEventArgs
        {
            Value = 0
        };
        var errors = source.Fire(this,args,false);
        Assert.That(errors,Is.Null);
        Assert.That(args.CurrentStage, Is.EqualTo(EventStage.After));
        Assert.That(args.Value, Is.EqualTo(3));
    }

    [Test]
    [TestCaseSource(nameof(TestedSortedEventSource))]
    public void TestStagedEventNotIgnoreErrors(IEventSource<object, TestEventArgs> source)
    {
        source.Register(Throw);
        var e = new TestEventArgs();
        Assert.Throws<InvalidOperationException>(() =>
        {
            source.Fire(this, e, false);
        });
        Assert.That(e.CurrentStage,Is.EqualTo(EventStage.Before));
    }
    
    [Test]
    [TestCaseSource(nameof(TestedSortedEventSource))]
    public void TestStagedEventIgnoreErrors(IEventSource<object, TestEventArgs> source)
    {
        source.Register(Throw);
        IEnumerable<Exception>? exceptions = null;
        var e = new TestEventArgs();
        Assert.DoesNotThrow(() =>
        {
            exceptions = source.Fire(this, e, true);
        });
        Assert.That(e.CurrentStage,Is.EqualTo(EventStage.After));
        var arr = exceptions?.ToArray();
        Assert.That(arr ,Is.Not.Null);
        Assert.That(arr, Has.Length.EqualTo(3));
        Assert.That(arr[0], Is.TypeOf<InvalidOperationException>());
        Assert.That(arr[1], Is.TypeOf<InvalidOperationException>());
        Assert.That(arr[2], Is.TypeOf<InvalidOperationException>());
    }
}
