using System.Linq;

namespace Varbsorb
{
    public class Filter
    {
        public static readonly IFilter None = new NoFilter();

        public static IFilter From(string[]? filters)
        {
            if (filters == null) return None;
            var sanitized = filters.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();
            if (sanitized.Length == 0) return None;
            return new StringsFilter(sanitized);
        }
    }

    public class NoFilter : IFilter
    {
        public bool IsFiltered(string path)
        {
            return false;
        }
    }

    public class StringsFilter : IFilter
    {
        private readonly string[] _filters;

        public StringsFilter(string[] filters)
        {
            _filters = filters;
        }

        public bool IsFiltered(string path)
        {
            return _filters.Any(f => path.StartsWith(f));
        }
    }

    public interface IFilter
    {
        bool IsFiltered(string localPath);
    }
}
