namespace IoC_Framework.Core;

public class Scope(Container container) : IDisposable
{
    private readonly Dictionary<Type, object> instances = new();
    private bool disposed = false;

    public T Resolve<T>() => (T)Resolve(typeof(T));

    public object Resolve(Type type)
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(Scope));

        if (instances.TryGetValue(type, out var instance))
            return instance;

        instance = container.Resolve(type, this);

        if (container.TryGetRegistration(type, out var registration) && registration?.Lifetime is Lifetime.Scoped)
            instances[type] = instance;

        return instance;
    }

    public void Dispose()
    {
        if (disposed) return;

        foreach (var instance in instances.Values.OfType<IDisposable>())
            instance.Dispose();

        disposed = true;
        instances.Clear();
    }
}