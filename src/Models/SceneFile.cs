using System.Collections.Generic;

namespace Varbsorb.Models
{
    public class SceneFile : FileReferenceBase
    {
        public FreeFile File { get; set; }
        public List<SceneReference> References { get; set; }

        public SceneFile(FreeFile file)
        {
            File = file;
        }
    }
}
