using IoC_Framework.Core;
using Shouldly;

[assembly: FixtureLifeCycle(LifeCycle.InstancePerTestCase)]

namespace IoC_Framework.Tests;

public interface IServiceA;
public interface IServiceB;
public class ServiceAImplementation : IServiceA;
public class AnotherServiceAImplementation : IServiceA;
public class ServiceBImplementation : IServiceB;

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
            exception.Message.ShouldBe("Type IServiceA not registered.");
        }

        public class CircularClassA(CircularClassB b);
        public class CircularClassB(CircularClassA a);

        [Test]
        public void Resolve_circular_dependency_should_throw_exception()
        {
            // Arrange
            container.Register<CircularClassA, CircularClassA>();
            container.Register<CircularClassB, CircularClassB>();

            // Act & Assert
            var exception = Should.Throw<InvalidOperationException>(() => container.Resolve<CircularClassA>());
            exception.Message.ShouldBe("Circular dependency detected for type CircularClassA.");
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

        [Test]
        public void Registering_the_same_interface_multiple_times_should_return_last_registration_for_single_resolve()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>();
            container.Register<IServiceA, AnotherServiceAImplementation>();

            // Act
            var result = container.Resolve<IServiceA>();

            // Assert
            result.ShouldBeOfType<AnotherServiceAImplementation>();
        }

        public class ClassWithSingleParameter(IServiceA serviceA);

        [Test]
        public void Resolve_type_with_constructor_dependencies_should_inject_dependencies()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>();
            container.Register<ClassWithSingleParameter, ClassWithSingleParameter>();

            // Act
            var result = container.Resolve<ClassWithSingleParameter>();

            // Assert
            result.ShouldNotBeNull();
        }

        public class ClassWithMultipleParameters(IServiceA serviceA, IServiceB classA);

        [Test]
        public void Resolve_type_with_multiple_constructor_dependencies_should_inject_dependencies()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>();
            container.Register<IServiceB, ServiceBImplementation>();
            container.Register<ClassWithMultipleParameters, ClassWithMultipleParameters>();

            // Act
            var result = container.Resolve<ClassWithMultipleParameters>();

            // Assert
            result.ShouldNotBeNull();
        }

        [Test]
        public void Register_without_specifying_lifetime_should_default_to_transient()
        {
            // Act
            container.Register<IServiceA, ServiceAImplementation>();

            // Assert
            var instance1 = container.Resolve<IServiceA>();
            var instance2 = container.Resolve<IServiceA>();

            instance1.ShouldNotBeSameAs(instance2);
        }

        [Test]
        public void Resolve_as_singleton_should_return_same_instance_each_time()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>(Lifetime.Singleton);

            // Act
            var instance1 = container.Resolve<IServiceA>();
            var instance2 = container.Resolve<IServiceA>();

            // Assert
            instance1.ShouldBeSameAs(instance2);
        }

        [Test]
        public void Resolve_as_transient_should_return_different_instance_each_time()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>(Lifetime.Transient);

            // Act
            var instance1 = container.Resolve<IServiceA>();
            var instance2 = container.Resolve<IServiceA>();

            // Assert
            instance1.ShouldNotBeSameAs(instance2);
        }

        [Test]
        public void Resolve_scoped_service_in_same_scope_should_return_same_instance()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>(Lifetime.Scoped);
            using var scope = container.BeginScope();

            // Act
            var instance1 = scope.Resolve<IServiceA>();
            var instance2 = scope.Resolve<IServiceA>();

            // Assert
            instance1.ShouldBeSameAs(instance2);
        }

        [Test]
        public void Resolve_scoped_service_in_different_scopes_should_return_different_instances()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>(Lifetime.Scoped);
            using var scope1 = container.BeginScope();
            using var scope2 = container.BeginScope();

            // Act
            var instance1 = scope1.Resolve<IServiceA>();
            var instance2 = scope2.Resolve<IServiceA>();

            // Assert
            instance1.ShouldNotBeSameAs(instance2);
        }

        [Test]
        public void Dispose_scope_should_clear_all_instances()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>(Lifetime.Scoped);
            var scope = container.BeginScope();
            _ = scope.Resolve<IServiceA>();

            // Act
            scope.Dispose();

            // Assert
            var exception = Should.Throw<ObjectDisposedException>(() => scope.Resolve<IServiceA>());
            exception.Message.ShouldBe("""
                                       Cannot access a disposed object.
                                       Object name: 'Scope'.
                                       """);
        }

        [Test]
        public void Resolve_singleton_service_in_different_scopes_should_return_same_instance()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>(Lifetime.Singleton);
            using var scope1 = container.BeginScope();
            using var scope2 = container.BeginScope();

            // Act
            var instance1 = scope1.Resolve<IServiceA>();
            var instance2 = scope2.Resolve<IServiceA>();

            // Assert
            instance1.ShouldBeSameAs(instance2);
        }

        [Test]
        public void Resolve_transient_service_in_same_scope_should_return_different_instances()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>(Lifetime.Transient);
            using var scope = container.BeginScope();

            // Act
            var instance1 = scope.Resolve<IServiceA>();
            var instance2 = scope.Resolve<IServiceA>();

            // Assert
            instance1.ShouldNotBeSameAs(instance2);
        }

        [Test]
        public void Resolve_scoped_service_outside_of_scope_should_throw_exception()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>(Lifetime.Scoped);

            // Act & Assert
            var exception = Should.Throw<InvalidOperationException>(() => container.Resolve<IServiceA>());
            exception.Message.ShouldBe("Scope is required for scoped lifetime.");
        }

        [Test]
        public void Nested_scopes_should_resolve_dependencies_correctly()
        {
            // Arrange
            container.Register<IServiceA, ServiceAImplementation>(Lifetime.Scoped);
            using var outerScope = container.BeginScope();
            using var innerScope = container.BeginScope();

            // Act
            var outerInstance = outerScope.Resolve<IServiceA>();
            var innerInstance = innerScope.Resolve<IServiceA>();

            // Assert
            outerInstance.ShouldNotBeSameAs(innerInstance);
        }

        public class DisposableService : IServiceA, IDisposable
        {
            public bool IsDisposed { get; private set; }
            public void Dispose() => IsDisposed = true;
        }

        [Test]
        public void Disposing_scope_should_dispose_scoped_services()
        {
            // Arrange
            container.Register<IServiceA, DisposableService>(Lifetime.Scoped);
            var scope = container.BeginScope();
            var service = (DisposableService)scope.Resolve<IServiceA>();

            // Act
            scope.Dispose();

            // Assert
            service.IsDisposed.ShouldBeTrue();
        }
    }
}