using System.IO;

namespace Varbsorb
{
    public static class TestExtensions{
        public static string Windows(this string path)
        {
            return Path.DirectorySeparatorChar == '/' ? path.Replace('/', '\\') : path;
        }
    }
}
