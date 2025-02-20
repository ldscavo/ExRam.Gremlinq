﻿using ExRam.Gremlinq.Core.Tests;
using ExRam.Gremlinq.Tests.Infrastructure;

namespace ExRam.Gremlinq.Providers.GremlinServer.Tests
{
    public sealed class IntegrationTests : QueryExecutionTest, IClassFixture<GremlinServerFixture>
    {
        public IntegrationTests(GremlinServerFixture fixture, ITestOutputHelper testOutputHelper) : base(
            fixture,
            new ExecutingVerifier(),
            testOutputHelper)
        {
        }
    }
}
