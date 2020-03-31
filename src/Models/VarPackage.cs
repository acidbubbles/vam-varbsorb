using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Varbsorb.Models
{
    public class VarPackage
    {
        public VarPackageName Name { get; }
        public string Path { get; }
        public IList<VarPackageFile> Files { get; }

        public VarPackage(VarPackageName name, string path, IList<VarPackageFile> files)
        {
            Name = name;
            Path = path;
            Files = files;
        }

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
            var match = _extract.Match(filename);
            if (!match.Success) throw new VarbsorberException($"Invalid var package name: '{filename}'");

            Filename = filename;
            Author = match.Groups["Author"].Value;
            Name = match.Groups["Name"].Value;
            Version = int.Parse(match.Groups["Version"].Value);
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
        public string Hash { get; }

        public VarPackageFile(string localPath, string hash)
            : base(localPath)
        {
            Hash = hash;
        }
    }
}
