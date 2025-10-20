using Microsoft.CodeAnalysis;

namespace PillarOfPillar.Event;

internal static class Helper
{
	public static void Debug(this SourceProductionContext context, string msg)
	{
		context.ReportDiagnostic(Diagnostic.Create(
									 new DiagnosticDescriptor("DEBUG0", "SOURCE_GENERATOR_DEBUG_OUTPUT", "{0}", "", DiagnosticSeverity.Warning, true),
									 null,
									 msg));
	}
}
