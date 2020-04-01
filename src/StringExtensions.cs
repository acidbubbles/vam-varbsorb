using System;

namespace Varbsorb
{
    public static class StringExtensions
    {
        public static string RelativeTo(this string path, string root)
        {
            if (!path.StartsWith(root, StringComparison.InvariantCultureIgnoreCase)) throw new InvalidOperationException($"Path '{path}' does not start with '{root}'");
            return path.Substring(root.Length + 1);
        }

        public static string NormalizePathSeparators(this string path)
        {
            return path.Replace('/', '\\');
        }
    }
}
