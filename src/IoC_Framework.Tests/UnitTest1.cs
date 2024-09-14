using IoC_Framework.Core;
using Shouldly;

[assembly: FixtureLifeCycle(LifeCycle.InstancePerTestCase)]

namespace IoC_Framework.Tests;

public interface IService { }
public class ServiceImplementation : IService { }
public class AnotherService(IService service)
{
    public IService Service { get; } = service;
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
            container.Register<IService, ServiceImplementation>();

            // Act
            var result = container.Resolve<IService>();

            // Assert
            result.ShouldBeOfType<ServiceImplementation>();
        }

        [Test]
        public void Resolving_unregistered_interface_should_throw_exception()
        {
            // Act & Assert
            var exception = Should.Throw<Exception>(() => container.Resolve<IService>());
            exception.Message.ShouldBe("Type IService not registered");
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
    }
}