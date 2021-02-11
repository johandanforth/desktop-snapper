using System.Collections.Generic;

namespace Snapper.Helpers
{
    public static class IntervalHelper
    {
        public static IEnumerable<int> GetInterval()
        {
            for (var i = 5; i <= 60; i = i + 5)
                yield return i;
        }
    }
}