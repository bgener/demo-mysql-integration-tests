using Xunit;

namespace OrderSystem.Tests.Infrastructure;

[CollectionDefinition("Optimized")]
public class OptimizedMySqlCollection : ICollectionFixture<OptimizedMySqlContainerFixture> { }
