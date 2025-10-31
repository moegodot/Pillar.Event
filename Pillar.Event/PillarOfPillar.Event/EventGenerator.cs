using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PillarOfPillar.Event;

[Generator]
public class EventGenerator : IIncrementalGenerator
{
	
	public string GeneratedCodeAttribute { get; }
	
	public string GeneratorHitName { get; }
	
	public EventGenerator()
	{
		GeneratedCodeAttribute = Helper.GetGeneratedCodeAttribute(typeof(EventGenerator));
		GeneratorHitName = Helper.GetHintNameOfGenerator(typeof(EventGenerator));
		if (!Debugger.IsAttached) 
		{ 
			//Debugger.Launch(); 
		} 
	}

	public const string OverQualifiedAttributeName = "global::Pillar.Event.EmitEventAttribute";
	
	public const string FullyQualifiedMetadataName = "Pillar.Event.EmitEventAttribute";

	public const string EventHandlerName = "Pillar.Event.Runtime.EventHandler";
	
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
				FullyQualifiedMetadataName,
				(_, _) => true,
				(syntaxContext, _) => GetClassDeclarationForSourceGen(syntaxContext)
			).Where(t => t is not null)
			.Select((t, _) => t!);
		context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
									 (ctx, t) => GenerateCode(ctx, t.Left, t.Right));
	}

	/// <summary>
	/// Checks whether the Node is annotated with the [Report] attribute and maps syntax context to the specific node type (ClassDeclarationSyntax).
	/// </summary>
	/// <param name="context">Syntax context, based on CreateSyntaxProvider predicate</param>
	/// <returns>The specific cast and whether the attribute was found.</returns>
	private static VariableDeclaratorSyntax? GetClassDeclarationForSourceGen(
		GeneratorAttributeSyntaxContext context)
	{
		// Go through all attributes of the class.
		foreach (var attributeSyntax in context.Attributes)
		{
			var name = attributeSyntax?.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			
			if (name is null)
			{
				continue;
			}

			if (!name.StartsWith("global::"))
			{
				name = $"global::{name}";
			}

			if (name != OverQualifiedAttributeName)
			{
				continue;
			}

			if (context.TargetNode is not VariableDeclaratorSyntax variableDeclaratorSyntax)
			{
				continue;
			}

			return variableDeclaratorSyntax;
		}

		return null;
	}

	public static string GetEventPropertyName(string fieldName)
	{
		if (fieldName.StartsWith("_") && fieldName.Length != 1)
		{
			var trimmedName = fieldName.TrimStart('_');
			return $"{char.ToUpperInvariant(trimmedName[0])}{trimmedName.Substring(1)}";
		}

		return $"{fieldName}Event";
	}

	private void GenerateCode(SourceProductionContext context, Compilation compilation, 
		ImmutableArray<VariableDeclaratorSyntax> variableDeclaratorSyntaxes)
	{
		try
		{
			// Go through all filtered class declarations.
			foreach (var declarator in variableDeclaratorSyntaxes)
			{
				var fieldDeclarationSyntax = (FieldDeclarationSyntax)declarator.Parent!.Parent!;
				
				var semanticModel = compilation.GetSemanticModel(declarator.SyntaxTree);

				var declaratorSymbol = semanticModel.GetDeclaredSymbol(declarator!)!;

				var variable = fieldDeclarationSyntax.Declaration;

				var genericNames = variable.ChildNodes().Where(node => node is GenericNameSyntax).ToArray();

				if (genericNames.Length != 1)
				{
					throw new Exception("Can not emit events that GenericNameSyntax.Length != 1");
				}

				var genericName = (GenericNameSyntax)genericNames.First()!;
				var arguments = genericName.TypeArgumentList.ChildNodes()
					.OfType<TypeSyntax>()
					.Select(node => new Tuple<ISymbol?, SyntaxNode>(semanticModel.GetSymbolInfo(node).Symbol, node))
					.SkipWhile(symbol => symbol.Item1 is null)
					.Select(symbol => $"{symbol.Item1!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}");

				var fullyArgumentList = string.Join(",", arguments);

				var declaratorName = declarator.Identifier.ValueText;

				var eventName = GetEventPropertyName(declaratorName);

				CSharpBuilder builder = new();
				builder.EnterNamespace(declaratorSymbol);
				builder.EnterNamedType(declaratorSymbol);
				
				builder.PutBody(
					$$"""
					{{GeneratedCodeAttribute}}
					public event {{EventHandlerName}}<{{fullyArgumentList}}> @{{eventName}}
					{
					    add
					    {
					        @{{declaratorName}}.Register(value);
					    }
					    remove
					    {
					        @{{declaratorName}}.Unregister(value);
					    }
					}
					"""
					);

				context.AddSource($"{GeneratorHitName}.{Helper.GetHintNameOfType(declaratorSymbol)}.g.cs",
					SourceText.From(builder.ToString(), Encoding.UTF8));
			}
		}
		catch (Exception ex)
		{
			context.Debug(ex.ToString().Replace("\n", ";;;"));
		}
	}
}
