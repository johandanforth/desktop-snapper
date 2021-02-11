using System.Collections.Generic;

namespace Snapper.Helpers
{
    public static class ScreenSelectionHelper
    {
        public static IEnumerable<string> GetScreenSelection()
        {
            var list = new List<string>
                           {
                               "Primary Screen",
                               "All Screens",
                               "Active Window"
                           };

            return list;
        }
    }

    public class ScreenSelection
    {
        public ScreenSelection(string name, int id)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; set; }
        public int Id { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}