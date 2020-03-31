using System.IO;

namespace Varbsorb.Models
{
    public abstract class FileReferenceBase
    {
        public string LocalPath { get; set; }
        public string FilenameLower { get; set; }
        public string Extension { get; set; }

        protected FileReferenceBase(string localPath)
        {
            LocalPath = localPath;
            FilenameLower = Path.GetFileName(localPath).ToLowerInvariant();
            Extension = Path.GetExtension(FilenameLower);
        }
    }
}
