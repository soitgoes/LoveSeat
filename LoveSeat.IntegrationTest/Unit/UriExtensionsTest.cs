using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LoveSeat.Support;
using NUnit.Framework;

namespace LoveSeat.IntegrationTest.Unit
{
    [TestFixture]
    public class UriExtensionsTest
    {
        [Test]
        public void TestCombine()
        {
            new Uri("http://localhost").Combine("path").ToString().Should().Be("http://localhost/path");
            new Uri("http://localhost/toppath/").Combine("path").ToString().Should().Be("http://localhost/toppath/path");
            new Uri("http://localhost/toppath").Combine("path").ToString().Should().Be("http://localhost/toppath/path");
            new Uri("http://localhost/toppath/path2/").Combine("path").ToString().Should().Be("http://localhost/toppath/path2/path");

            new Uri("http://localhost/toppath").Combine("?query").ToString().Should().Be("http://localhost/toppath?query");
            new Uri("http://localhost/toppath/").Combine("?query").ToString().Should().Be("http://localhost/toppath?query");
            new Uri("http://localhost/toppath/").Combine("user.blah.blah:hello").ToString().Should().Be("http://localhost/toppath/user.blah.blah:hello");

        }
    }
}
