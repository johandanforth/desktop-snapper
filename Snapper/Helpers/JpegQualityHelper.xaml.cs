using System.Collections.Generic;

namespace Snapper.Helpers
{
    public static class JpegQualityHelper
    {
        public static IEnumerable<int> GetQuality()
        {
            for (var i = 10; i <= 100; i = i + 10)
                yield return i;
        }
    }

}