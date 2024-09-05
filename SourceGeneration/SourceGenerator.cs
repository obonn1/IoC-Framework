using Microsoft.CodeAnalysis;

namespace IoC_Framework.SourceGeneration;

[Generator]
public class SourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        var source = 
        """
            // Auto-generated code for DI
            public static class GeneratedContainer
            {
                public static void Initialize() { /*...*/ }
            }
        """;

        context.AddSource("GeneratedContainer.cs", source);
    }
}