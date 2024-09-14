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
    }
}