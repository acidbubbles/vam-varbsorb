using System.Linq;

namespace Varbsorb
{
    public class Filter
    {
        public static readonly IFilter None = new NoFilter();

        public static IFilter From(string[]? include, string[]? exclude)
        {
            if (include == null && exclude == null) return None;
            include = (include ?? new string[0]).Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();
            exclude = (exclude ?? new string[0]).Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();
            if (include.Length == 0 && exclude.Length == 0) return None;
            if (include.Length == 0) return new ExcludeFilter(exclude);
            return new IncludeFilter(include, exclude);
        }
    }

    public class NoFilter : IFilter
    {
        public bool IsFiltered(string path)
        {
            return false;
        }
    }

    public class ExcludeFilter : IFilter
    {
        private readonly string[] _exclude;

        public ExcludeFilter(string[] exclude)
        {
            _exclude = exclude;
        }

        public bool IsFiltered(string path)
        {
            return _exclude.Any(f => path.StartsWith(f));
        }
    }

    public class IncludeFilter : IFilter
    {
        private readonly string[] _include;
        private readonly string[] _exclude;

        public IncludeFilter(string[] include, string[] exclude)
        {
            _include = include;
            _exclude = exclude;
        }

        public bool IsFiltered(string path)
        {
            if (_exclude.Any(f => path.StartsWith(f))) return true;
            return !_include.Any(f => path.StartsWith(f));
        }
    }

    public interface IFilter
    {
        bool IsFiltered(string localPath);
    }
}
