namespace IoC_Framework.Core;

public class Container
{
    private readonly Dictionary<Type, Registration> registrations = [];
    private readonly HashSet<Type> resolvingTypes = [];

    public void Register<TInterface, TImplementation>(Lifetime lifetime = Lifetime.Transient)
    {
        if (!registrations.ContainsKey(typeof(TInterface)))
            registrations[typeof(TInterface)] = new Registration([], lifetime);

        registrations[typeof(TInterface)] = registrations[typeof(TInterface)].AddImplementation(typeof(TImplementation));
    }

    public TInterface Resolve<TInterface>()
    {
        return (TInterface)Resolve(typeof(TInterface));
    }

    public IEnumerable<TInterface> ResolveAll<TInterface>()
    {
        return ResolveAll(typeof(TInterface)).Cast<TInterface>();

    }

    private object Resolve(Type interfaceType)
    {
        if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            var itemType = interfaceType.GetGenericArguments()[0];
            return ResolveAll(itemType);
        }

        if (!registrations.TryGetValue(interfaceType, out var registration))
            throw new Exception($"Type {interfaceType.Name} not registered.");

        var implementationType = registration.Implementations.Last();

        if (registration.Lifetime == Lifetime.Singleton)
        {
            return registration.Instance ??
                   (registration.Instance = CreateInstance(implementationType));
        }

        return CreateInstance(implementationType);
    }

    private IEnumerable<object> ResolveAll(Type interfaceType)
    {
        if (!registrations.TryGetValue(interfaceType, out var registration))
            throw new Exception($"Type {interfaceType.Name} not registered.");

        return registration.Implementations.Select(CreateInstance).ToList();
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
}