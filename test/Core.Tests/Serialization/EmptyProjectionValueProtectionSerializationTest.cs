﻿using ExRam.Gremlinq.Tests.Infrastructure;
using Gremlin.Net.Process.Traversal;
using static ExRam.Gremlinq.Core.GremlinQuerySource;

namespace ExRam.Gremlinq.Core.Tests
{
    public sealed class EmptyProjectionValueProtectionSerializationTest : QueryExecutionTest, IClassFixture<EmptyProjectionValueProtectionSerializationTest.EmptyProjectionValueProtectionFixture>
    {
        public sealed class EmptyProjectionValueProtectionFixture : GremlinqFixture
        {
            public EmptyProjectionValueProtectionFixture() : base(g
                .ConfigureEnvironment(_ => _
                    .ConfigureOptions(o => o.SetValue(GremlinqOption.EnableEmptyProjectionValueProtection, true))))
            {
            }
        }

        public EmptyProjectionValueProtectionSerializationTest(EmptyProjectionValueProtectionFixture fixture, ITestOutputHelper testOutputHelper) : base(
            fixture,
            new SerializingVerifier<Bytecode>(),
            testOutputHelper)
        {
        }
    }
}
