using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PillarOfPillar.Event;

internal static class Helper
{
	public static void Debug(this SourceProductionContext context, string msg)
	{
		msg = msg.Replace("\n", ";;;");
		context.ReportDiagnostic(Diagnostic.Create(
									 new DiagnosticDescriptor("DEBUG0", "SOURCE_GENERATOR_DEBUG_OUTPUT", "{0}", "",
										 DiagnosticSeverity.Error, true),
									 null,
									 msg));
	}

	public static string GetGeneratedAttribute(Type generator)
	{
		var version  = generator.Assembly.GetName().Version?.ToString() ?? "0.0.1-unknown";
		return $"[System.CodeDom.Compiler.GeneratedCode(\"{generator.FullName}\",\"{version}\")]";
	}

	public static string GetHintNameOfType(ISymbol symbol)
	{
		return symbol.ToString().Replace("@", "[at]");
	}

	public static string GetHintNameOfGenerator(Type generator)
	{
		return generator.FullName!;
	}
}
