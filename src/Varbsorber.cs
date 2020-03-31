using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Models;
using Varbsorb.Operations;

namespace Varbsorb
{
    public class Varbsorber
    {
        private readonly IConsoleOutput _output;
        private readonly IOperationsFactory _operationsFactory;

        public Varbsorber(IConsoleOutput output, IOperationsFactory operationsFactory)
        {
            _output = output;
            _operationsFactory = operationsFactory;
        }

        public async Task ExecuteAsync(string vam, bool noop)
        {
            if (string.IsNullOrWhiteSpace(vam)) throw new VarbsorberException("The vam parameter is required (please specify the Virt-A-Mate installation folder)");
            if (vam.EndsWith('/') || vam.EndsWith('\\')) vam = vam[0..^1];

            var sw = Stopwatch.StartNew();

            var varFiles = await _operationsFactory.Get<IListVarPackagesOperation>().ExecuteAsync(vam);
            var freeFiles = await _operationsFactory.Get<IListFilesOperation>().ExecuteAsync(vam);
            var scenes = await _operationsFactory.Get<IListScenesOperation>().ExecuteAsync(vam, freeFiles);
            var matches = await _operationsFactory.Get<IMatchFilesToPackagesOperation>().ExecuteAsync(varFiles, freeFiles);

            _output.WriteLine("Scan complete. Files matching a var reference:");
            var filesToDelete = new HashSet<FreeFile>();
            foreach (var match in matches.GroupBy(m => m.Package))
            {
                foreach (var file in match.SelectMany(m => m.FreeFiles).SelectMany(ff => ff.Children != null ? ff.Children.Concat(new[] { ff }) : new[] { ff }))
                {
                    filesToDelete.Add(file);
                    _output.WriteLine($"- {file.LocalPath} in {match.Key.Name.Filename} (used in {scenes.Count(s => s.References.Any(r => r.File == file))} scenes)");
                }
            }
            _output.WriteLine($"Complete. Found {matches.Count} matches in {varFiles.Count} packages and {freeFiles.Count} files in {sw.Elapsed.Seconds:0.00} seconds. Estimated space saved: {filesToDelete.Sum(f => f.Size) / 1024f:0.00}MB.");
        }
    }
}
