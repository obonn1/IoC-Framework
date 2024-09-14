namespace IoC_Framework.Core;

public class Container
{
    private readonly Dictionary<Type, Type> registrations = [];
    private readonly HashSet<Type> resolvingTypes = [];

    public void Register<TInterface, TImplementation>()
    {
        registrations[typeof(TInterface)] = typeof(TImplementation);
    }

    public TInterface Resolve<TInterface>()
    {
        return (TInterface)Resolve(typeof(TInterface));
    }

    private object Resolve(Type interfaceType)
    {
        if (resolvingTypes.Contains(interfaceType))
            throw new InvalidOperationException($"Circular dependency detected for type {interfaceType.Name}");

        if (!registrations.TryGetValue(interfaceType, out var implementationType))
            throw new Exception($"Type {interfaceType.Name} not registered");

        resolvingTypes.Add(interfaceType);

        try
        {
            var constructor = implementationType.GetConstructors().First();

            var parameters = constructor.GetParameters()
                .Select(parameter => Resolve(parameter.ParameterType))
                .ToArray();

            var instance = Activator.CreateInstance(implementationType, parameters);

            return instance ?? throw new Exception($"Failed to create instance of {implementationType.Name}");
        }
        finally
        {
            resolvingTypes.Remove(interfaceType);
        }
    }
}