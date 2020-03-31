using System.Collections.Generic;

namespace Varbsorb.Models
{
    public class FreeFilePackageMatch
    {
        public VarPackage Package { get; }
        public VarPackageFile PackageFile { get; }
        public IList<FreeFile> FreeFiles { get; }

        public FreeFilePackageMatch(VarPackage package, VarPackageFile packageFile, IList<FreeFile> freeFiles)
        {
            Package = package;
            PackageFile = packageFile;
            FreeFiles = freeFiles;
        }
    }
}
