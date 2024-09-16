using IoC_Framework.Core;
using Shouldly;

[assembly: FixtureLifeCycle(LifeCycle.InstancePerTestCase)]

namespace IoC_Framework.Tests;

public interface IServiceA;
public class ServiceAImplementation : IServiceA;

public class AnotherServiceAImplementation(IServiceA service)
{
    public IServiceA Service { get; } = service;
}

public class Tests
{
    [TestFixture]
    public class ContainerTests
    {
        private readonly Container container = new();

        [Test]
        public void Register_and_resolve_interface_should_return_correct_implementation()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>();

            // Act
            var result = container.Resolve<IServiceA>();

            // Assert
            result.ShouldBeOfType<ServiceAImplementation>();
        }

        [Test]
        public void Resolving_unregistered_interface_should_throw_exception()
        {
            // Act & Assert
            var exception = Should.Throw<Exception>(() => container.Resolve<IServiceA>());
            exception.Message.ShouldBe("Type IServiceA not registered");
        }

        public class ClassA
        {
            public ClassA(ClassB b) { }
        }

        public class ClassB
        {
            public ClassB(ClassA a) { }
        }

        [Test]
        public void Resolve_circular_dependency_should_throw_exception()
        {
            // Arrange
            container.Register<ClassA, ClassA>();
            container.Register<ClassB, ClassB>();

            // Act & Assert
            var exception = Should.Throw<InvalidOperationException>(() => container.Resolve<ClassA>());
            exception.Message.ShouldBe("Circular dependency detected for type ClassA");
        }

        [Test]
        public void Register_multiple_implementations_and_resolve_all_should_return_all_implementations()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>();
            container.Register<IServiceA, AnotherServiceAImplementation>();

            // Act
            var result = container.ResolveAll<IServiceA>().ToList();

            // Assert
            result.Count.ShouldBe(2);
            result.ShouldContain(x => x.GetType() == typeof(ServiceAImplementation));
            result.ShouldContain(x => x.GetType() == typeof(AnotherServiceAImplementation));
        }
    }
}