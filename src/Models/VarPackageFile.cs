namespace Varbsorb.Models
{
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
