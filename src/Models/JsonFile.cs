using System.Collections.Generic;

namespace Varbsorb.Models
{
    public class JsonFile
    {
        public FreeFile File { get; set; }
        public IList<SceneReference> References { get; }
        public IList<string> Missing { get; }

        public JsonFile(FreeFile file, IList<SceneReference> references, IList<string> missing)
        {
            File = file;
            References = references;
            Missing = missing;
        }
    }
}
