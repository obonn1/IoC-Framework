# IoC Framework: A Learning Project

This is a learning project where I'm building a Dependency Injection (DI) container from scratch to gain a deeper understanding of how DI frameworks work under the hood.

## Overview

The DI container supports:
- **Singleton**, **Scoped**, and **Transient** lifetimes for registered services.
- **Automatic resolution** of dependencies, including constructor injection.
- **Scope management** to isolate dependencies within specific lifetimes.
- **Automatic disposal** of scoped services when a scope is disposed.

## Key Components

- **Container**: Manages service registrations and resolves dependencies.
- **Scope**: Controls the lifecycle of scoped services within a specific context.
- **Registration**: Holds the configuration for each service, including its lifetime and implementations.

## Features

- **Register and Resolve**: Register interfaces and their implementations with specified lifetimes and resolve them as needed.
- **Scoped Disposal**: Scoped services are automatically disposed when the scope is disposed.
- **Circular Dependency Detection**: Detects and prevents circular dependencies during resolution.

## Tests

The project includes unit tests to validate the functionality of:
- Basic service registration and resolution.
- Scoped and singleton lifetime behaviors.
- Proper disposal of scoped services.

## Getting Started

To run the tests, use your preferred test runner (e.g., NUnit or xUnit).

```bash
# Example with dotnet test
dotnet test
