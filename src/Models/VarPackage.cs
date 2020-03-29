using System.Collections.Generic;

namespace Varbsorb
{
    public class VarPackage
    {
        public string Path { get; set; }
        public List<VarPackageFile> Files { get; } = new List<VarPackageFile>();
    }

    public class VarPackageFile
    {
        public string LocalPath { get; set; }
    }
}