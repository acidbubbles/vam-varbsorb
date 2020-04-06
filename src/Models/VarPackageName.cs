using System;
using System.Text.RegularExpressions;

namespace Varbsorb.Models
{
    public class VarPackageName
    {
        public static readonly Regex _extract = new Regex(@"^(?<Author>([^\.]+)|(\*))\.(?<Name>([^\.]+|\*))\.(?<Version>([0-9]+|\*))\.var$", RegexOptions.Compiled, TimeSpan.FromSeconds(0.5));

        public string Filename { get; }
        public string Author { get; }
        public string Name { get; }
        public int Version { get; }

        public static bool TryGet(string filename, out VarPackageName? name)
        {
            var match = _extract.Match(filename);
            if (!match.Success)
            {
                name = null;
                return false;
            }

            name = new VarPackageName(filename, match.Groups["Author"].Value, match.Groups["Name"].Value, match.Groups["Version"].Value == "*" ? -1 : int.Parse(match.Groups["Version"].Value));
            return true;
        }

        public VarPackageName(string filename, string author, string name, int version)
        {
            Filename = filename;
            Author = author;
            Name = name;
            Version = version;
        }

        public override string ToString()
        {
            if (Name != null)
                return $"{Name} v{Version} by {Author} ({Filename})";
            else
                return Filename;
        }
    }
}
