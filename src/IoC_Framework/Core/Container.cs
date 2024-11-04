namespace IoC_Framework.Core;

public class Container
{
    private readonly Dictionary<Type, Registration> registrations = [];
    private readonly HashSet<Type> resolvingTypes = [];

    public Scope BeginScope() => new(this);

    public void Register<TInterface, TImplementation>(Lifetime lifetime = Lifetime.Transient)
    {
        var interfaceType = typeof(TInterface);

        if (!registrations.ContainsKey(interfaceType))
            registrations[interfaceType] = new Registration([], lifetime);

        registrations[interfaceType] = registrations[interfaceType].AddImplementation(typeof(TImplementation));
    }

    public TInterface Resolve<TInterface>(Scope? scope = null) => (TInterface)Resolve(typeof(TInterface), scope);

    public IEnumerable<TInterface> ResolveAll<TInterface>()
    {
        return ResolveAll(typeof(TInterface)).Cast<TInterface>();

    }

    public object Resolve(Type interfaceType, Scope? scope = null)
    {
        if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            var itemType = interfaceType.GetGenericArguments()[0];
            return ResolveAll(itemType, scope);
        }

        if (!registrations.TryGetValue(interfaceType, out var registration))
        {
            if (interfaceType is { IsAbstract: false, IsInterface: false })
                return CreateInstance(interfaceType);

            throw new Exception($"Type {interfaceType.Name} not registered.");
        }

        var implementationType = registration.Implementations.Last();

        return registration.Lifetime switch
        {
            Lifetime.Singleton => registration.Instance ?? (registration.Instance = CreateInstance(implementationType)),
            Lifetime.Scoped when scope is null => throw new InvalidOperationException("Scope is required for scoped lifetime."),
            Lifetime.Scoped => scope.Resolve(implementationType),
            _ => CreateInstance(implementationType)
        };
    }

    private IEnumerable<object> ResolveAll(Type interfaceType, Scope? scope = null)
    {
        if (!registrations.TryGetValue(interfaceType, out var registration))
            throw new Exception($"Type {interfaceType.Name} not registered.");

        return [.. registration.Implementations.Select(type => Resolve(type, scope))];
    }

    private object CreateInstance(Type implementationType)
    {
        if (!resolvingTypes.Add(implementationType))
            throw new InvalidOperationException($"Circular dependency detected for type {implementationType.Name}.");

        try
        {
            var constructor = implementationType.GetConstructors().First();
            var parameters = constructor.GetParameters()
                .Select(parameter => Resolve(parameter.ParameterType))
                .ToArray();

            var instance = Activator.CreateInstance(implementationType, parameters);
            return instance ?? throw new Exception($"Failed to create instance of {implementationType.Name}.");
        }
        finally
        {
            resolvingTypes.Remove(implementationType);
        }
    }

    public bool TryGetRegistration(Type type, out Registration? registration)
    {
        return registrations.TryGetValue(type, out registration);
    }
}