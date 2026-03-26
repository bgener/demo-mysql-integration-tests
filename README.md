# MySQL Integration Tests in .NET - Demo

This demo project accompanies the blog article **"MySQL Integration Tests in .NET: From One Container Per Test to a Shared Baseline"**. It demonstrates how to build fully isolated MySQL integration tests with Testcontainers in .NET, progressing from one container per test class to a shared baseline architecture.

## What This Demo Shows

- **Simple approach**: One MySQL container per test class using `IAsyncLifetime` directly
- **Shared container approach**: One container shared across all test classes, with database-per-class isolation
- **Baseline Docker image**: Pre-initialized MySQL image that skips cold start initialization
- **TCP-only SQL execution**: All SQL runs through MySqlConnector over TCP (no Docker exec overhead)
- **DELIMITER parsing**: Schema files with stored procedures are parsed into wire-protocol-compatible statements
- **Batch assertions**: Multiple result sets from a single round-trip using `ExecuteBatchAsync`

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or any Docker-compatible runtime)
- [.NET 8+](https://dotnet.microsoft.com/download)

## Quick Start

```bash
# Clone the repository
git clone https://github.com/bgener/demo-mysql-integration-tests.git
cd demo-mysql-integration-tests

# Restore and build
dotnet restore
dotnet build
```

## Building the Baseline Image

The baseline image bakes a fully initialized MySQL data directory into the Docker image, reducing container startup from ~15 seconds to ~3 seconds.

```bash
docker build -t mysql-test-baseline:latest ./baselines/
```

If you skip this step, the tests will fall back to `mysql:8.0` automatically. It works, just slower.

## Running Tests

```bash
# Run ALL tests (requires Docker)
dotnet test

# Run only the simple approach (one container per class)
dotnet test --filter "Category=Simple"

# Run only the shared container approach
dotnet test --filter "Category=MySQL Integration Tests"
```

## How It Works

### Simple Approach (OrderRepositoryTests.cs)

Each test class creates its own `MySqlContainer`. The container starts before the first test, runs the schema, and is destroyed after the last test. Simple but expensive at scale (~15s per class).

### Shared Container Approach (Shared/ directory)

1. **MySqlContainerFixture** starts one MySQL container for the entire test collection
2. **MySqlTestBase** creates a unique database (`db_<guid>`) per test class via `IAsyncLifetime`
3. Schema is loaded via cached TCP statements (parsed once, reused for every class)
4. Each test class gets full isolation with minimal overhead (~500ms per class)

### Baseline Image (baselines/)

The multi-stage Dockerfile initializes MySQL with the full schema during the Docker build. The resulting image has a pre-populated data directory, so MySQL skips initialization at container start (~3s instead of ~15s).

## Blog Post

Read the full article: [MySQL Integration Tests in .NET: From One Container Per Test to a Shared Baseline](https://blog.bgener.nl/blog/mysql-integration-tests-testcontainers-dotnet/)
