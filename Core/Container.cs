namespace IoC_Framework.Core;

public class Container
{
    private readonly Dictionary<Type, Type> registrations = new();

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
        if (!registrations.ContainsKey(interfaceType))
            throw new Exception($"Type {interfaceType.Name} not registered");

        var implementationType = registrations[interfaceType];
        var constructor = implementationType.GetConstructors().First();

        var parameters = constructor.GetParameters()
            .Select(parameter => Resolve(parameter.ParameterType))
            .ToArray();

        var instance = Activator.CreateInstance(implementationType, parameters);

        if (instance is null)
            throw new Exception($"Failed to create instance of {implementationType.Name}");

        return instance;
    }
}