using System.Collections.Generic;
using System.IO;
using System.Linq;
using Varbsorb.Models;

namespace Varbsorb
{
    public class Filter
    {
        public static readonly IFilter None = new NoFilter();

        public class VarOrPath
        {
            public VarPackageName? Var { get; set; }
            public string? Path { get; set; }
        }

        public static IFilter From(string vam, string[]? include, string[]? exclude)
        {
            if (include == null && exclude == null) return None;
            var includeSplit = include?.Select(f => VarPackageName.TryGet(f, out var name) ? new VarOrPath { Var = name } : new VarOrPath { Path = f }).ToList();
            var excludeSplit = exclude?.Select(f => VarPackageName.TryGet(f, out var name) ? new VarOrPath { Var = name } : new VarOrPath { Path = f }).ToList();
            return new IncludeExcludeFilter(
                includeSplit?.Where(f => !string.IsNullOrWhiteSpace(f.Path)).Select(f => f.Path!.Trim()).Tap(f => SanitizeFilterPath(vam, f)).ToArray(),
                includeSplit?.Where(f => f.Var != null).Select(f => f.Var!).ToArray(),
                excludeSplit?.Where(f => !string.IsNullOrWhiteSpace(f.Path)).Select(f => f.Path!.Trim()).ToArray(),
                excludeSplit?.Where(f => f.Var != null).Select(f => f.Var!).ToArray());
        }

        private static string SanitizeFilterPath(string vam, string f)
        {
            if (!Path.IsPathFullyQualified(f)) f = Path.Combine(vam, f);
            f = Path.GetFullPath(f);
            if (!f.StartsWith(vam)) throw new VarbsorberException($"Filter '{f}' is not within the vam folder");
            if (!f.StartsWith(Path.Combine(vam, "Saves"))) throw new VarbsorberException($"Filter '{f}' is not within the vam Saves folder");
            return f.Substring(vam.Length + 1);
        }
    }

    public class NoFilter : IFilter
    {
        public bool IsFiltered(string path) => false;
        public bool IsFiltered(VarPackageName package) => false;
        public IEnumerable<string> GetPaths() => new string[0];
    }

    public class IncludeExcludeFilter : IFilter
    {
        private readonly string[]? _includePaths;
        private readonly VarPackageName[]? _includePackages;
        private readonly string[]? _excludePaths;
        private readonly VarPackageName[]? _excludePackages;

        public IncludeExcludeFilter(string[]? include, VarPackageName[]? includePackages, string[]? exclude, VarPackageName[]? excludePackages)
        {
            _includePaths = include;
            _includePackages = includePackages;
            _excludePaths = exclude;
            _excludePackages = excludePackages;
        }

        public bool IsFiltered(string localPath)
        {
            if (_excludePaths != null && _excludePaths.Any(f => localPath.StartsWith(f))) return true;
            if (_includePaths != null && !_includePaths.Any(f => localPath.StartsWith(f))) return true;
            return false;
        }

        public bool IsFiltered(VarPackageName package)
        {
            if (_excludePackages != null)
            {
                foreach (var exclude in _excludePackages)
                {
                    if (exclude.Author == "*") continue;
                    if (exclude.Author == package.Author) return true;
                    if (exclude.Name == "*") continue;
                    if (exclude.Name == package.Name) return true;
                    if (exclude.Version == -1) continue;
                    if (exclude.Version == package.Version) return true;
                }
            }
            if (_includePackages != null)
            {
                foreach (var include in _includePackages)
                {
                    if (
                        (include.Author == "*" || include.Author == package.Author) &&
                        (include.Name == "*" || include.Name == package.Name) &&
                        (include.Version == -1 || include.Version == package.Version))
                        return false;
                }
                return true;
            }
            return false;
        }

        public IEnumerable<string> GetPaths()
        {
            if (_includePaths != null)
            {
                foreach (var path in _includePaths)
                    yield return path;
            }
            if (_excludePaths != null)
            {
                foreach (var path in _excludePaths)
                    yield return path;
            }
        }
    }

    public interface IFilter
    {
        bool IsFiltered(string localPath);
        bool IsFiltered(VarPackageName package);
        IEnumerable<string> GetPaths();
    }
}
