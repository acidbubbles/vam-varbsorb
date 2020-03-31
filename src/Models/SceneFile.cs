using System.Collections.Generic;

namespace Varbsorb.Models
{
    public class SceneFile
    {
        public FreeFile File { get; set; }
        public IList<SceneReference> References { get; }
        public IList<string> Missing { get; }

        public SceneFile(FreeFile file, IList<SceneReference> references, IList<string> missing)
        {
            File = file;
            References = references;
            Missing = missing;
        }
    }
}
