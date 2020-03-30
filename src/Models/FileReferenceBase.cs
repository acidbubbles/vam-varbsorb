namespace Varbsorb.Models
{
    public abstract class FileReferenceBase
    {
        public string LocalPath { get; set; }
        public string FilenameLower { get; set; }
        public string Extension { get; set; }
        public string Hash { get; set; }
    }
}