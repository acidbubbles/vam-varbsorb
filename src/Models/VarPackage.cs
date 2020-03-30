using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Varbsorb.Models
{
    public class VarPackage
    {
        public VarPackageName Name { get; set; }
        public string Path { get; set; }
        public List<VarPackageFile> Files { get; } = new List<VarPackageFile>();

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    public class VarPackageName
    {
        public static readonly Regex _extract = new Regex(@"^(?<Author>[^\.]+)\.(?<Name>[^\.]+)\.(?<Version>[0-9]+)\.var$", RegexOptions.Compiled, TimeSpan.FromSeconds(0.5));

        public string Filename { get; }
        public string Author { get; }
        public string Name { get; }
        public int Version { get; }

        public VarPackageName(string filename)
        {
            Filename = filename;
            var match = _extract.Match(filename);

            if (match.Success)
            {
                Author = match.Groups["Name"].Value;
                Name = match.Groups["Name"].Value;
                Version = int.Parse(match.Groups["Version"].Value);
            }
        }

        public override string ToString()
        {
            if (Name != null)
                return $"{Name} v{Version} by {Author} ({Filename})";
            else
                return Filename;
        }
    }

    public class VarPackageFile : FileReferenceBase
    {
        public VarPackageFile(string localPath, string filenameLower, string extension)
        {
            LocalPath = localPath;
            FilenameLower = filenameLower;
            Extension = extension;
        }
    }
}
