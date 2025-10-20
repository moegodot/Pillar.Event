using System;
using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace PillarOfPillar.Event;

[Generator]
public class EventGenerator : IIncrementalGenerator
{
	public EventGenerator()
	{
		if (!Debugger.IsAttached) 
		{ 
			//Debugger.Launch(); 
		} 
	}
	
	public const string GeneratorName = $"{nameof(PillarOfPillar)}.{nameof(Event)}.{nameof(EventGenerator)}";
	
	public const string GeneratorVersion = "1.0.0";

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
				return null;
			}

			if (!name.StartsWith("global::"))
			{
				name = $"global::{name}";
			}

			if (name != OverQualifiedAttributeName)
			{
				return null;
			}

			if (context.TargetNode is not VariableDeclaratorSyntax variableDeclaratorSyntax)
			{
				return null;
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

	private static void GenerateCode(SourceProductionContext context, Compilation compilation, 
		ImmutableArray<VariableDeclaratorSyntax> variableDeclaratorSyntaxes)
	{
		try
		{
			// Go through all filtered class declarations.
			foreach (var declarator in variableDeclaratorSyntaxes)
			{
				var fieldDeclarationSyntax = (FieldDeclarationSyntax)declarator.Parent!.Parent!;
				
				// We need to get semantic model of the class to retrieve metadata.
				var semanticModel = compilation.GetSemanticModel(fieldDeclarationSyntax.SyntaxTree);

				// Symbols allow us to get the compile-time information.
				if (semanticModel.GetDeclaredSymbol(fieldDeclarationSyntax.Parent!) is not INamedTypeSymbol fieldSymbol)
				{
					continue;
				}

				string nestedClassName = string.Empty;
				string openNestedClass = string.Empty;
				string closeNestedClass = string.Empty;
				INamedTypeSymbol? containingType = fieldSymbol.ContainingType;
				while (containingType != null)
				{
					nestedClassName = $"{containingType.Name}.{nestedClassName}";
					openNestedClass = $" partial class {containingType.Name} {{ {openNestedClass} ";
					closeNestedClass += "}";
					containingType = containingType.ContainingType;
				}
				
				var namespaceName = fieldSymbol.ContainingNamespace.ToDisplayString();

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

				var fieldName = declarator.Identifier.ValueText;

				var eventName = GetEventPropertyName(fieldName);

				var className = ((ClassDeclarationSyntax)fieldDeclarationSyntax.Parent!).Identifier.Text;

				// Build up the source code
				var code = $$"""
				             // <auto-generated/>
				             namespace {{namespaceName}}
				             {
				                 {{openNestedClass}}
				                 partial class {{className}}
				                 {
				                     [System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}","{{GeneratorVersion}}")]
				                     public event {{EventHandlerName}}<{{fullyArgumentList}}> @{{eventName}}
				                         {
				                         add{
				                             @{{fieldName}}.Register(value);
				                         }
				                         remove{
				                             @{{fieldName}}.Unregister(value);
				                         }
				                     }
				                 }
				                 {{closeNestedClass}}
				             }

				             """;

				context.AddSource($"{namespaceName}.{nestedClassName}{className}.{fieldName.Replace('@', ' ')}.event.g.cs",
					SourceText.From(code, Encoding.UTF8));
			}
		}
		catch (Exception ex)
		{
			context.Debug(ex.ToString().Replace("\n", ";;;"));
		}
	}
}
