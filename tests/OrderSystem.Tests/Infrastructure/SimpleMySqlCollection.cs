using Xunit;

namespace OrderSystem.Tests.Infrastructure;

[CollectionDefinition("Simple")]
public class SimpleMySqlCollection : ICollectionFixture<SimpleMySqlContainerFixture> { }
