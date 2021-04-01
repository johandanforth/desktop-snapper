using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Snapper.Tests
{
    [TestClass]
    public class CalendarUnitTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var datestring = "2011-03-11";
            var date = DateTime.Parse(datestring);
        }
    }
}