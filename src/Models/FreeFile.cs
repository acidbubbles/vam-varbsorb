using System;
using System.Collections.Generic;
using System.Linq;

namespace Varbsorb.Models
{
    public class FreeFile : FileReferenceBase
    {
        public string Path { get; set; }
        public int Size { get; set; }
        public List<FreeFile> Children { get; set; }

        public FreeFile(string path, string localPath, string filenameLower, string extension)
        {
            Path = path;
            LocalPath = localPath;
            FilenameLower = filenameLower;
            Extension = extension;
        }

        internal IEnumerable<FreeFile> SelfAndChildren()
        {
            if (Children == null) return new[] { this };
            return Children.Concat(new[] { this });
        }
    }
}
