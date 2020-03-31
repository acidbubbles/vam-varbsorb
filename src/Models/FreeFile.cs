using System;
using System.Collections.Generic;
using System.Linq;

namespace Varbsorb.Models
{
    public class FreeFile : FileReferenceBase
    {
        public string Path { get; set; }
        public string? Hash { get; set; }
        public int? Size { get; set; }
        public List<FreeFile>? Children { get; set; }

        public FreeFile(string path, string localPath)
            : base(localPath)
        {
            Path = path;
        }

        internal IEnumerable<FreeFile> SelfAndChildren()
        {
            if (Children == null) return new[] { this };
            return Children.Concat(new[] { this });
        }
    }
}
