using SoapJsonConversion.Middleware;
using System;
using System.Collections.Generic;
using Xunit;
using Shouldly;
using System.Linq;

namespace SoapJsonConversion.Middleware_Tests
{
    public class ServiceDescriptionExtensions_Tests
    {
        [Fact]
        public void TryGetMatchControllers_Test()
        {
            ServiceDescriptionExtensions.PathControllers["test.asmx"] = new HashSet<Type> { typeof(int) };

            ServiceDescriptionExtensions.TryGetMatchControllers("test.asmx", out IEnumerable<Type> types)
                .ShouldBeTrue();
            types.Count().ShouldBeGreaterThan(0);

        }
    }
}
