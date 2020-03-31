using System.Collections.Generic;

namespace Varbsorb.Models
{
    public class SceneFile
    {
        public FreeFile File { get; set; }
        public IList<SceneReference> References { get; }

        public SceneFile(FreeFile file, IList<SceneReference> references)
        {
            File = file;
            References = references;
        }
    }
}
