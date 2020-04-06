using System.Collections.Generic;

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
}
