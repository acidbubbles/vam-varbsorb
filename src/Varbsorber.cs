using System;
using System.Diagnostics;
using System.IO.Abstractions;
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

        public Varbsorber(IConsoleOutput output, IFileSystem fs, IOperationsFactory operationsFactory)
        {
            _output = output;
            _fs = fs;
            _operationsFactory = operationsFactory;
        }

        public async Task ExecuteAsync(string vam, string[]? include, string[]? exclude, DeleteOptions delete, VerbosityOptions verbosity, ErrorReportingOptions warnings, ExecutionOptions execution)
        {
            vam = SanitizeVamRootFolder(vam);
            var filter = Filter.From(vam, include, exclude);

            var sw = Stopwatch.StartNew();

            var varFiles = await _operationsFactory.Get<IScanVarPackagesOperation>().ExecuteAsync(vam, filter);
            var freeFiles = await _operationsFactory.Get<IScanFilesOperation>().ExecuteAsync(vam);
            var matches = await _operationsFactory.Get<IMatchFilesToPackagesOperation>().ExecuteAsync(varFiles, freeFiles);
            var scenes = await _operationsFactory.Get<IScanJsonFilesOperation>().ExecuteAsync(vam, freeFiles, filter, warnings);
            await _operationsFactory.Get<IUpdateJsonFileReferencesOperation>().ExecuteAsync(scenes, matches, execution);
            await _operationsFactory.Get<IDeleteMatchedFilesOperation>().ExecuteAsync(freeFiles, matches, delete, filter, verbosity, execution);
            await _operationsFactory.Get<IDeleteOrphanMorphFilesOperation>().ExecuteAsync(freeFiles, delete, filter, verbosity, execution);

            _output.WriteLine($"Cleanup complete in {sw.Elapsed.Milliseconds / 1000f:0.00} seconds.");
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
    }
}
