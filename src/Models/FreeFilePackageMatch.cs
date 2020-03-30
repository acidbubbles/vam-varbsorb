using System.Collections.Generic;

namespace Varbsorb
{
    public class FreeFilePackageMatch
    {
        public VarPackage Package { get; set; }
        public VarPackageFile PackageFile { get; set; }
        public IList<FreeFile> FreeFiles { get; set; }
    }
}