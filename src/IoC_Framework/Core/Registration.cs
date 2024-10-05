using System.Collections.Immutable;

namespace IoC_Framework.Core;

public sealed record Registration(ImmutableList<Type> Implementations, Lifetime Lifetime)
{
    public object? Instance { get; set; }

    public Registration AddImplementation(Type implementationType) => this with { Implementations = Implementations.Add(implementationType) };
}