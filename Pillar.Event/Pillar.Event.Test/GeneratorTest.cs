using System.CodeDom.Compiler;

namespace Pillar.Event.Test;

public partial class GeneratorTest
{
    [EmitEvent][GeneratedCode("","")] private readonly WeakEvent<int?, EventArgs> _withUnderline = new();
    [EmitEvent][GeneratedCode("","")] private readonly WeakEvent<int?, EventArgs> withoutUnderLine = new();
    [EmitEvent][GeneratedCode("","")] private readonly WeakEvent<int?, EventArgs> _ = new();
    [EmitEvent] private readonly WeakEvent<EventArgs?, EventArgs> @int = new();
    [EmitEvent] protected readonly WeakEvent<EventArgs?, EventArgs> WithoutUnderLineEventAndUpper = new();
    
    [Test]
    public void GenerateTest()
    {
        // 看看我们的生成器还工作吗？
        // 看看我们生成的名字符合预期吗？
        WithUnderline += (_,_) => { };
        withoutUnderLineEvent  += (_,_) => { };
        _Event += (_,_) => { };
        intEvent += (_,_) => { };
        WithoutUnderLineEventAndUpperEvent += (_,_) => { };
    }
}