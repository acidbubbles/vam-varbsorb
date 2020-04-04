using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Operations;

namespace Varbsorb
{
    public class Varbsorber
    {
        public const int MaxWarnings = 20;

        private readonly IConsoleOutput _output;
        private readonly IFileSystem _fs;
        private readonly IOperationsFactory _operationsFactory;

        public Varbsorber(IConsoleOutput output, IFileSystem fs,  IOperationsFactory operationsFactory)
        {
            _output = output;
            _fs = fs;
            _operationsFactory = operationsFactory;
        }

        public async Task ExecuteAsync(string vam, string[]? include, string[]? exclude, DeleteOptions delete, VerbosityOptions verbosity, ErrorReportingOptions warnings, ExecutionOptions execution)
        {
            vam = SanitizeVamRootFolder(vam);
            var filter = BuildFilter(vam, include, exclude);

            var sw = Stopwatch.StartNew();

            var varFiles = await _operationsFactory.Get<IListVarPackagesOperation>().ExecuteAsync(vam);
            var freeFiles = await _operationsFactory.Get<IListFilesOperation>().ExecuteAsync(vam);
            var scenes = await _operationsFactory.Get<IListScenesOperation>().ExecuteAsync(vam, freeFiles, filter, warnings);
            var matches = await _operationsFactory.Get<IMatchFilesToPackagesOperation>().ExecuteAsync(varFiles, freeFiles);
            await _operationsFactory.Get<IUpdateSceneReferencesOperation>().ExecuteAsync(scenes, matches, execution);
            await _operationsFactory.Get<IDeleteMatchedFilesOperation>().ExecuteAsync(freeFiles, matches, delete, filter, verbosity, execution);
            await _operationsFactory.Get<IDeleteOrphanMorphFilesOperation>().ExecuteAsync(freeFiles, delete, filter, verbosity, execution);

            _output.WriteLine($"Cleanup complete in {sw.Elapsed.Seconds:0.00} seconds.");
        }

        public IFilter BuildFilter(string vam, string[]? include, string[]? exclude)
        {
            return Filter.From(
                include?.Select(f => SanitizeFilterPath(vam, f)).ToArray(),
                exclude?.Select(f => SanitizeFilterPath(vam, f)).ToArray());
        }

        private string SanitizeVamRootFolder(string vam)
        {
            if (string.IsNullOrWhiteSpace(vam))
            {
                if (_fs.File.Exists(_fs.Path.Combine(Environment.CurrentDirectory, "VaM.exe")))
                {
                    return Environment.CurrentDirectory;
                }
                throw new VarbsorberException("The vam parameter is required, or you can place varbsorb.exe in the same folder as VaM.exe.");
            }

            if (vam.EndsWith('/') || vam.EndsWith('\\')) vam = vam[0..^1];
            return vam;
        }

        private string SanitizeFilterPath(string vam, string f)
        {
            if (!_fs.Path.IsPathFullyQualified(f)) f = _fs.Path.Combine(vam, f);
            f = _fs.Path.GetFullPath(f);
            if (!f.StartsWith(vam)) throw new VarbsorberException($"Filter '{f}' is not within the vam folder");
            if (!f.StartsWith(_fs.Path.Combine(vam, "Saves"))) throw new VarbsorberException($"Filter '{f}' is not within the vam Saves folder");
            return f.Substring(vam.Length + 1);
        }
    }
}
